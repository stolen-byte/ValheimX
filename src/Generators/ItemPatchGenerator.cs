// SPDX-License-Identifier: GPL-3.0-or-later
namespace StolenByte.ValheimX.Generators;

// =============================================================================
[Generator(LanguageNames.CSharp)]
public sealed class ItemPatchGenerator : IIncrementalGenerator
{
	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		var patches = context.SyntaxProvider
			.ForAttributeWithMetadataName(
				TypeNames.Internal.ItemPatchAttribute,
				predicate: static (node, _) => node is ClassDeclarationSyntax,
				transform: GetItemPatch
			)
			.NonNull();

		context.RegisterSourceOutput(
			patches.Collect(),
			GeneratePatchMethod
		);
	}

	static ItemPatch? GetItemPatch(GeneratorAttributeSyntaxContext ctx, CancellationToken ct)
	{
		ct.ThrowIfCancellationRequested();

		if (ctx.TargetSymbol is not INamedTypeSymbol cls)
			return null;

		var args = ctx.Attributes[0].ConstructorArguments;
		if (args.HasErrors())
			return null;

		string section = "null";
		string key;
		string description;
		if (args.Length > 2) {
			section = args[0].ToCode();
			key = args[1].ToCode();
			description = args[2].ToCode();
		} else if (args.Length > 1) {
			key = args[0].ToCode();
			description = args[1].ToCode();
		} else {
			return null;
		}

		return new(cls.ClassRef(), section, key, description);
	}

	static void GeneratePatchMethod(SourceProductionContext ctx, ImmutableArray<ItemPatch> targets)
	{
		ctx.CancellationToken.ThrowIfCancellationRequested();
		var builder = new CodeBuilder();

		builder
			.Header()
			.Line()
			.Namespace(TypeNames.Internal.Namespace)
			.Line()
			.BeginClass(Accessibility.Public, "ItemPatcher", isPartial: true, isStatic: true)
				.BeginFunctionF(Accessibility.Internal, "static void BindPatches(global::{0} cfg)", TypeNames.BepIn.ConfigFile)
					.For(targets.AsSpan(), (builder, ref readonly target) => {
						builder.BeginStatement("Add(new global::");
						target.Target.FormatTo(builder);
						builder.AppendFormat(
							"().Configure(cfg, {0}, {1}, {2})",
							target.Section,
							target.Key,
							target.Description
						);
						builder.EndStatement(")");
					})
				.EndFunction()
			.EndClass();

		ctx.AddSource("ItemPatcher.g.cs", SourceText.From(builder.ToString(), Encoding.UTF8));
	}

	// =============================================================================
	readonly record struct ItemPatch(
		ClassRef Target,
		string Section,
		string Key,
		string Description
	);
}
