// SPDX-License-Identifier: GPL-3.0-or-later
namespace StolenByte.ValheimX;

// =============================================================================
[ItemPatch(ConfigSections.Crafting, "BuildInDungeons", "Allow building some structures in dungeons")]
sealed class BuildInDungeons : ItemPatch<Piece>
{
	static readonly ObjectSet m_blackList = new(
		"Raft",
		"Karve",
		"VikingShip",
		"VikingShip_Ashlands",
		"smelter",
		"blastfurnace",
		"charcoal_kiln",
		"incinerator",
		"Catapult",
		"piece_beehive",
		"piece_birdnest",
		"piece_maypole",
		"piece_shieldgenerator",
		"piece_wisplure",
		"windmill",
		"piece_drawbridge",
		"piece_drawbridge_log",
		"sapling_barley",
		"sapling_carrot",
		"sapling_flax",
		"sapling_jotunpuffs",
		"sapling_Kale",
		"sapling_magecap",
		"sapling_oat",
		"sapling_onion",
		"sapling_poteitr",
		"sapling_seedcarrot",
		"sapling_seedkale",
		"sapling_seedonion",
		"sapling_seedturnip",
		"sapling_turnip",
		"Beech_Sapling",
		"Birch_Sapling",
		"FirTree_big_Sapling",
		"FirTree_Sapling",
		"Oak_Sapling",
		"PineTree_Sapling",
		"VineAsh_sapling",
		"VineGreen_sapling"
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
