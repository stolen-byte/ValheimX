// SPDX-License-Identifier: GPL-3.0-or-later
namespace StolenByte.ValheimX.Generators;

// =============================================================================
[Generator(LanguageNames.CSharp)]
public sealed class StaticInitGenerator : IIncrementalGenerator
{
	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		var targets = context.SyntaxProvider
			.ForAttributeWithMetadataName(
				TypeNames.Internal.StaticInitAttribute,
				predicate: static (node, _) => node is ClassDeclarationSyntax,
				transform: GetTarget
			)
			.NonNull();

		var buildInfo = context.AnalyzerConfigOptionsProvider
			.Select(static (opts, _) => BuildInfo.From(opts));

		context.RegisterSourceOutput(
			buildInfo.Combine(targets.Collect()),
			static (ctx, data) => GenerateStaticInit(ctx, data.Left, data.Right)
		);
	}

	static ClassRef? GetTarget(GeneratorAttributeSyntaxContext ctx, CancellationToken ct)
	{
		ct.ThrowIfCancellationRequested();

		if (ctx.TargetSymbol is not INamedTypeSymbol cls)
			return null;

		return cls.ClassRef();
	}

	static void GenerateStaticInit(SourceProductionContext ctx, BuildInfo build, ImmutableArray<ClassRef> targets)
	{
		ctx.CancellationToken.ThrowIfCancellationRequested();
		var builder = new CodeBuilder();

		builder
			.Header()
			.Line()
			.Namespace(build.RootNamespace)
			.Line()
			.Usings("System.Runtime.CompilerServices", "BepInEx.Logging")
			.Line()
			.BeginClass(Accessibility.Internal, "StaticInit", isPartial: false, isStatic: true)
			.BeginFunction(Accessibility.Internal, "static void InitAll(ManualLogSource log)")
				.For(targets.AsSpan(), (builder, ref readonly target) => {
					builder.StatementF("Init(typeof(global::{0}), log)", target.ToDisplayString());
				})
			.EndFunction()
			.Line()
			.Line("[MethodImpl(MethodImplOptions.AggressiveInlining)]")
			.BeginFunction(Accessibility.Internal, "static void Init(Type type, ManualLogSource log)")
				.BeginBlock("try")
					.Statement("RuntimeHelpers.RunClassConstructor(type.TypeHandle)")
				.EndBlock(false)
				.BeginBlock("catch (Exception ex)")
					.Statement("log.LogError($\"{type.Name}: {ex}\")")
				.EndBlock()
			.EndFunction()
			.EndClass();

		ctx.AddSource("StaticInit.g.cs", SourceText.From(builder.ToString(), Encoding.UTF8));
	}
}
