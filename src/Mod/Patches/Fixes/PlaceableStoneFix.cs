// SPDX-License-Identifier: GPL-3.0-or-later
namespace StolenByte.ValheimX;

// =============================================================================
[StaticInit]
static class PlaceableStoneFix
{
	static PlaceableStoneFix()
	{
		PrefabDB.Updated += OnDbUpdated;
	}

	static void OnDbUpdated(object sender, PrefabDB.UpdateEventArgs e)
	{
		if (!e.HasItems)
			return;

		if (PrefabDB.GetPrefab("Placeable_Stone") is { } prefab) {
			if (prefab.GetComponent<Pickable>() is { } stone) {
				stone.m_dontScale = true;
			}
		}
	}
}
