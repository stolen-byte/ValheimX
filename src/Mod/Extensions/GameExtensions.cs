// SPDX-License-Identifier: GPL-3.0-or-later
namespace StolenByte.ValheimX;

// =============================================================================
public static class GameExtensions
{
	extension(Game)
	{
		public static bool IsShuttingDown()
			=> Game.instance is not null && Game.instance.IsShuttingDown();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsInShip(this Player player)
		=> IsInShip(player, out var _);

	public static bool IsInShip(this Player player, [NotNullWhen(true)] out Ship? ship)
	{
		if (Ship.GetLocalShip() is { } s && s.IsPlayerInBoat(player)) {
			ship = s;
			return true;
		}
		ship = null;
		return false;
	}

	public static IEnumerable<ItemDrop.ItemData> GetEquippedItems(this Player player)
	{
		// Inventory.GetEquippedItems returns a new List<>, so wasteful
		foreach (var itemData in player.GetInventory().m_inventory) {
			if (itemData.m_equipped) {
				yield return itemData;
			}
		}
	}

	public static ItemDrop.ItemData? GetEquippedItem(this Player player, int hashCode)
		=> GetEquippedItems(player).FirstOrDefault(obj => ObjectComparer.Default.Equals(obj.m_dropPrefab, hashCode));

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ItemDrop.ItemData? GetEquippedItem(this Player player, string name)
		=> GetEquippedItem(player, name.GetStableHashCode());

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool HasItemEquipped(this Player player, int hashCode)
		=> GetEquippedItem(player, hashCode) is not null;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool HasItemEquipped(this Player player, string name)
		=> GetEquippedItem(player, name.GetStableHashCode()) is not null;
}
