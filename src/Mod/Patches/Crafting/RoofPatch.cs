// SPDX-License-Identifier: GPL-3.0-or-later
namespace StolenByte.ValheimX;

// =============================================================================
[ItemPatch(ConfigSections.Crafting, "ExtraRoofs", "Floor/Stair pieces count as roofs")]
sealed class RoofPatch : ItemPatch<Transform>
{
	static readonly ObjectSet m_items = new(
		"wood_floor",
		"wood_floor_1x1",
		"ashwood_floor_1x1",
		"ashwood_floor_2x2",
		"wood_stair",
		"ashwood_stair",
		"piece_dvergr_spiralstair",
		"piece_dvergr_spiralstair_right",
		"piece_table",
		"piece_table_oak",
		"piece_table_round",
		"piece_table_runed",
		"piece_table_runed_small",
		"piece_blackmarble_table",
		"piece_blackmarble_bench",
		"dvergrprops_wood_floor",
		"dvergrprops_wood_stair"
	);

	protected override IEnumerable<Transform> GetItems()
	{
		if (PrefabDB.GetPieceTable(Constants.PieceTables.Hammer) is not { } hammer)
			throw new Exception("Unable to get hammer piece table");

		foreach (var piece in hammer.m_pieces) {
			if (!m_items.Contains(piece))
				continue;

			if (GetCollider(piece) is { } collider) {
				yield return collider;
			}
		}
	}

	protected override void Apply(Transform collider)
	{
		collider.tag = Constants.Tags.Roof;
	}

	protected override void Remove(Transform collider)
	{
		collider.tag = Constants.Tags.Leaky;
	}

	protected override void OnEnabledChanged(bool enabled)
	{
		var tag = enabled ? Constants.Tags.Roof : Constants.Tags.Leaky;
		// update existing pieces in-world
		foreach (var piece in GameObject.FindObjectsByType<Piece>(FindObjectsSortMode.None)) {
			if (!m_items.Contains(piece))
				continue;

			if (GetCollider(piece.gameObject) is { } collider) {
				collider.tag = tag;
			}
		}
	}

	static Transform? GetCollider(GameObject piece)
	{
		if (piece.GetComponent<Transform>() is { } transform) {
			if (transform.Find("collider") is { } collider) {
				return collider;
			}
		}
		return null;
	}
}
