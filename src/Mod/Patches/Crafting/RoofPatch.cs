// SPDX-License-Identifier: GPL-3.0-or-later
namespace StolenByte.ValheimX;

using Constants;

// =============================================================================
[ItemPatch(ConfigSections.Crafting, "ExtraRoofs", "Floor/Stair pieces count as roofs")]
sealed class RoofPatch : ItemPatch<Transform>
{
	static readonly ObjectSet m_items = new(
		Pieces.Hammer.WoodFloor,
		Pieces.Hammer.WoodFloor1x1,
		Pieces.Hammer.AshwoodFloor1x1,
		Pieces.Hammer.AshwoodFloor2x2,
		Pieces.Hammer.WoodStairs,
		Pieces.Hammer.AshwoodStairs,
		Pieces.Hammer.DvergrSpiralStairs,
		Pieces.Hammer.DvergrSpiralStairsRight,
		Pieces.Hammer.Table,
		Pieces.Hammer.TableOak,
		Pieces.Hammer.TableRound,
		Pieces.Hammer.TableRuned,
		Pieces.Hammer.TableRunedSmall,
		Pieces.Hammer.BlackmarbleTable,
		Pieces.Hammer.BlackmarbleBench,
		Pieces.Hammer.DvergrWoodFloor,
		Pieces.Hammer.DvergrWoodStair
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
