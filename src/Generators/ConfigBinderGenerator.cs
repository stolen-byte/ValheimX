// SPDX-License-Identifier: GPL-3.0-or-later
namespace StolenByte.ValheimX.Generators;

// =============================================================================
[Generator(LanguageNames.CSharp)]
public sealed class ConfigBinderGenerator : IIncrementalGenerator
{
	const string BindClass = "ConfigBinder";
	const string BindMethod = "BindConfigs";
	const string BindAllMethod = "BindAll";
	const string SectionParameter = "section";

	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		var bindings = context.SyntaxProvider
			.ForAttributeWithMetadataName(
				TypeNames.Internal.BindConfigAttribute,
				predicate: static (_, _) => true,
				transform: GetBinding
			)
			.NonNull();

		context.RegisterSourceOutput(
			bindings,
			static (ctx, result) => {
				result.ReportDiagnostics(ctx);
				GenerateBindingMethod(ctx, result.Value);
			}
		);

		var buildInfo = context.AnalyzerConfigOptionsProvider
			.Select(static (opts, _) => BuildInfo.From(opts));

		var targets = bindings
			.Select(static (r, _) => new BindingTarget(r.Value.Container, r.Value.Section))
			.Collect();

		context.RegisterSourceOutput(
			buildInfo.Combine(targets),
			static (ctx, data) => {
				var (build, targets) = data;
				GenerateBindingClass(ctx, build, targets);
			}
		);
	}

	static Result<ConfigBinding>? GetBinding(
		GeneratorAttributeSyntaxContext ctx,
		CancellationToken ct)
	{
		ct.ThrowIfCancellationRequested();

		if (ctx.TargetSymbol is not INamedTypeSymbol container)
			return null;

		string? section = null;
		if (ctx.Attributes[0].ConstructorArguments.FirstOrDefault() is { Value: { } } arg)
			section = arg.ToCode();

		var diagnostics = new List<Diagnostic>();
		var entries = GetBindingEntries(ctx.SemanticModel, container, diagnostics, ct);
		return new(new(container.ClassRef(), container.IsStatic, section, entries), diagnostics.ToEquatableArray());
	}

	static EquatableArray<ConfigEntry> GetBindingEntries(
		SemanticModel ctx,
		INamedTypeSymbol container,
		IList<Diagnostic> diagnostics,
		CancellationToken ct)
	{
		var entries = new List<ConfigEntry>();
		var attributeType = ctx.GetTypeByName(TypeNames.Internal.ConfigValueAttribute);
		var configEntryType = ctx.GetTypeByName(TypeNames.BepIn.ConfigEntry);

		foreach (var member in container.GetMembers()) {
			ct.ThrowIfCancellationRequested();

			if (member.GetMemberType() is not INamedTypeSymbol memberType)
				continue;

			if (!SymbolEqualityComparer.Default.Equals(memberType.OriginalDefinition, configEntryType))
				continue;

			if (memberType.TypeArguments.FirstOrDefault() is not { } configType)
				continue;

			if (!member.IsStatic) {
				diagnostics.Add(Diagnostics.TypeNotStatic(member));
				continue;
			}

			if (member.IsReadOnly()) {
				diagnostics.Add(Diagnostics.ReadOnlyConfigValue(member));
				continue;
			}

			var configAttr = member
				.GetAttributes()
				.FirstOrDefault((attr) => SymbolEqualityComparer.Default.Equals(attr.AttributeClass, attributeType));

			var key = member.Name;
			var desc = string.Empty;
			ConfigProps props;
			if (configAttr is not null && configAttr.ConstructorArguments.Length > 1) {
				key = configAttr.ConstructorArguments[0].ToCode();
				desc = configAttr.ConstructorArguments[1].ToCode();

				if (!GetConfigProps(member, configType, configAttr.NamedArguments, diagnostics, ct, out var cProps))
					continue;

				props = cProps.Value;
			} else {
				diagnostics.Add(Diagnostics.MissingConfigValueAttribute(member));
				props = new(GetDefaultValue(configType));
			}

			entries.Add(new(configType.ToDisplayString(), member.Name, key, desc, props));
		}

		return entries.ToEquatableArray();
	}

	static bool GetConfigProps(
		ISymbol member,
		ITypeSymbol configType,
		ImmutableArray<KeyValuePair<string, TypedConstant>> args,
		IList<Diagnostic> diagnostics,
		CancellationToken ct,
		[NotNullWhen(true)] out ConfigProps? props)
	{
		string? defaultValue = null;
		string? advanced = null;
		string? sectionOverride = null;
		string? onChange = null;
		string? min = null;
		string? max = null;
		string? percentage = null;
		int? order = null;

		props = null;
		foreach (var arg in args) {
			ct.ThrowIfCancellationRequested();

			var value = arg.Value;
			if (value.Kind == TypedConstantKind.Error)
				return false;

			switch (arg.Key) {
				case nameof(ConfigProps.Default): {
					if (!SymbolEqualityComparer.Default.Equals(value.Type, configType)) {
						diagnostics.Add(Diagnostics.InvalidParameterType(arg.Key, member, configType));
						return false;
					}
					defaultValue = value.ToCode();
					break;
				}
				case nameof(ConfigProps.Advanced): {
					advanced = value.ToCode();
					break;
				}
				case nameof(ConfigProps.Order): {
					order = (int)value.Value!;
					break;
				}
				case nameof(ConfigProps.OnChange): {
					onChange = (string)value.Value!;
					break;
				}
				case nameof(ValueRange.Min): {
					if (!GetRangeComponent(arg.Key, configType, member, value, diagnostics, out min))
						return false;
					break;
				}
				case nameof(ValueRange.Max): {
					if (!GetRangeComponent(arg.Key, configType, member, value, diagnostics, out max))
						return false;
					break;
				}
				case nameof(ValueRange.Percentage): {
					percentage = value.ToCode();
					break;
				}
				case "Section": {
					sectionOverride = value.ToCode();
					break;
				}
			}
		}

		ValueRange? range = null;
		if (min is null && max is not null) {
			min = GetDefaultValue(configType);
		} else if (min is not null && max is null) {
			max = $"{configType.ToDisplayString()}.MaxValue";
		}

		if (min is not null && max is not null)
			range = new(min, max, configType.ToDisplayString(), percentage);

		if (defaultValue is null) {
			if (min is not null)
				defaultValue = min;
			else
				defaultValue = GetDefaultValue(configType);
		}

		props = new(defaultValue, advanced, order, sectionOverride, onChange, range);
		return true;
	}

	static bool GetRangeComponent(
		string name,
		ITypeSymbol configType,
		ISymbol prop,
		TypedConstant value,
		IList<Diagnostic> diagnostics,
		[NotNullWhen(true)] out string? result)
	{
		result = null;

		if (!configType.IsNumeric()) {
			diagnostics.Add(Diagnostics.InvalidRangeType(prop, configType));
			return false;
		}

		if (!SymbolEqualityComparer.Default.Equals(value.Type, configType)) {
			diagnostics.Add(Diagnostics.InvalidParameterType(name, prop, configType));
			return false;
		}

		result = value.ToCode();
		return true;
	}

	static string GetDefaultValue(ITypeSymbol type)
	{
		return type.SpecialType switch {
			SpecialType.System_Boolean => "true",
			SpecialType.System_String => string.Empty.Quoted(),
			_ => type.IsValueType ? "default" : "null"
		};
	}

	static void GenerateBindingMethod(SourceProductionContext ctx, ConfigBinding target)
	{
		ctx.CancellationToken.ThrowIfCancellationRequested();
		var builder = new CodeBuilder();
		var containerFullName = target.Container.ToDisplayString();

		builder
			.Header()
			.Line()
			.Namespace(target.Container.Namespace)
			.Line()
			.Usings("System")
			.Line()
			.BeginClass(target.Container.Visibility, target.Container.Name, isPartial: true, isStatic: target.IsStatic)
				.BeginFunctionF(
					Accessibility.Internal,
					"static void {0}(global::{1} cfg, string {2})",
					BindMethod,
					TypeNames.BepIn.ConfigFile,
					SectionParameter);

		foreach (var entry in target.Entries) {
			var range = entry.Props.Range;
			int? order = entry.Props.Order switch {
				-1 => null,
				int i => 999 - i,
			};

			builder
				.LineF("global::{0}.{1} = cfg.Bind<{2}>(", containerFullName, entry.Name, entry.Type)
				.Indent()
					.LineF("{0},", entry.Props.SectionOverride ?? SectionParameter)
					.LineF("{0},", entry.Key)
					.LineF("{0},", entry.Props.Default)
					.LineF("new global::{0}(", TypeNames.BepIn.ConfigDescription)
					.Indent()
						.LineF("{0},", entry.Description)
						.IfNotNull(
							in range,
							static (builder, ref readonly range) => {
								builder.LineF(
									"new global::{0}<{1}>({2}, {3}),",
									TypeNames.BepIn.AcceptableValueRange,
									range.Type,
									range.Min,
									range.Max);
							},
							static (builder) => {
								builder.Line("null,");
							}
						)
						.LineF("new global::{0} {{", TypeNames.Internal.ConfigManagerAttributes)
						.Indent()
							.LineF("IsAdvanced = {0},", entry.Props.Advanced)
							.IfNotNull(
								in range,
								static (builder, ref readonly range) => builder.LineF("ShowRangeAsPercent = {0},", range.Percentage)
							)
							.IfNotNull(
								order,
								static (builder, value) => builder.LineF("Order = {0}", value)
							)
						.EndBlock()
					.Dedent().Line(")")
				.Dedent().Statement(")");

			if (entry.Props.OnChange is string handlerName) {
				builder.StatementF(
					"global::{0}.{1}.SettingChanged += global::{0}.{2}",
					containerFullName,
					entry.Name,
					handlerName
				);
			}
		}

		builder
			.EndFunction()
			.EndClass();

		ctx.AddSource($"{containerFullName}.g.cs", SourceText.From(builder.ToString(), Encoding.UTF8));
	}

	static void GenerateBindingClass(
		SourceProductionContext ctx,
		BuildInfo build,
		ImmutableArray<BindingTarget> targets)
	{
		ctx.CancellationToken.ThrowIfCancellationRequested();
		var builder = new CodeBuilder();

		builder
			.Header()
			.Line()
			.Namespace(build.RootNamespace)
			.Line()
			.Usings("System")
			.Line()
			.BeginClass(Accessibility.Internal, BindClass, isStatic: true, isPartial: false)
				.BeginFunctionF(
					Accessibility.Internal,
					"static void {0}(this global::{1} cfg)",
					BindAllMethod,
					TypeNames.BepIn.ConfigFile
				);

		if (!targets.IsEmpty) {
			builder
				.Statement("var oldSet = cfg.SaveOnConfigSet")
				.BeginBlock("try")
				.Statement("cfg.SaveOnConfigSet = false")
				.Line();

			foreach (ref readonly var target in targets.AsSpan()) {
				builder.StatementF(
					"global::{0}.{1}(cfg, {2})",
					target.Container.ToDisplayString(),
					BindMethod,
					target.Section ?? target.Container.Name.Quoted()
				);
			}

			builder
				.Line()
				.Statement("cfg.Save()")
				.EndBlock(false)
				.BeginBlock("finally")
					.Statement("cfg.SaveOnConfigSet = oldSet")
				.EndBlock();
		}

		builder
			.EndFunction()
			.EndClass();

		ctx.AddSource($"{BindClass}.g.cs", SourceText.From(builder.ToString(), Encoding.UTF8));
	}

	// =============================================================================
	readonly record struct BindingTarget(ClassRef Container, string? Section);

	record ConfigBinding(
		ClassRef Container,
		bool IsStatic,
		string? Section,
		EquatableArray<ConfigEntry> Entries
	);

	record ConfigEntry(
		string Type,
		string Name,
		string Key,
		string Description,
		ConfigProps Props
	);

	readonly record struct ValueRange
	{
		public readonly string Min;
		public readonly string Max;
		public readonly string Type;
		public readonly string Percentage;

		public ValueRange(string min, string max, string type, string? percentage = null)
		{
			Min = min;
			Max = max;
			Type = type;
			Percentage = percentage ?? "false";
		}
	}

	readonly record struct ConfigProps
	{
		public readonly string Default;
		public readonly string Advanced;
		public readonly string? SectionOverride;
		public readonly string? OnChange;
		public readonly int Order;
		public readonly ValueRange? Range;

		public ConfigProps(
			string defaultValue,
			string? advanced = null,
			int? order = null,
			string? sectionOverride = null,
			string? onChange = null,
			ValueRange? range = null)
		{
			Default = defaultValue;
			Advanced = advanced ?? "false";
			SectionOverride = sectionOverride;
			OnChange = onChange;
			Order = order ?? -1;
			Range = range;
		}
	}
}
