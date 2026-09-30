// SPDX-License-Identifier: GPL-3.0-or-later
namespace StolenByte.ValheimX.Generators;

// =============================================================================
[Generator(LanguageNames.CSharp)]
public sealed class ModGenerator : IIncrementalGenerator
{
	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		var info = context.AnalyzerConfigOptionsProvider.Select(
			static (config, _) => new ModInfo(
				config.GetBuildProp(BuildProps.ModName, string.Empty)!,
				config.GetBuildProp(BuildProps.ModVersion, string.Empty)!,
				config.GetBuildProp(BuildProps.ModAuthor, string.Empty)!,
				config.GetBuildProp(BuildProps.ModGUID, string.Empty)!
			)
		);

		var patches = context.SyntaxProvider
			.ForAttributeWithMetadataName(
				TypeNames.Harmony.PatchAttribute,
				predicate: static (node, _) => node is ClassDeclarationSyntax,
				transform: GetPatchTarget
			)
			.NonNull();

		var targets = context.SyntaxProvider
			.ForAttributeWithMetadataName(
				TypeNames.Internal.ModAttribute,
				predicate: static (_, _) => true,
				transform: GetModTarget
			)
			.NonNull();

		context.RegisterSourceOutput(
			info.Combine(patches.Collect()).Combine(targets.Collect()),
			static (ctx, data) => {
				var ((mod, patches), targets) = data;

				if (targets.Length > 1) {
					ctx.ReportDiagnostic(Diagnostics.MultipleModsDefined(targets[1].Location).ToRoslynDiagnostic());
				} else if (targets.IsEmpty) {
					return;
				}

				ref readonly var target = ref targets.AsSpan()[0];
				GenerateModExtension(ctx, in mod, in target, patches);
			}
		);
	}

	static ModTarget? GetModTarget(GeneratorAttributeSyntaxContext ctx, CancellationToken ct)
	{
		ct.ThrowIfCancellationRequested();
		if (ctx.TargetSymbol is INamedTypeSymbol cls)
			return new(cls.ClassRef(), cls.IsSealed, ctx.Attributes[0].SourceLocation());
		return null;
	}

	static ClassRef? GetPatchTarget(GeneratorAttributeSyntaxContext ctx, CancellationToken ct)
	{
		ct.ThrowIfCancellationRequested();
		if (ctx.TargetSymbol is INamedTypeSymbol cls)
			return cls.ClassRef();
		return null;
	}

	static void GenerateModExtension(
		SourceProductionContext ctx,
		ref readonly ModInfo mod,
		ref readonly ModTarget target,
		ImmutableArray<ClassRef> patches)
	{
		ctx.CancellationToken.ThrowIfCancellationRequested();
		var builder = new CodeBuilder();

		builder
			.Header()
			.Line()
			.Namespace(target.Type.Namespace)
			.Line()
			.Usings("System", "System.Reflection", "BepInEx")
			.Line()
			.Line("[BepInPlugin(GUID, Name, Version)]")
			.BeginClass(target.Type.Visibility, target.Type.Name, isPartial: true)
				.Property(Accessibility.Private, "Harmony", "Harmony", write: false, value: "new(GUID)")
				.Line()
				.Field(Accessibility.Public, "string", "Name", isConst: true, value: mod.Name.Quoted())
				.Field(Accessibility.Public, "string", "Version", isConst: true, value: mod.Version.Quoted())
				.Field(Accessibility.Public, "string", "Author", isConst: true, value: mod.Author.Quoted())
				.Field(Accessibility.Public, "string", "GUID", isConst: true, value: mod.GUID.Quoted())
				.Line()
				.With(in target, static (builder, ref readonly target) => {
					builder.BeginStatement("public static Assembly Assembly { get; } = typeof(global::");
					target.Type.FormatTo(builder);
					builder.EndStatement(").GetTypeInfo().Assembly");
				})
				.Line()
				.BeginFunction(
					target.IsSealed ? Accessibility.Private : Accessibility.Protected,
					"void PatchAll()")
					.For(patches.AsSpan(), (builder, ref readonly target) => {
						builder.BeginStatement("Harmony.PatchAll(typeof(global::");
						target.FormatTo(builder);
						builder.EndStatement("))");
					})
				.EndFunction()
			.EndClass();

		ctx.AddSource($"{target.Type.Name}.g.cs", SourceText.From(builder.ToString(), Encoding.UTF8));
	}

	// =============================================================================
	/// <remarks>32 bytes</remarks>
	readonly record struct ModInfo(string Name, string Version, string Author, string GUID);

	/// <remarks>32 bytes</remarks>
	readonly record struct ModTarget(ClassRef Type, bool IsSealed, SourceLocation? Location = null)
	{
		// Location is for diagnostics only, and should not contribute to equality
		public bool Equals(ModTarget other)
		{
			return EqualityComparer<ClassRef>.Default.Equals(Type, other.Type)
				&& EqualityComparer<bool>.Default.Equals(IsSealed, other.IsSealed);
		}

		public override int GetHashCode()
			=> HashCode.Combine(Type, IsSealed);
	}
}
