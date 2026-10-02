// SPDX-License-Identifier: GPL-3.0-or-later
namespace StolenByte.ValheimX;

// =============================================================================
public static class ItemExtensions
{
	public static PieceTable? GetPieceTable(this ItemDrop tool)
	{
		if (tool.m_itemData.m_shared.m_buildPieces is { } pieceTable)
			return pieceTable;
		return null;
	}
}
