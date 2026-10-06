// SPDX-License-Identifier: GPL-3.0-or-later
namespace StolenByte.ValheimX;

// =============================================================================
[ConsoleCommand("Print current biome/environment information", IsDev = true)]
static partial class PrintEnv
{
	static bool Execute(Terminal ctx)
	{
		if (Player.m_localPlayer is not { } localPlayer)
			return false;

		if (EnvMan.instance is not { } envMan)
			return false;

		var env = envMan.GetCurrentEnvironment();
		ctx.AddString($"Current Biome: {localPlayer.GetCurrentBiome()}");
		ctx.AddString($"Current Env: {env.m_name}");
		ctx.AddString("Env Systems Active:");
		foreach (var sys in env.m_psystems) {
			ctx.AddString($"  {sys.name}");
		}

		return true;
	}
}
