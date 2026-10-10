// SPDX-License-Identifier: GPL-3.0-or-later
namespace StolenByte.ValheimX;

using ItemType = ItemDrop.ItemData.ItemType;
using PaintType = TerrainModifier.PaintType;

// =============================================================================
[HarmonyPatch, BindConfig(ConfigSections.Crafting)]
partial class RemoveOnTerrainMod : MonoBehaviour
{
	[Flags]
	enum ModType
	{
		None = 0,
		Cultivate = 1 << 0,
		Pave = 1 << 1,
		All = Cultivate | Pave,
	}

	static readonly ObjectSet m_cultivateExclude = new(
		"Pickable_Barley",
		"Pickable_Barley_Wild",
		"Pickable_Flax",
		"Pickable_Flax_Wild",
		"Pickable_Oat",
		"Pickable_SeedCarrot",
		"Pickable_SeedKale",
		"Pickable_SeedOnion",
		"Pickable_SeedTurnip",
		"Pickable_Turnip",
		"Placeable_Stone"
	);

	[HarmonyPatch(typeof(TerrainComp), nameof(TerrainComp.InternalDoOperation)), HarmonyPostfix]
	static void OnTerrainMod(TerrainComp __instance, Vector3 pos, TerrainOp.Settings modifier)
	{
		var radius = modifier.GetRadius() / __instance.m_hmap.m_scale;

		foreach (var removable in GameObject.FindObjectsByType<RemoveOnTerrainMod>(FindObjectsSortMode.None)) {
			var dist = Vector3.Distance(pos, removable.gameObject.transform.position);
			if (dist <= radius && removable.ShouldRemoveOn(modifier.m_paintType)) {
				removable.TriggerRemove();
			}
		}
	}

	[HarmonyPatch(typeof(Plant), nameof(Plant.Awake)), HarmonyPostfix]
	static void PlantAwake(Plant __instance)
	{
		AddComponent(__instance.gameObject, ModType.Pave);
	}

	[HarmonyPatch(typeof(Pickable), nameof(Pickable.Awake)), HarmonyPostfix]
	static void PickableAwake(Pickable __instance)
	{
		if (__instance.m_itemPrefab.GetComponent<ItemDrop>() is { } item) {
			var paintType = ModType.Pave;
			if (ShouldRemovePickable(item))
				paintType |= ModType.Cultivate;

			var c = AddComponent(__instance.gameObject, paintType);
			c.m_pickable = __instance;
		}
	}

	static bool ShouldRemovePickable(ItemDrop item)
	{
		var itemType = item.m_itemData.m_shared.m_itemType;
		return itemType != ItemType.Consumable && !(itemType == ItemType.Material && m_cultivateExclude.Contains(item));
	}

	static RemoveOnTerrainMod AddComponent(GameObject obj, ModType onTypes)
	{
		var comp = obj.GetOrAddComponent<RemoveOnTerrainMod>();
		comp.m_onType = onTypes;
		return comp;
	}

	// =============================================================================
	ModType m_onType = ModType.None;
	Pickable? m_pickable;

	public bool ShouldRemoveOn(PaintType paintType)
	{
		return paintType switch {
			PaintType.Cultivate => m_onType.HasFlag(ModType.Cultivate),
			PaintType.Paved => m_onType.HasFlag(ModType.Pave),
			_ => false,
		};
	}

	public void TriggerRemove()
	{
		if (m_pickable is not null) {
			var zview = m_pickable.m_nview;
			if (zview.IsValid() && m_pickable.m_enabled != 0 && !m_pickable.m_picked) {
				// use RPC directly, so we don't effect harvesting stats
				zview.InvokeRPC("RPC_Pick", 0);
			}
		}
		gameObject.TriggerDestroy();
	}
}
