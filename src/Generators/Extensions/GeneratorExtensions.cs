// SPDX-License-Identifier: GPL-3.0-or-later
namespace StolenByte.ValheimX.Generators;

// =============================================================================
static class GeneratorExtensions
{
	public static string? GetBuildProp(this AnalyzerConfigOptionsProvider cfg, string name, string? defaultValue = null)
		=> cfg.GlobalOptions.TryGetValue($"build_property.{name}", out var result)
				? result
				: defaultValue;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static INamedTypeSymbol GetTypeByName(this SemanticModel sm, string fqn)
		=> sm.Compilation.GetTypeByMetadataName(fqn) ?? throw new InvalidOperationException($"Unable to find type {fqn}");

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static INamedTypeSymbol GetSpecialType(this SemanticModel sm, SpecialType type)
		=> sm.Compilation.GetSpecialType(type);

	public static INamedTypeSymbol GetGenericSpecialType(this SemanticModel sm, SpecialType generic, SpecialType inner)
	{
		var outerType = sm.Compilation.GetSpecialType(generic);
		var innerType = sm.Compilation.GetSpecialType(inner);
		// no need to check if outer is generic, Construct will do that for us (and throw)
		return outerType.Construct(innerType);
	}

	public static INamedTypeSymbol GetSymbolForType(this SemanticModel sm, Type type)
	{
		if (!type.IsConstructedGenericType)
			return sm.GetTypeByName(type.FullName);

		var baseType = sm.GetTypeByName(type.GetGenericTypeDefinition().FullName);
		return baseType.Construct(type.GenericTypeArguments.Select(t => GetSymbolForType(sm, t)).ToArray());
	}
}
