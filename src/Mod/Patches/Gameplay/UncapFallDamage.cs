// SPDX-License-Identifier: GPL-3.0-or-later
namespace StolenByte.ValheimX;

// =============================================================================
[HarmonyPatch, BindConfig(ConfigSections.Gameplay)]
static partial class UncapFallDamage
{
	[ConfigValue("UncapFallDamage", "Remove 100hp cap on fall damage")]
	static ConfigEntry<bool> m_enabled = null!;

	static readonly Func<Instruction, bool>[] m_codeMatcher = [
		x => x.MatchCall<Mathf>(nameof(Mathf.Clamp01))
	];

	[HarmonyPatch(typeof(Character), nameof(Character.UpdateGroundContact)), HarmonyILManipulator]
	static void UpdateGroundContact(ILContext ctx)
	{
		ctx.ApplyPatch(nameof(UncapFallDamage), static (ctx, cursor) => {
			if (!cursor.TryGotoNext(MoveType.Before, m_codeMatcher))
				throw new Exception("Failed to match Clamp01 call");

			// IL_01ae: call float32 [UnityEngine.CoreModule]UnityEngine.Mathf::Clamp01(float32)
			// --- insert ---
			// call () => m_enabled.Value
			// brtrue.s IL_01b3
			// --- insert ---
			// IL_01b3: ldc.r4 100
			var skipLabel = cursor.DefineLabel();

			// skip over call to clamp when enabled
			cursor.EmitBrTrue(skipLabel, () => m_enabled.Value);
			cursor.Index++;
			cursor.MarkLabel(skipLabel);
		});
	}
}
