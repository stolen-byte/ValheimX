// SPDX-License-Identifier: GPL-3.0-or-later
namespace StolenByte.ValheimX;

using ItemType = ItemDrop.ItemData.ItemType;
using AttackType = Attack.AttackType;

// =============================================================================
[HarmonyPatch, BindConfig(ConfigSections.Gameplay)]
static partial class EquipInWater
{
	static readonly Func<Instruction, bool>[] m_codeMatches = [
		x => x.MatchLdarg(0),
		x => x.MatchCall<Character>(nameof(Character.IsSwimming)),
		x => x.OpCode == OpCodes.Brfalse_S,
		x => x.MatchLdarg(0),
		x => x.MatchCall<Character>(nameof(Character.IsOnGround)),
		x => x.OpCode == OpCodes.Brfalse_S || x.OpCode == OpCodes.Brtrue_S
	];

	[ConfigValue("EquipInWater", "Allow using non-projectile 1handed equipment in water")]
	static ConfigEntry<bool> m_enabled = null!;

	[HarmonyILManipulator]
	[HarmonyPatch(typeof(Player), nameof(Player.Update))]
	[HarmonyPatch(typeof(Humanoid), nameof(Humanoid.EquipItem))]
	[HarmonyPatch(typeof(Humanoid), nameof(Humanoid.UpdateEquipment))]
	static void Transpile(ILContext ctx, MethodBase original, ILLabel retLabel)
	{
		ctx.ApplyPatch(nameof(EquipInWater), original, retLabel, static (ctx, cursor, original, ret) => {
			var methodName = original.Name;

			if (!cursor.TryGotoNext(MoveType.After, m_codeMatches))
				throw new Exception($"Failed patching {methodName} - code matcher failed");

			switch (methodName) {
				case nameof(Player.Update):
					PatchUpdate(ctx, cursor);
					break;
				case nameof(Humanoid.EquipItem):
					PatchEquipItem(ctx, cursor, ret);
					break;
				case nameof(Humanoid.UpdateEquipment):
					PatchUpdateEquipment(ctx, cursor);
					break;
			}
		});
	}

	static void PatchUpdate(ILContext ctx, ILCursor cursor)
	{
		// Skip calls to IsSwimming/IsOnGround so ShowHandItems runs
		// --- insert ---
		// 	call EquipInWater::IsEnabled()
		// 	brtrue.s IL_035b
		// --- insert ---
		// IL_034b: ldarg.0
		// IL_034c: call instance bool Character::IsSwimming()
		// IL_0351: brfalse.s IL_035b
		// IL_0353: ldarg.0
		// IL_0354: call instance bool Character::IsOnGround()
		// IL_0359: brfalse.s IL_036b
		// --- cursor ---
		// IL_035b: ldarg.0
		// IL_035c: callvirt instance bool Character::InDodge()
		// IL_0361: brtrue.s IL_036b
		// IL_0363: ldarg.0
		// IL_0364: ldc.i4.0
		// IL_0365: ldc.i4.1
		// IL_0366: call instance void Humanoid::ShowHandItems(bool, bool)
		// IL_036b:

		var target = cursor.MarkLabel();
		cursor.Index -= m_codeMatches.Length;
		cursor.EmitBrTrue(target, IsEnabled);
	}

	static void PatchEquipItem(ILContext ctx, ILCursor cursor, ILLabel retLabel)
	{
		// IL_002d: ldarg.0
		// --- insert --- here because a previous branch jumps to IL_002d, which would skip our code
		// 	call EquipInWater::IsEnabled()
		// 	brfalse.s IL_002e
		// 	ldarg.1
		// 	call EquipInWater::Check(Humanoid, ItemData)
		// 	brfalse.s IL_004d
		// 	br IL_004f
		// --- insert ---
		// IL_002e: callvirt instance bool Character::IsPlayer()
		// IL_0033: brfalse.s IL_004f
		// IL_0035: ldarg.0
		// IL_0036: callvirt instance bool Character::IsDead()
		// IL_003b: brtrue.s IL_004f
		// IL_003d: ldarg.0
		// IL_003e: call instance bool Character::IsSwimming()
		// IL_0043: brfalse.s IL_004f
		// IL_0045: ldarg.0
		// IL_0046: call instance bool Character::IsOnGround()
		// IL_004b: brtrue.s IL_004f
		// IL_004d: ldc.i4.0
		// IL_004e: ret
		// IL_004f: ...
		var disabledLabel = cursor.DefineLabel();
		var checkFalse = cursor.MarkLabel();
		var skipLabel = cursor.CreateLabelAt(2);

		cursor.Index -= 11;
		cursor
			.EmitBrFalse(disabledLabel, IsEnabled)
			.Emit(OpCodes.Ldarg_1)
			.EmitBrFalse(checkFalse, Check)
			.Emit(OpCodes.Br_S, skipLabel)
			.MarkLabel(disabledLabel);
	}

	static void PatchUpdateEquipment(ILContext ctx, ILCursor cursor)
	{
		// Replace call to HideHandItems with our own
		// IL_0009: ldarg.0
		// IL_000a: call instance bool Character::IsSwimming()
		// IL_000f: brfalse.s IL_0022
		// IL_0011: ldarg.0
		// IL_0012: call instance bool Character::IsOnGround()
		// IL_0017: brtrue.s IL_0022
		// --- snip ---
		// IL_0019: ldarg.0
		// IL_001a: ldc.i4.0
		// IL_001b: ldc.i4.1
		// IL_001c: call instance bool Humanoid::HideHandItems(bool, bool)
		// IL_0021: pop
		// --- snip ---
		// --- insert ---
		// 	ldarg.0
		// 	call EquipInWater.UpdateHandItems
		// --- insert ---
		// IL_0022:

		cursor
			.RemoveRange(5)
			.Emit(OpCodes.Ldarg_0)
			.EmitCall(UpdateHandItems);
	}

	static bool IsEnabled()
		=> m_enabled.Value;

	static bool Check(Humanoid character, ItemDrop.ItemData item)
	{
		if (character.IsPlayer()) {
			// original check is fine for player
			if (!character.IsDead() && character.IsSwimming() && !character.IsOnGround()) {
				return CanUseInWater(item);
			}
		} else {
			if (character.InLiquidSwimDepth()) {
				return CanUseInWater(item, true);
			}
		}
		return true;
	}

	internal static bool CanUseInWater(ItemDrop.ItemData item, bool isAi = false)
	{
		var attackType = item.m_shared.m_attack.m_attackType;
		var itemType = item.m_shared.m_itemType;

		if (!item.IsTwoHanded() && attackType != AttackType.Projectile && itemType != ItemType.Torch) {
			return !isAi || item.m_shared.m_aiWhenSwiming;
		}
		return false;
	}

	static void UpdateHandItems(Humanoid character)
	{
		if (m_enabled.Value) {
			if (UpdateItem(character, character.m_leftItem, false) || UpdateItem(character, character.m_rightItem, true)) {
				character.SetupVisEquipment(character.m_visEquipment, false);
				character.m_zanim.SetTrigger("equip_hip");
			}
		} else {
			// original
			character.HideHandItems(false, true);
		}
	}

	static bool UpdateItem(Humanoid character, ItemDrop.ItemData? item, bool isRightHand)
	{
		if (item is null || CanUseInWater(item))
			return false;

		character.UnequipItem(item);
		if (isRightHand) {
			character.m_hiddenRightItem = item;
		} else {
			character.m_hiddenLeftItem = item;
		}

		return true;
	}
}
