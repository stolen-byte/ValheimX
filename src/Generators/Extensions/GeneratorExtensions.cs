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
}
