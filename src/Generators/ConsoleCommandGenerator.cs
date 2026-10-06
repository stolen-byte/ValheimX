// SPDX-License-Identifier: GPL-3.0-or-later
namespace StolenByte.ValheimX.Generators;

// =============================================================================
[Generator(LanguageNames.CSharp)]
public sealed class ConsoleCommandGenerator : IIncrementalGenerator
{
	const string RunMethod = "Run";
	const string DefaultExecute = "Execute";
	const string CommandBinder = "CommandBinder";

	private static readonly Dictionary<SpecialType, string?> ArgumentTypes = new() {
		{ SpecialType.System_String, null },
		{ SpecialType.System_Int32, "StringParser.ParseInt" },
		{ SpecialType.System_Int64, "StringParser.ParseLong" },
		{ SpecialType.System_Single, "StringParser.ParseFloat" },
		{ SpecialType.System_Double, "StringParser.ParseDouble" },
		{ SpecialType.System_Boolean, "StringParser.ParseBool" },
	};

	public static readonly string ValidArgTypesString = string.Join(",", [
		"string",
		"int",
		"long",
		"float",
		"double",
		"bool"
	]);

	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		var results = context.SyntaxProvider
			.ForAttributeWithMetadataName(
				TypeNames.Internal.ConsoleCommandAttribute,
				predicate: static (node, _) => node is ClassDeclarationSyntax,
				transform: GetCommand
			)
			.NonNull();

		context.ReportDiagnostics(results.Select(static (r, _) => r.Diagnostics));

		var commands = results.Select(static (r, _) => r.Value).NonNull();

		var runInfo = commands
			.Select(static (cmd, _) => new RunInfo(cmd.Container, cmd.Methods.Exec));

		context.RegisterSourceOutput(
			runInfo,
			static (ctx, info) => GenerateCommand(ctx, in info)
		);

		var buildInfo = context.AnalyzerConfigOptionsProvider
			.Select(static (opts, _) => BuildInfo.From(opts));

		var addInfo = commands
			.Select(static (cmd, ct) => new AddInfo(
				cmd.Container,
				cmd.Name,
				BuildCommandDescription(cmd.Description, cmd.Methods.Exec.Args, ct),
				cmd.Methods.TabComplete,
				cmd.Props
			));

