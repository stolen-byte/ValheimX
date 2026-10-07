// SPDX-License-Identifier: GPL-3.0-or-later
namespace StolenByte.ValheimX;

// =============================================================================
[HarmonyPatch, BindConfig(ConfigSections.Combat)]
static partial class MultiHitPatch
{
	static readonly Func<Instruction, bool>[] m_codeMatchers = [
		x => x.MatchLdarg(0),
		x => x.MatchLdfld<Attack>(nameof(Attack.m_multiHit)),
		x => x.OpCode == OpCodes.Brfalse_S,
		x => x.MatchLdarg(0),
		x => x.MatchLdfld<Attack>(nameof(Attack.m_lowerDamagePerHit)),
		x => x.OpCode == OpCodes.Brfalse_S,
	];

	[ConfigValue("DisableMultiHitPenalty", "Disable damage penalty for hitting multiple targets")]
	static ConfigEntry<bool> m_enabled = null!;

	[HarmonyPatch(typeof(Attack), nameof(Attack.DoMeleeAttack)), HarmonyILManipulator]
	static void DoMeleeAttack(ILContext ctx)
	{
		ctx.ApplyPatch(nameof(MultiHitPatch), static (ctx, cursor) => {
			if (!cursor.TryGotoNext(m_codeMatchers))
				throw new Exception("Failed to match code section");

			// IL_0743: ldarg.0
			// IL_0744: ldfld bool Attack::m_multiHit
			// IL_0749: brfalse.s IL_0770
			// IL_074b: ldarg.0
			// IL_074c: ldfld bool Attack::m_lowerDamagePerHit
			// IL_0751: brfalse.s IL_0770
			// Reminder: first `.Next` is *current* instruction
			if (cursor.Next.Next.Next.Operand is ILLabel jumpLabel) {
				// skip over entire `if` check when enabled
				cursor.EmitBrTrue(jumpLabel, () => m_enabled.Value);
			} else {
				throw new Exception("Unable to find jump label");
			}
		});
	}
}
