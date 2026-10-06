// SPDX-License-Identifier: GPL-3.0-or-later
namespace StolenByte.ValheimX;

using Biome = Heightmap.Biome;
using ItemType = ItemDrop.ItemData.ItemType;
using EnvMap = Dictionary<string, HashSet<string>>;

// =============================================================================
[ConsoleCommand(
	"Dump all items/environments to the specified directory, each categorised in its own file",
	IsDev = false
)]
static partial class DumpObjects
{
	static readonly Encoding Encoding = new UTF8Encoding(false);

	static bool Execute(Terminal ctx, string path)
	{
		if (Player.m_localPlayer is null)
			return false;

		if (ObjectDB.instance is not { } odb || ZNetScene.instance is not { } znet)
			return false;

		if (ZoneSystem.instance is not { } zoneSys || EnvMan.instance is not { } envMan || zoneSys.m_startTime <= 0f)
			return false;

		ctx.AddString($"exporting objects database to '{path}'");
		var sw = System.Diagnostics.Stopwatch.StartNew();

		Directory.CreateDirectory(path);
		var (nCat, nObj) = DumpDatabase(odb, znet, path);
		var (nBiome, nEnv, nSys) = DumpEnvironments(envMan, zoneSys, path);

		sw.Stop();
		ctx.AddString($"  {nObj} objects in {nCat} categories");
		ctx.AddString($"  {nSys} weather systems in {nEnv} environments and {nBiome} biomes");
		ctx.AddString($"done in {sw.Elapsed.ToString("c").TrimEnd('0')}!");

		return true;
	}

	static (int nCat, int nObj) DumpDatabase(ObjectDB odb, ZNetScene znet, string basePath)
	{
		var db = SortDatabase(odb, znet);
		var count = 0;

		foreach (var cat in db) {
			var outPath = Path.Combine(basePath, cat.Key.ToString().ToLowerInvariant() + ".txt");
			using (var writer = new StreamWriter(outPath, false, Encoding)) {
				foreach (var item in cat.Value) {
					WriteItem(writer, item);
					++count;
				}
			}
		}

		return (db.Count, count);
	}

	static (int nBiome, int nEnv, int nSys) DumpEnvironments(EnvMan envMan, ZoneSystem zoneSys, string basePath)
	{
		var data = SortEnvironments(envMan, zoneSys);
		var sb = new StringBuilder();

		using (var writer = new StreamWriter(Path.Combine(basePath, "biomes.txt"), false, Encoding)) {
			foreach (var biome in data.Biomes) {
				writer.WriteLine("{0}:", biome.Key);

				if (biome.Value.Count == 0) {
					writer.WriteLine("  -");
				} else {
					foreach (var env in biome.Value) {
						writer.Write("  {0} [", env.Key);

						foreach (var system in env.Value.Intersperse(", ")) {
							writer.Write(system);
						}

						writer.WriteLine(']');
					}
				}
			}
		}

		using (var writer = new StreamWriter(Path.Combine(basePath, "env.txt"), false, Encoding)) {
			foreach (var name in data.Environments) {
				WriteItem(writer, name);
			}
		}

		using (var writer = new StreamWriter(Path.Combine(basePath, "env-systems.txt"), false, Encoding)) {
			foreach (var name in data.Systems) {
				WriteItem(writer, name);
			}
		}

		return (data.Biomes.Count, data.Environments.Length, data.Systems.Length);
	}

	static IReadOnlyDictionary<Category, List<string>> SortDatabase(ObjectDB odb, ZNetScene znet)
	{
		var seen = new HashSet<string>();
		var result = new Dictionary<Category, List<string>>();

		result.Add(Category.Recipes, GetNames(PrefabDB.Recipes, seen));
		result.Add(Category.PieceTables, GetNames(PrefabDB.PieceTables, seen));

		foreach (var (hash, obj) in odb.m_itemByHash.Concat(znet.m_namedPrefabs)) {
			if (!seen.Add(obj.name))
				continue;

			var cat = GetCategory(obj);
			result.GetOrAddDefault(cat).Add(obj.name);
		}

		return result;
	}

	static EnvData SortEnvironments(EnvMan envMan, ZoneSystem zoneSys)
	{
		var biomes = new Dictionary<Biome, EnvMap>();
		var envNames = new SortedSet<string>();
		var sysNames = new SortedSet<string>();

		foreach (var setup in envMan.m_biomes) {
			var envs = biomes.GetOrAddDefault(setup.m_biome);

			foreach (var env in setup.m_environments) {
				var systems = envs.GetOrAddDefault(env.m_environment);

				envNames.Add(env.m_environment);
				foreach (var system in env.m_env.m_psystems) {
					systems.Add(system.name);
					sysNames.Add(system.name);
				}
			}
		}

		return new(biomes, envNames.ToImmutableArray(), sysNames.ToImmutableArray());
	}

	static List<string> GetNames<T>(IEnumerable<T> items, HashSet<string> seen)
		where T : UnityEngine.Object
	{
		var result = new List<string>();
		foreach (var item in items) {
			if (!seen.Add(item.name))
				continue;
			result.Add(item.name);
		}

		result.Sort();
		return result;
	}

	static Category GetCategory(GameObject obj)
	{
		if (IsPickable(obj))
			return Category.Pickables;

		if (obj.GetComponent<ItemDrop>() is { } itemDrop) {
			var itemData = itemDrop.m_itemData;
			var sharedData = itemData.m_shared;

			var result = sharedData.m_itemType switch {
				// Note: This includes enemy "attacks" too, no way that i can see to differentiate them
				ItemType.Bow or ItemType.OneHandedWeapon or ItemType.Shield or
				ItemType.TwoHandedWeapon or ItemType.TwoHandedWeaponLeft => Category.Weapons,

				ItemType.Chest or ItemType.Hands or ItemType.Helmet or
				ItemType.Legs or ItemType.Shoulder => Category.Armour,

				ItemType.Tool or ItemType.Torch => Category.Equipment,
				ItemType.Ammo or ItemType.AmmoNonEquipable => Category.Ammo,

				ItemType.Consumable => Category.Consumables,
				ItemType.Trophy => Category.Trophies,
				ItemType.Material => Category.Materials,
				ItemType.Trinket => Category.Trinkets,
				ItemType.Utility => Category.Utility,

				_ => Category.Invalid,
			};

			if (result != Category.Invalid)
				return result;

			if (itemData.IsEquipable())
				return Category.Equipment;

			if (itemDrop.IsPiece())
				return Category.Pieces;
		}

		var piece = obj.GetComponent<Piece>();
		if (piece is not null)
			return Category.Pieces;

		return Category.Other;
	}

	static void WriteItem(StreamWriter writer, string data)
	{
		var hash = data.GetStableHashCode();
		writer.WriteLine($"0x{hash:x8} {hash,11} {data}");
	}

	static bool IsPickable(GameObject obj)
		=> (obj.GetComponent<Pickable>() is not null) || (obj.GetComponent<PickableItem>() is not null);

	// =============================================================================
	private enum Category : int
	{
		Invalid = -1,
		Ammo = 0,
		Armour,
		Weapons,
		Equipment,
		Trophies,
		Consumables,
		Materials,
		Pieces,
		Recipes,
		Pickables,
		PieceTables,
		Trinkets,
		Utility,
		Other,
	};

	readonly record struct EnvData(
		IReadOnlyDictionary<Biome, EnvMap> Biomes,
		ImmutableArray<string> Environments,
		ImmutableArray<string> Systems
	);
}
