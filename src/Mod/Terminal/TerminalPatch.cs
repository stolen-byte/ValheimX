// SPDX-License-Identifier: GPL-3.0-or-later
namespace StolenByte.ValheimX;

// =============================================================================
[HarmonyPatch]
static class TerminalPatch
{
	static bool m_done = false;

	// Purely debug-related commands, that can be helpful in reporting bugs, and as such,
	// have no reason to be regarded as cheats.
	static readonly HashSet<string> m_nonCheats = new() {
		"dpsdebug",
		"listkeys",
		"printcreatures",
		"printnetobj",
		"printlocations",
		"biomeinfo",
	};

	[HarmonyPatch(typeof(Terminal), nameof(Terminal.InitTerminal)), HarmonyPostfix]
	static void InitTerminal()
	{
		if (!m_done) {
			Terminal.commands.Remove("help"); // remove built-in help command so we don't trigger any exceptions.
			FixCheats();
			CommandBinder.RegisterCommands();
			m_done = true;
		}
	}

	static void FixCheats()
	{
		foreach (var name in m_nonCheats) {
			if (Terminal.commands.TryGetValue(name, out var cmd)) {
				cmd.IsCheat = false;
			}
		}
	}
}