		context.RegisterSourceOutput(
			buildInfo.Combine(addInfo.Collect()),
			static (ctx, data) => {
				var (build, infos) = data;
				GenerateCommandBinder(ctx, build, infos);
			}
		);
	}

	static string BuildCommandDescription(string description, EquatableArray<CommandArg> execArgs, CancellationToken ct)
	{
		var builder = new StringBuilder("\"");

		foreach (ref readonly var arg in execArgs.AsSpan()) {
			ct.ThrowIfCancellationRequested();

			builder.Append('[');

			if (arg.DefaultValue is string value) {
				builder.Append(arg.Name);
				if (!string.Equals(value, "null", StringComparison.OrdinalIgnoreCase)) {
					builder.Append('=').Append(value.Unquote());
				}
			} else {
				builder.Append('*').Append(arg.Name);
			}

			builder.Append("] ");
		}

		if (!string.IsNullOrWhiteSpace(description)) {
			if (!execArgs.IsEmpty)
				builder.Append("- ");
			builder.Append(description);
		}

		return builder.Append("\"").ToString();
	}

	static Result<Command?>? GetCommand(GeneratorAttributeSyntaxContext ctx, CancellationToken ct)
	{
		ct.ThrowIfCancellationRequested();
		if (ctx.TargetSymbol is not INamedTypeSymbol cls)
			return null;

		var diagnostics = new List<Diagnostic>();
		Command? result = null;

		if (GetCommandInfo(cls, ctx.Attributes[0], diagnostics, ct) is { } info) {
			if (GetMethods(ctx.SemanticModel, cls, info, diagnostics, ct) is { } methods) {
				result = new(
					cls.ClassRef(),
					info.Name,
					info.Description,
					methods,
					info.Props
				);
			}
		}

		return new(result, diagnostics);
	}

	static CommandInfo? GetCommandInfo(
		INamedTypeSymbol container,
		AttributeData attr,
		List<Diagnostic> diagnostics,
		CancellationToken ct)
	{
		string name;
		string desc;

		if (!container.IsStatic) {
			diagnostics.Add(Diagnostics.TypeNotStatic(container));
			return null;
		}

		var nArgs = attr.ConstructorArguments.Length;
		if (nArgs > 1) {
			name = attr.ConstructorArguments[0].ToCode();
			desc = (string)attr.ConstructorArguments[1].Value!; // we don't want this quoted
		} else {
			name = container.Name.ToLowerInvariant().Quoted();
			if (nArgs > 0) {
				desc = (string)attr.ConstructorArguments[0].Value!;
			} else {
				desc = string.Empty;
			}
		}

		var props = new CommandProps();
		string execMethod = DefaultExecute;
		string? tabMethod = null;

		foreach (var arg in attr.NamedArguments) {
			ct.ThrowIfCancellationRequested();

			var value = arg.Value;
			if (value.Kind == TypedConstantKind.Error)
				return null;

			switch (arg.Key) {
				case "Execute":
					execMethod = (string)value.Value!;
					break;
				case "TabComplete":
					tabMethod = (string)value.Value!;
					break;
				case nameof(CommandProps.IsCheat):
					props = props with { IsCheat = true };
					break;
				case nameof(CommandProps.IsNetwork):
					props = props with { IsNetwork = true };
					break;
				case nameof(CommandProps.IsSecret):
					props = props with { IsSecret = true };
					break;
				case nameof(CommandProps.IsDev):
					props = props with { IsDev = true };
					break;
				case nameof(CommandProps.IsRemote):
					props = props with { IsRemote = true };
					break;
				case nameof(CommandProps.ServerOnly):
					props = props with { ServerOnly = true };
					break;
				case nameof(CommandProps.AdminOnly):
					props = props with { AdminOnly = true };
					break;
			}
		}

		return new(name, desc, execMethod, tabMethod, props);
	}

	static MethodInfo? GetMethods(
		SemanticModel sm,
		INamedTypeSymbol container,
		CommandInfo info,
		List<Diagnostic> diagnostics,
		CancellationToken ct)
	{
		var terminalType = sm.GetTypeByName("Terminal");
		var stringListType = sm.GetSymbolForType(typeof(List<string>));
		ExecInfo? exec = null;
		string? tabComplete = null;

		foreach (var method in container.GetMembers().OfType<IMethodSymbol>()) {
			ct.ThrowIfCancellationRequested();

			if (method.Name == info.ExecMethod) {
				if (exec is not null) {
					diagnostics.Add(Diagnostics.MultipleMethodsDefined(method));
					return null;
				}

				if (!ValidateExecMethod(method, terminalType, diagnostics, ct, out exec)) {
					return null;
				}
			} else if (method.Name == info.TabMethod) {
				if (tabComplete is not null) {
					diagnostics.Add(Diagnostics.MultipleMethodsDefined(method));
					return null;
				}

				if (!ValidateTabMethod(method, stringListType, diagnostics, out tabComplete)) {
					return null;
				}
			}
		}

		if (exec is null) {
			diagnostics.Add(Diagnostics.MissingExecMethod(info.ExecMethod, container));
			return null;
		}

		if (info.TabMethod is not null && tabComplete is null)
			diagnostics.Add(Diagnostics.MissingTabCompleteMethod(info.TabMethod, container));

		return new(tabComplete, exec.Value);
	}

	static bool ValidateExecMethod(
		IMethodSymbol method,
		INamedTypeSymbol ctxType,
		List<Diagnostic> diagnostics,
		CancellationToken ct,
		[NotNullWhen(true)] out ExecInfo? result)
	{
		result = null;

		if (!method.IsStatic) {
			diagnostics.Add(Diagnostics.MethodNotStatic(method));
			return false;
		}

		if (!IsValidReturnType(method.ReturnType)) {
			diagnostics.Add(Diagnostics.ExecMethodInvalidReturnType(method));
			return false;
		}

		if (method.Parameters.Length == 0 || !SymbolEqualityComparer.Default.Equals(method.Parameters[0].Type, ctxType)) {
			diagnostics.Add(Diagnostics.ExecMethodMissingCtxParameter(method, ctxType));
			return false;
		}

		if (GetCommandArgs(method, diagnostics, ct) is not { } args) {
			return false;
		}

		result = new(method.Name, method.ReturnsVoid, args);
		return true;
	}

	static bool ValidateTabMethod(
		IMethodSymbol method,
		INamedTypeSymbol returnType,
		List<Diagnostic> diagnostics,
		[NotNullWhen(true)] out string? result)
	{
		result = null;

		if (!method.IsStatic) {
			diagnostics.Add(Diagnostics.MethodNotStatic(method));
			return false;
		}

		if (method.Parameters.Length != 0) {
			diagnostics.Add(Diagnostics.TabCompleteInvalidArgs(method));
			return false;
		}

		if (!SymbolEqualityComparer.Default.Equals(method.ReturnType, returnType)) {
			diagnostics.Add(Diagnostics.TabCompleteInvalidReturnType(method, returnType));
			return false;
		}

		result = method.Name;
		return true;
	}

	static EquatableArray<CommandArg>? GetCommandArgs(
		IMethodSymbol method,
		List<Diagnostic> diagnostics,
		CancellationToken ct)
	{
		var args = new List<CommandArg>();
		var index = 0;

		foreach (var arg in method.Parameters.Skip(1)) {
			ct.ThrowIfCancellationRequested();

			if (arg.Kind == SymbolKind.ErrorType)
				return null;

			var argType = (INamedTypeSymbol)arg.Type;
			if (argType.IsNullable()) {
				if (argType.TypeArguments.FirstOrDefault() is INamedTypeSymbol inner) {
					argType = inner;
				}
			}

			if (!ArgumentTypes.TryGetValue(argType.SpecialType, out var parser)) {
				diagnostics.Add(Diagnostics.ExecMethodInvalidArgumentType(arg, method.Name));
				return null;
			}

			args.Add(new(index, arg.Name, $"arg{index}", parser, FormatDefaultValue(arg, argType)));
			++index;
		}

		return args.ToEquatableArray();
	}

	public static string? FormatDefaultValue(IParameterSymbol param, INamedTypeSymbol type)
	{
		if (param.HasExplicitDefaultValue) {
			var ret = SymbolDisplay.FormatPrimitive(param.ExplicitDefaultValue, true, false)!;
			if (type.SpecialType == SpecialType.System_Single)
				ret += "f"; // annoying

			return ret;
		}
		return null;
	}

	static void GenerateCommand(SourceProductionContext ctx, ref readonly RunInfo info)
	{
		ctx.CancellationToken.ThrowIfCancellationRequested();
		var builder = new CodeBuilder();
		var className = info.Container.ToDisplayString();
		var exec = info.Exec;

		builder
			.Header()
			.Line()
			.Namespace(info.Container.Namespace)
			.Line()
			.BeginClass(info.Container.Visibility, info.Container.Name, isStatic: true, isPartial: true)
				.BeginFunctionF(Accessibility.Internal, "static object? {0}(global::{1} args)", RunMethod, TypeNames.Valheim.ConsoleEventArgs)
					.BeginBlock("try")
						.For(exec.Args.AsSpan(), (builder, ref readonly arg) => {
							var getMethod = arg.DefaultValue is null ? "GetRequired" : "Get";

							builder.BeginStatementF("var {0} = args.{1}({2}, {3}", arg.VarName, getMethod, arg.Index, arg.Name.Quoted());
							if (arg.Parser is string p)
								builder.AppendFormat(", {0}", p);

							if (arg.DefaultValue is string def && !string.Equals(def, "null", StringComparison.OrdinalIgnoreCase)) {
								builder.AppendFormat(") ?? {0}", arg.DefaultValue!);
							} else {
								builder.Append(')');
							}

							builder.EndStatement();
						})
						.With(in exec, (builder, ref readonly exec) => {
							if (exec.DiscardReturn) {
								builder
									.BeginStatement()
										.With(in exec, FormatCall)
									.EndStatement()
									.Statement("return null");
							} else {
								builder
									.BeginStatement("return ")
										.With(in exec, FormatCall)
									.EndStatement();
							}
						})
					.EndBlock(false)
					.BeginBlock("catch (Exception ex)")
						.Statement("return $\"{args[0]}: {ex.Message}\"")
					.EndBlock()
				.EndFunction()
			.EndClass();

		ctx.AddSource($"{className}.g.cs", SourceText.From(builder.ToString(), Encoding.UTF8));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	static void FormatCall(CodeBuilder builder, ref readonly ExecInfo info)
	{
		builder.AppendFormat("{0}(args.Context", info.Method);
		foreach (ref readonly var arg in info.Args.AsSpan())
			builder.Append(", ").Append(arg.VarName);
		builder.Append(')');
	}

	static void GenerateCommandBinder(SourceProductionContext ctx, BuildInfo build, ImmutableArray<AddInfo> commands)
	{
		ctx.CancellationToken.ThrowIfCancellationRequested();
		var builder = new CodeBuilder();

		builder
			.Header()
			.Line()
			.Namespace(build.RootNamespace)
			.Line()
			.BeginClass(Accessibility.Internal, CommandBinder, isStatic: true, isPartial: false)
			.BeginFunction(Accessibility.Internal, "static void RegisterCommands()")
				.For(commands.AsSpan(), (builder, ref readonly cmd) => {
					var className = cmd.Container.ToDisplayString();

					builder
						.LineF("new global::{0}(", TypeNames.Valheim.ConsoleCommand)
						.Indent()
							.LineF("{0},", cmd.Name)
							.LineF("{0},", cmd.Description)
							.LineF("global::{0}.{1},", className, RunMethod)
							.BeginLine("optionsFetcher: ")
								.IfNotNull(
									cmd.TabMethodName,
									(builder, name) => builder.AppendFormat("global::{0}.{1}", className, name),
									(builder) => builder.Append("null")
								)
							.EndLine(",")
							.LineF("isCheat: {0},", cmd.Props.IsCheat.AsString())
							.LineF("isNetwork: {0},", cmd.Props.IsNetwork.AsString())
							.LineF("isSecret: {0},", cmd.Props.IsSecret.AsString())
							.LineF("hideBehindDevCommands: {0},", cmd.Props.IsDev.AsString())
							.LineF("remoteCommand: {0},", cmd.Props.IsRemote.AsString())
							.LineF("onlyServer: {0},", cmd.Props.ServerOnly.AsString())
							.LineF("onlyAdmin: {0}", cmd.Props.AdminOnly.AsString())
						.Dedent()
						.Statement(")");
				})
			.EndFunction()
			.EndClass();

		ctx.AddSource($"{CommandBinder}.g.cs", SourceText.From(builder.ToString(), Encoding.UTF8));
	}

	static bool IsValidReturnType(ITypeSymbol type)
	{
		return type.SpecialType switch {
			SpecialType.System_Void or SpecialType.System_String or SpecialType.System_Boolean => true,
			_ => false,
		};
	}

	// =============================================================================
	record CommandInfo(
		string Name,
		string Description,
		string ExecMethod,
		string? TabMethod,
		CommandProps Props
	);

	readonly record struct CommandProps(
		bool IsCheat,
		bool IsNetwork,
		bool IsSecret,
		bool IsDev,
		bool IsRemote,
		bool ServerOnly,
		bool AdminOnly
	);

	readonly record struct CommandArg(
		int Index,
		string Name,
		string VarName,
		string? Parser,
		string? DefaultValue
	);

	readonly record struct ExecInfo(
		string Method,
		bool DiscardReturn,
		EquatableArray<CommandArg> Args
	);

	readonly record struct MethodInfo(
		string? TabComplete,
		ExecInfo Exec
	);

	record Command(
		ClassRef Container,
		string Name,
		string Description,
		MethodInfo Methods,
		CommandProps Props
	);

	readonly record struct RunInfo(
		ClassRef Container,
		ExecInfo Exec
	);

	readonly record struct AddInfo(
		ClassRef Container,
		string Name,
		string Description,
		string? TabMethodName,
		CommandProps Props
	);
}
