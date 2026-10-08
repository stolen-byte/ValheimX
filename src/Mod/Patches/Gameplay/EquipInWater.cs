// SPDX-License-Identifier: GPL-3.0-or-later
namespace StolenByte.ValheimX;

// =============================================================================
[HarmonyPatch, BindConfig(ConfigSections.Gameplay)]
static partial class EquipInWater
{
	static readonly Type ThisType = typeof(EquipInWater);

	// non 2handed items we don't want usable underwater
	static readonly ObjectSet m_blackList = new(
		"BombBile",
		"BombBlob_Frost",
		"BombBlob_Lava",
		"BombBlob_Morkhalla",
		"BombBlob_Poison",
		"BombBlob_PoisonElite",
		"BombBlob_Tar",
		"BombDynamite",
		"BombLava",
		"BombOoze",
		"BombSmoke",
		"Torch",
		"TorchMist",
		"Sparkler",
		"Feaster",
		"Lantern",
		"Lantern_DN",
		"Lantern_hooded"
	);

	static readonly Func<Instruction, bool>[] m_codeMatches = [
		x => x.MatchLdarg(0),
		x => x.MatchCall<Character>(nameof(Character.IsSwimming)),
		x => x.OpCode == OpCodes.Brfalse_S,
		x => x.MatchLdarg(0),
		x => x.MatchCall<Character>(nameof(Character.IsOnGround)),
		x => x.OpCode == OpCodes.Brfalse_S || x.OpCode == OpCodes.Brtrue_S
	];

	[ConfigValue("EquipInWater", "Allow using 1handed equipment in water")]
	static ConfigEntry<bool> m_enabled = null!;

	[HarmonyILManipulator]
	[HarmonyPatch(typeof(Player), nameof(Player.Update))]
	[HarmonyPatch(typeof(Humanoid), nameof(Humanoid.EquipItem))]
	[HarmonyPatch(typeof(Humanoid), nameof(Humanoid.UpdateEquipment))]
	static void Transpile(ILContext ctx, MethodBase original)
	{
		ctx.ApplyPatch(original, nameof(EquipInWater), static (ctx, original, cursor) => {
			var methodName = original.Name;

			if (!cursor.TryGotoNext(MoveType.After, m_codeMatches))
				throw new Exception($"Failed patching {methodName} - code matcher failed");

			switch (methodName) {
				case nameof(Player.Update):
					PatchUpdate(ctx, cursor);
					break;
				case nameof(Player.EquipItem):
					PatchEquipItem(ctx, cursor);
					break;
				case nameof(Player.UpdateEquipment):
					PatchUpdateEquipment(ctx, cursor);
					break;
			}
		});
	}

	static void PatchUpdate(ILContext ctx, ILCursor cursor)
	{
		// Skip calls to IsSwimming/IsOnGround so ShowHandItems runs
		// --- insert ---
		// call () => m_enabled.Value
		// brtrue.s IL_035b
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
		cursor.EmitBrTrue(target, () => m_enabled.Value);
	}

	static void PatchEquipItem(ILContext ctx, ILCursor cursor)
	{
		// IL_002d: ldarg.0
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
		// --- insert ---
		// call () => m_enabled.Value
		// brfalse.s IL_004d
		// ldarg.1
		// call EquipInWater.CanUseInWater
		// brtrue.s IL_004f
		// --- insert ---
		// IL_004d: ldc.i4.0
		// IL_004e: ret
		// IL_004f:

		var jumpLabel = (ILLabel)cursor.Prev.Operand;
		var skipLabel = cursor.DefineLabel();

		// entire check becomes:
		// `if (this.IsPlayer() && !this.IsDead() && base.IsSwimming() && !IsOnGround() && (!Enabled || !CanUseInWater(item)))`
		cursor
			.EmitBrFalse(skipLabel, () => m_enabled.Value)
			.Emit(OpCodes.Ldarg_1)
			.EmitFunc(CanUseInWater)
			.Emit(OpCodes.Brtrue_S, jumpLabel);

		cursor.MarkLabel(skipLabel);
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
		// ldarg.0
		// call EquipInWater.UpdateHandItems
		// --- insert ---
		// IL_0022:

		cursor
			.RemoveRange(5)
			.Emit(OpCodes.Ldarg_0)
			.EmitFunc(UpdateHandItems);
	}

	static bool CanUseInWater(ItemDrop.ItemData item)
		=> !item.IsTwoHanded() && !m_blackList.Contains(item.m_dropPrefab);

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
