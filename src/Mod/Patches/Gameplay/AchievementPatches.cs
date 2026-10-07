// SPDX-License-Identifier: GPL-3.0-or-later
namespace StolenByte.ValheimX;

// =============================================================================
[HarmonyPatch]
static class AchievementPatches
{
	private static readonly Func<Instruction, bool>[] m_isModdedMatcher = [
		x => x.OpCode == OpCodes.Brtrue_S,
		x => x.MatchLdsfld<Game>(nameof(Game.isModded)),
		x => x.OpCode == OpCodes.Br_S,
		x => x.MatchLdcI4(1),
		x => x.MatchStsfld<Achievements>(nameof(Achievements.m_cheatCheckCache))
	];

	[HarmonyPatch(typeof(Achievements), nameof(Achievements.IsCheatedAtAll)), HarmonyILManipulator]
	static void IsCheatedAtAll(ILContext ctx)
	{
		ctx.ApplyPatch(nameof(AchievementPatches), static (ctx, cursor) => {
			if (!cursor.TryGotoNext(MoveType.Before, m_isModdedMatcher)) {
				throw new Exception("Failed to apply patch to IsCheatedAtAll");
			}

			// IL_0055: ldloc.1
			// IL_0056: or
			// IL_0057: ldloc.0
			// IL_0058: or
			// -- snip --
			// IL_0059: brtrue.s  IL_0062
			// IL_005B: ldsfld    bool Game::isModded
			// IL_0060: br.s      IL_0063
			// IL_0062: ldc.i4.1
			// -- snip --
			// IL_0063: stsfld    bool Achievements::m_cheatCheckCache
			// IL_0068: ldsfld    bool Achievements::m_cheatCheckCache
			// IL_006D: ret
			cursor.RemoveRange(4);
		});
	}
}
