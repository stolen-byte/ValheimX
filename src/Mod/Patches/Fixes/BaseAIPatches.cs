// SPDX-License-Identifier: GPL-3.0-or-later
namespace StolenByte.ValheimX;

// =============================================================================
[HarmonyPatch]
static class BaseAIPatches
{
	static readonly MethodInfo? InLiquidSwimDepth = Reflection.Method<Character>(nameof(Character.InLiquidSwimDepth));
	static readonly Func<Instruction, bool>[] m_isSwimmingMatcher = [
		x => x.MatchLdarg(0),
		x => x.MatchLdfld<BaseAI>(nameof(BaseAI.m_character)),
		x => x.MatchCallvirt<Character>(nameof(Character.IsSwimming)),
	];

	[HarmonyPatch(typeof(BaseAI), nameof(BaseAI.CanUseAttack)), HarmonyILManipulator]
	static void CanUseAttack(ILContext ctx)
	{
		ctx.ApplyPatch(nameof(BaseAIPatches), static (ctx, cursor) => {
			if (InLiquidSwimDepth is null)
				throw new Exception("Unable to find `InLiquidSwimDepth` method");

			if (!cursor.TryGotoNext(MoveType.After, m_isSwimmingMatcher))
				throw new Exception("Failed to apply IsSwimming patch");

			// switch the `IsSwimming` call to use `InLiquidSwimDepth` instead
			cursor.Index--;
			cursor
				.Remove()
				.Emit(OpCodes.Callvirt, InLiquidSwimDepth);
		});
	}
}
