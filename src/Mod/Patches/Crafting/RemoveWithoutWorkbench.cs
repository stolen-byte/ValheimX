// SPDX-License-Identifier: GPL-3.0-or-later
namespace StolenByte.ValheimX;

// =============================================================================
[HarmonyPatch]
static class RemoveWithoutWorkbenchPatch
{
	static readonly Func<Instruction, bool>[] m_codeMatches = [
		x => x.MatchLdarg(1),
		x => x.MatchLdfld<Piece>(nameof(Piece.m_craftingStation)),
		x => x.MatchLdfld<CraftingStation>(nameof(CraftingStation.m_name)),
		x => x.MatchLdarg(0),
	];

	[HarmonyPatch(typeof(Player), nameof(Player.CheckCanRemovePiece)), HarmonyILManipulator]
	static void CheckCanRemovePiece(ILContext ctx)
	{
		// IL_0000: ldarg.0
		// IL_0001: ldfld bool Player::m_noPlacementCost
		// IL_0006: brtrue.s IL_0057
		//
		// IL_0008: ldarg.1
		// IL_0009: ldfld Piece::m_craftingStation
		// IL_000e: ldnull
		// IL_000f: call bool UnityEngine.Object::op_Inequality(UnityEngine.Object, UnityEngine.Object)
		// IL_0014: brfalse.s IL_0057
		//
		// --- insert ---
		// ldarg.1
		// call RemoveWithoutWorkbenchPatch::NeedsWorkbench(Piece)
		// brfalse.s IL_0057
		// --- insert ---
		//
		// IL_0016: ldarg.1
		// IL_0017: ldfld Piece::m_craftingStation
		// IL_001c: ldfld CraftingStation::m_name
		// IL_0021: ldarg.0
		// IL_0022: call UnityEngine.Component::get_transform()
		// IL_0027: callvirt UnityEngine.Transform::get_position()
		// IL_002c: call CraftingStation::HaveBuildStationInRange(string, UnityEngine.Vector3)
		// IL_0031: call bool UnityEngine.Object::op_Implicit(UnityEngine.Object)
		// IL_0036: brtrue.s IL_0057
		//
		// IL_0038: call ZoneSystem::get_instance()
		// IL_003d: ldc.i4.s 27
		// IL_003f: callvirt bool ZoneSystem::GetGlobalKey(GlobalKeys)
		// IL_0044: brtrue.s IL_0057
		//
		// IL_0046: ldarg.0
		// IL_0047: ldc.i4.2
		// IL_0048: ldstr "$msg_missingstation"
		// IL_004d: ldc.i4.0
		// IL_004e: ldnull
		// IL_004f: ldc.i4.0
		// IL_0050: callvirt Character::Message(MessageHud/MessageType, string, int32, UnityEngine.Sprite, bool)
		//
		// IL_0055: ldc.i4.0
		// IL_0056: ret
		//
		// IL_0057: ldc.i4.1
		// IL_0058: ret
		ctx.ApplyPatch(nameof(RemoveWithoutWorkbenchPatch), static (ctx, cursor) => {
			if (!cursor.TryGotoNext(MoveType.Before, m_codeMatches))
				throw new Exception("Failed to match code section");

			if (cursor.Prev.Operand is not ILLabel skipLabel)
				throw new Exception("Unable to find jump label");

			cursor
				.Emit(OpCodes.Ldarg_1)
				.EmitCall(NeedsWorkbench)
				.Emit(OpCodes.Brfalse_S, skipLabel);
		});
	}

	static bool NeedsWorkbench(Piece piece)
		=> piece.m_craftingStation is not null &&
				!ObjectComparer.Default.Equals(piece.m_craftingStation, Constants.Pieces.Hammer.Workbench);
}
