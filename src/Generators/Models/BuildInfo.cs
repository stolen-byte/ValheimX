// SPDX-License-Identifier: GPL-3.0-or-later
namespace StolenByte.ValheimX.Generators;

// =============================================================================
/// <remarks>8 bytes</remarks>
readonly record struct BuildInfo(string? RootNamespace)
{
	public static BuildInfo From(AnalyzerConfigOptionsProvider opts)
	{
		return new(
			opts.GetBuildProp(BuildProps.RootNamespace)
		);
	}
}
