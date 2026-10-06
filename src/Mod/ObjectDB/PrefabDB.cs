// SPDX-License-Identifier: GPL-3.0-or-later
namespace StolenByte.ValheimX;

// =============================================================================
[HarmonyPatch]
public static class PrefabDB
{
	private static readonly ConcurrentDictionary<int, Recipe> m_recipes = new();
	private static readonly ConcurrentDictionary<int, PieceTable> m_pieceTables = new();

	public class UpdateEventArgs : EventArgs
	{
		public required bool HasItems { get; init; }
	}

	public static event EventHandler<UpdateEventArgs>? Updated;

	public static IEnumerable<Recipe> Recipes => m_recipes.Values;
	public static IEnumerable<PieceTable> PieceTables => m_pieceTables.Values;

	public static GameObject? GetPrefab(int hash)
		=> GetPrefabFrom(ZNetScene.instance?.m_namedPrefabs, hash)
				?? GetPrefabFrom(ObjectDB.instance?.m_itemByHash, hash);

	public static GameObject? GetPrefab(string name)
		=> GetPrefab(name.GetStableHashCode());

	public static ItemDrop? GetItem(int hash)
		=> GetPrefab(hash)?.GetComponent<ItemDrop>();

	public static ItemDrop? GetItem(string name)
		=> GetItem(name.GetStableHashCode());

	public static Recipe? GetRecipe(int hash)
		=> m_recipes.TryGetValue(hash, out var result) ? result : null;

	public static Recipe? GetRecipe(string name)
		=> GetRecipe(name.GetStableHashCode());

	public static PieceTable? GetPieceTable(int hash)
		=> m_pieceTables.TryGetValue(hash, out var result) ? result : null;

	public static PieceTable? GetPieceTable(string name)
		=> GetPieceTable(name.GetStableHashCode());

	static GameObject? GetPrefabFrom(IReadOnlyDictionary<int, GameObject>? dict, int hash)
		=> dict?.TryGetValue(hash, out var result) == true ? result : null;

	[HarmonyPatch(typeof(ObjectDB), nameof(ObjectDB.Awake)), HarmonyPostfix, HarmonyPriority(Priority.First)]
	static void Awake()
	{
		if (Game.IsShuttingDown())
			return;

		try {
			var nItems = 0;
			var nTables = 0;
			var nRecipes = 0;

			m_recipes.Clear();
			m_pieceTables.Clear();

			if (ObjectDB.instance is { } odb) {
				foreach (var kvp in odb.m_itemByHash) {
					if (kvp.Value.GetComponent<ItemDrop>() is { } item) {
						if (item.GetPieceTable() is { } table) {
							var hash = table.name.GetStableHashCode();
							m_pieceTables[hash] = table;
						}
						++nItems;
					}
				}

				// recipes are stored as a list, which currently sits at almost 500 entries,
				// which warrants a dictionary lookup.
				foreach (var recipe in odb.m_recipes) {
					var hash = recipe.name.GetStableHashCode();
					m_recipes[hash] = recipe;
				}

				nTables = m_pieceTables.Count;
				nRecipes = m_recipes.Count;
			}

			var hasItems = nItems > 0;
			if (hasItems)
				Log.Debug($"PrefabDB Updated: {nItems} items, {nRecipes} recipes, {nTables} piece tables");
			else
				Log.Debug("PrefabDB Cleared");

			if (Updated is { } onUpdated)
				onUpdated(null, new UpdateEventArgs { HasItems = hasItems });
		} catch (Exception ex) {
			Log.Error(ex);
		}
	}
}
