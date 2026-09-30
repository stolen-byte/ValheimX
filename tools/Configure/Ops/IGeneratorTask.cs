// SPDX-License-Identifier: GPL-3.0-or-later
namespace StolenByte.ValheimX.Configure;

// =============================================================================
interface IGeneratorTask
{
	Task Run(Project mod, DirectoryInfo gameDir, CancellationToken ct);
}
