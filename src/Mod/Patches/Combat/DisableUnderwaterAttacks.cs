// SPDX-License-Identifier: GPL-3.0-or-later
namespace StolenByte.ValheimX;

using ItemData = ItemDrop.ItemData;
using ItemType = ItemDrop.ItemData.ItemType;

// =============================================================================
[ItemPatch(ConfigSections.Combat, "DisableUnderwaterAttacks", "Disable ranged enemy attacks when they are underwater")]
sealed class DisableUnderwaterAttacks : ItemPatch<ItemData>
{
	static readonly ObjectSet m_extraOneHanded = new(
		"GoblinTorch", // classified as 1handed weapons instead of torches
		"GoblinTorchDeepNorth"
	);

	protected override IEnumerable<ItemData> GetItems()
	{
		foreach (var obj in ObjectDB.instance.m_itemByHash.Values) {
			if (obj.GetComponent<ItemDrop>() is not { } item)
				continue;

			var itemData = item.m_itemData;
			if (!EquipInWater.CanUseInWater(itemData)) {
				yield return itemData;
			} else if (itemData.m_shared.m_itemType == ItemType.OneHandedWeapon && m_extraOneHanded.Contains(item)) {
				yield return itemData;
			}
		}
	}

	protected override void Apply(ItemData item)
	{
		UpdateItem(item, false);
	}

	protected override void Remove(ItemData item)
	{
		UpdateItem(item, true);
	}

	protected override void OnEnabledChanged(bool enabled)
	{
		// in-world spawned monsters have cloned instances, so we also need to update them
		foreach (var ai in GameObject.FindObjectsByType<MonsterAI>(FindObjectsSortMode.None)) {
			if (ai.m_character is not Humanoid monster)
				continue;

			foreach (var item in monster.GetInventory().m_inventory) {
				if (item.IsWeapon() && !EquipInWater.CanUseInWater(item)) {
					UpdateItem(item, enabled);
				}
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	static void UpdateItem(ItemDrop.ItemData item, bool allow)
	{
		item.m_shared.m_aiWhenSwiming = allow;
	}
}
