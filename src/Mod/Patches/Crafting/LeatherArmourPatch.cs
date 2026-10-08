// SPDX-License-Identifier: GPL-3.0-or-later
namespace StolenByte.ValheimX;

using Constants;

// =============================================================================
[ItemPatch(ConfigSections.Crafting, "LeatherNoBones", "Leather armour requires leather instead of bones")]
sealed class LeatherArmourPatch : ItemPatch<LeatherArmourPatch.MaterialReplacement>
{
	private static readonly string[] m_recipes = [
		Recipes.ArmorLeatherChest,
		Recipes.ArmorLeatherLegs,
		Recipes.ArmorLeatherHelmet,
		Recipes.CapeDeerHide,
	];

	private ItemDrop m_leatherScraps = null!;
	private ItemDrop m_boneFragments = null!;

	protected override void OnInit()
	{
		m_leatherScraps = PrefabDB.GetItem(Materials.LeatherScraps)
			?? throw new Exception("Unable to find leather scraps in PrefabDB");

		m_boneFragments = PrefabDB.GetItem(Materials.BoneFragments)
			?? throw new Exception("Unable to find bone fragments in PrefabDB");
	}

	protected override IEnumerable<MaterialReplacement> GetItems()
	{
		foreach (var name in m_recipes) {
			if (PrefabDB.GetRecipe(name) is { } recipe) {
				if (GetReplacement(recipe, m_boneFragments) is { } replacement) {
					yield return replacement;
				}
			} else {
				throw new Exception($"Unable to find recipe '{name}' in PrefabDB");
			}
		}
	}

	protected override void Apply(MaterialReplacement repl)
	{
		repl.Resource.m_resItem = m_leatherScraps;
	}

	protected override void Remove(MaterialReplacement repl)
	{
		repl.Resource.m_resItem = repl.Original;
	}

	static MaterialReplacement? GetReplacement(Recipe recipe, ItemDrop toReplace)
	{
		foreach (var req in recipe.m_resources) {
			if (ObjectComparer.Default.Equals(req.m_resItem, toReplace)) {
				return new(req);
			}
		}
		return null;
	}

	public sealed record MaterialReplacement(Piece.Requirement Resource, ItemDrop Original)
	{
		public MaterialReplacement(Piece.Requirement req)
			: this(req, req.m_resItem)
		{ }
	}
}
