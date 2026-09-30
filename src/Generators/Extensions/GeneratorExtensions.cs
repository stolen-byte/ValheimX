// SPDX-License-Identifier: GPL-3.0-or-later
namespace StolenByte.ValheimX.Generators;

// =============================================================================
static class GeneratorExtensions
{
	public static string? GetBuildProp(this AnalyzerConfigOptionsProvider cfg, string name, string? defaultValue = null)
		=> cfg.GlobalOptions.TryGetValue($"build_property.{name}", out var result)
				? result
				: defaultValue;
}
