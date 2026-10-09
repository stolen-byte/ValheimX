// SPDX-License-Identifier: GPL-3.0-or-later
namespace StolenByte.ValheimX;

using Biome = Heightmap.Biome;
using ItemType = ItemDrop.ItemData.ItemType;
using UnityObject = UnityEngine.Object;

// =============================================================================
[ConsoleCommand(
	"Dump all items/environments to the specified directory, each categorised in its own file",
	IsDev = false
)]
static partial class DumpObjects
{
	static readonly Encoding Encoding = new UTF8Encoding(false);

	static readonly string[] m_fixAttacks = [
		"Fader_",
		"FrozenKing_",
		"fallenvalkyrie_",
		"JotunWarrior",
		"Asksvin_",
		"bonemass_",
		"Bonemaw",
		"dragon_",
		"Eikthyr_",
		"gd_king",
		"Morgen_",
		"spiritbjorn_",
		"stonegolem_",
		"tick_",
	];

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
		var (nCat, nObj) = DumpPrefabs(odb, znet, path);
		var (nBiomes, nEnvs, nSys) = DumpEnvironments(envMan, zoneSys, path);

		sw.Stop();
		ctx.AddString($"  {nObj} objects in {nCat} categories");
		ctx.AddString($"  {nSys} weather systems in {nEnvs} environments and {nBiomes} biomes");
		ctx.AddString($"done in {sw.Elapsed.ToString("c").TrimEnd('0')}!");
		return true;
	}

	static (int, int) DumpPrefabs(ObjectDB odb, ZNetScene znet, string basePath)
	{
		var db = GetPrefabs(odb, znet);
		var nObjects = 0;

		foreach (var (cat, objects) in db) {
			var outPath = Path.Combine(basePath, cat.ToString().ToLowerInvariant() + ".txt");
			using (var writer = new StreamWriter(outPath, false, Encoding)) {
				objects.Sort();

				foreach (var name in objects) {
					WriteItem(writer, name);
					++nObjects;
				}
			}
		}

		return (db.Count, nObjects);
	}

	static (int, int, int) DumpEnvironments(EnvMan envMan, ZoneSystem zoneSys, string basePath)
	{
		var biomes = GetEnvironments(envMan, zoneSys);
		var uniqueEnvs = new HashSet<string>();
		var uniqueSys = new HashSet<string>();

		using (var writer = new StreamWriter(Path.Combine(basePath, "biomes.txt"), false, Encoding)) {
			foreach (var (biome, environments) in biomes) {
				writer.WriteLine("{0}:", biome);

				if (environments.Count == 0) {
					writer.WriteLine("  -");
				} else {
					foreach (var (env, systems) in environments) {
						systems.Sort();
						uniqueEnvs.Add(env);
						uniqueSys.AddRange(systems);

						writer.Write("  {0} [", env);
						foreach (var system in systems.Intersperse(", "))
							writer.Write(system);
						writer.WriteLine(']');
					}
				}
			}
		}

		return (biomes.Count, uniqueEnvs.Count, uniqueSys.Count);
	}

	static IReadOnlyDictionary<Biome, SortedDictionary<string, List<string>>> GetEnvironments(
		EnvMan envMan,
		ZoneSystem zoneSys)
	{
		var biomes = new SortedDictionary<Biome, SortedDictionary<string, List<string>>>();

		foreach (var setup in envMan.m_biomes) {
			var biome = biomes.GetOrAddDefault(setup.m_biome);

			foreach (var env in setup.m_environments) {
				var envSystems = biome.GetOrAddDefault(env.m_environment);

				foreach (var system in env.m_env.m_psystems) {
					envSystems.Add(system.name);
				}
			}
		}

		return biomes;
	}

	static IReadOnlyDictionary<Category, List<string>> GetPrefabs(ObjectDB odb, ZNetScene znet)
	{
		var seen = new HashSet<string>();
		var result = new Dictionary<Category, List<string>>();

		AddEntities(znet, result, seen);
		result.Add(Category.Recipes, GetUniqueNames(PrefabDB.Recipes, seen).ToList());
		result.Add(Category.PieceTables, GetUniqueNames(PrefabDB.PieceTables, seen).ToList());

		foreach (var obj in odb.m_itemByHash.Values.Concat(znet.m_namedPrefabs.Values)) {
			if (!seen.Add(obj.name))
				continue;

			var cat = GetCategory(obj);
			result.GetOrAddDefault(cat).Add(obj.name);
		}

		return result;
	}

	static void AddEntities(ZNetScene znet, IDictionary<Category, List<string>> result, HashSet<string> seen)
	{
		var animals = result.GetOrAddDefault(Category.Animals);
		var monsters = result.GetOrAddDefault(Category.Monsters);
		var attacks = result.GetOrAddDefault(Category.Attacks);
		var equipment = result.GetOrAddDefault(Category.MonsterEquipment);

		foreach (var obj in znet.m_namedPrefabs.Values) {
			if (obj.GetComponent<AnimalAI>() is not null) {
				if (seen.Add(obj.name))
					animals.Add(obj.name);
				continue;
			}

			if (obj.GetComponent<MonsterAI>() is null || obj.GetComponent<Humanoid>() is not { } monster)
				continue;
			if (!seen.Add(obj.name))
				continue;

			monsters.Add(obj.name);

			var gear = ConcatAll(
				monster.m_defaultItems,
				monster.m_randomShield,
				monster.m_randomItems?.Select(x => x.m_prefab),
				monster.m_randomSets?.SelectMany(x => x.m_items)
			);

			foreach (var piece in gear) {
				if (seen.Add(piece.name)) {
					if (IsAttack(piece))
						attacks.Add(piece.name);
					else
						equipment.Add(piece.name);
				}
			}

			if (monster.m_randomWeapon is not null)
				attacks.AddRange(GetUniqueNames(monster.m_randomWeapon, seen));
		}
	}

	static Category GetCategory(GameObject obj)
	{
		if (IsPickable(obj))
			return Category.Pickables;

		if (obj.GetComponent<Projectile>() is not null)
			return Category.Attacks;

		if (obj.GetComponent<ItemDrop>() is { } itemDrop) {
			var itemData = itemDrop.m_itemData;
			var sharedData = itemData.m_shared;

			var result = sharedData.m_itemType switch {
				ItemType.Bow => Category.Bows,
				ItemType.OneHandedWeapon => Category.OneHandedWeapons,
				ItemType.TwoHandedWeapon or ItemType.TwoHandedWeaponLeft => Category.TwoHandedWeapons,

				ItemType.Shield => Category.Shields,
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
				return FixCategory(itemDrop, result);

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

	static Category FixCategory(ItemDrop item, Category oldCategory)
	{
		// Try and limit this as much as possible, there is currently only around ~100 or so of each of these,
		// so the impact *should* be minimal.
		// Normally i wouldn't resort to this string-matching nonsense, however it was bugging me too much.
		if (oldCategory == Category.OneHandedWeapons || oldCategory == Category.TwoHandedWeapons) {
			// bare foreach instead of `.Any(...)` to avoid captures
			foreach (var str in m_fixAttacks) {
				if (item.name.Contains(str))
					return Category.Attacks;
			}
		}
		return oldCategory;
	}


	static bool IsAttack(GameObject go)
	{
		if (go.GetComponent<Projectile>() is not null)
			return true;
		if (go.GetComponent<ItemDrop>() is { } item && item.m_itemData.IsWeapon())
			return true;
		return false;
	}

	static bool IsPickable(GameObject obj)
		=> (obj.GetComponent<Pickable>() is not null) || (obj.GetComponent<PickableItem>() is not null);

	static IEnumerable<string> GetUniqueNames<T>(IEnumerable<T> objects, HashSet<string> seen)
		where T : UnityObject
	{
		foreach (var obj in objects) {
			if (!seen.Add(obj.name))
				continue;
			yield return obj.name;
		}
	}

	static IEnumerable<T> ConcatAll<T>(params IEnumerable<T?>?[] others)
		where T : class
	{
		var result = Enumerable.Empty<T>();
		foreach (var enumerable in others) {
			if (enumerable is null)
				continue;
			result = result.Concat(enumerable.NonNull());
		}
		return result;
	}

	static void WriteItem(StreamWriter writer, string data)
	{
		var hash = data.GetStableHashCode();
		writer.WriteLine($"0x{hash:x8} {hash,11} {data}");
	}

	// =============================================================================
	enum Category
	{
		Invalid = -1,
		Ammo = 0,
		Armour,
		Attacks,
		Bows,
		OneHandedWeapons,
		TwoHandedWeapons,
		Shields,
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
		Animals,
		Monsters,
		MonsterEquipment,
		Other,
	}
}
