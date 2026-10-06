// SPDX-License-Identifier: GPL-3.0-or-later
namespace StolenByte.ValheimX;

// =============================================================================
[HarmonyPatch]
static class TerminalPatch
{
	static bool m_done = false;

	[HarmonyPatch(typeof(Terminal), nameof(Terminal.InitTerminal)), HarmonyPostfix]
	static void InitTerminal()
	{
		if (!m_done) {
			CommandBinder.RegisterCommands();
			m_done = true;
		}
	}
}
