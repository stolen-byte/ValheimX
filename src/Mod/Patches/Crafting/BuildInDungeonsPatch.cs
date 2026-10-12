// SPDX-License-Identifier: GPL-3.0-or-later
namespace StolenByte.ValheimX;

using Constants;

// =============================================================================
[ItemPatch(ConfigSections.Crafting, "BuildInDungeons", "Allow building some structures in dungeons")]
sealed class BuildInDungeons : ItemPatch<Piece>
{
	static readonly ObjectSet m_blackList = new(
		Pieces.Hammer.Raft,
		Pieces.Hammer.Karve,
		Pieces.Hammer.VikingShip,
		Pieces.Hammer.VikingShipAshlands,
		Pieces.Hammer.Smelter,
		Pieces.Hammer.BlastFurnace,
		Pieces.Hammer.CharcoalKiln,
		Pieces.Hammer.Incinerator,
		Pieces.Hammer.Catapult,
		Pieces.Hammer.Beehive,
		Pieces.Hammer.Birdnest,
		Pieces.Hammer.Maypole,
		Pieces.Hammer.ShieldGenerator,
		Pieces.Hammer.WispLure,
		Pieces.Hammer.Windmill,
		Pieces.Hammer.Drawbridge,
		Pieces.Hammer.DrawbridgeLog,
		//
		Pieces.Cultivator.Barley,
		Pieces.Cultivator.Carrot,
		Pieces.Cultivator.Flax,
		Pieces.Cultivator.Jotunpuffs,
		Pieces.Cultivator.Kale,
		Pieces.Cultivator.Magecap,
		Pieces.Cultivator.Oat,
		Pieces.Cultivator.Onion,
		Pieces.Cultivator.Poteitr,
		Pieces.Cultivator.Seedcarrot,
		Pieces.Cultivator.Seedkale,
		Pieces.Cultivator.Seedonion,
		Pieces.Cultivator.Seedturnip,
		Pieces.Cultivator.Turnip,
		Pieces.Cultivator.Beech,
		Pieces.Cultivator.Birch,
		Pieces.Cultivator.FirTree,
		Pieces.Cultivator.FirTreeBig,
		Pieces.Cultivator.Oak,
		Pieces.Cultivator.PineTree,
		Pieces.Cultivator.VineAsh,
		Pieces.Cultivator.VineGreen
	);

	protected override IEnumerable<Piece> GetItems()
	{
		if (PrefabDB.GetPieceTable(Constants.PieceTables.Hammer) is not { } hammer)
			throw new Exception("Unable to get hammer piece table");

		foreach (var piece in hammer.m_pieces.AsComponents<Piece>()) {
			if (m_blackList.Contains(piece))
				continue;

			yield return piece;
		}
	}

	protected override void Apply(Piece piece)
	{
		piece.m_allowedInDungeons = true;
	}

	protected override void Remove(Piece piece)
	{
		piece.m_allowedInDungeons = false;
	}
}
