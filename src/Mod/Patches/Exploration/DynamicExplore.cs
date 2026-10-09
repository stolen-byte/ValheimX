// SPDX-License-Identifier: GPL-3.0-or-later
namespace StolenByte.ValheimX;

using Biome = Heightmap.Biome;

// =============================================================================
// based on https://github.com/searica/ExplorersVision, but heavily modified.
[HarmonyPatch, BindConfig(ConfigSections.Exploration)]
static partial class DynamicExplore
{
	const float RadiusMin = 40f;
	const float RadiusMax = 400f; // unlikely to go this high, but any greater is pointless due to draw distance
	const float AltitudeMultiplier = 0.5f;
	const float ForestMultiplier = 0.3f;
	const float LightMultiplier = 0.2f;
	const float FogMultiplier = 0.5f;

	static readonly IReadOnlyDictionary<string, VisInfo> m_visPenalties
		= new Dictionary<string, VisInfo>(ObjectNameComparer.Default) {
		{ "Mist", new(0.5f, false) },
		{ "Ashlands_Misty", new(0.5f, false) },
		{ "Ashlands_AshRain", new(0.9f, true) },
		{ "Ashlands_CinderRain", new(0.2f, false) },
		{ "Ashlands_storm", new(0.8f, false) },
		{ "SnowStorm", new(0.7f, true) },
	};

	[ConfigValue("DynamicExplore", "Enable dynamic minimap exploration")]
	static ConfigEntry<bool> m_enabled = null!;

	[ConfigValue(
		"DynamicLandRadius",
		"Base radius on land before modifiers are applied",
		Min = RadiusMin,
		Max = 350f,
		Default = 150f
	)]
	static ConfigEntry<float> m_landRadius = null!;

	[ConfigValue(
		"DynamicSeaRadius",
		"Base radius at sea before modifiers are applied",
		Min = RadiusMin,
		Max = 350f,
		Default = 300f
	)]
	static ConfigEntry<float> m_seaRadius = null!;

	[HarmonyPatch(typeof(Minimap), nameof(Minimap.UpdateExplore)), HarmonyPrefix]
	static bool UpdateExplore(Minimap __instance, float dt, Player player)
	{
		if (!m_enabled.Value)
			return true;

		ref var exploreTimer = ref __instance.m_exploreTimer;

		if (player.InInterior()) {
			exploreTimer = .0f; // disable uncovering minimap while in a cave/dungeon
		} else {
			exploreTimer += dt;
			if (exploreTimer > __instance.m_exploreInterval) {
				exploreTimer = .0f;
				__instance.Explore(player.transform.position, GetExploreRadius(player));
			}
		}

		return false;
	}

	static float GetExploreRadius(Player player)
	{
		var biome = player.GetCurrentBiome();
		var radius = RadiusMin;

		if (biome == Biome.Mistlands) {
			if (player.HasItemEquipped(Constants.Utilities.Demister)) {
				// Would rather use Demister.m_forceField.endRange directly, however the demister in the
				// inventory is not linked to a valid Demister component...
				// ItemData.m_dropPrefab > Particle System (child) > Demister Component -> m_forceField == null
				radius *= WispRadius.FieldRange.Value / 10f;
			}
		} else {
			var baseRadius = player.IsInShip() ? m_seaRadius.Value : m_landRadius.Value;
			var mult = GetEnvMultiplier(EnvMan.instance, ZoneSystem.instance, biome, player.transform.position);
			radius = baseRadius * Mathf.Clamp(mult, .2f, 5f);
		}

		return Mathf.Clamp(radius, RadiusMin, RadiusMax);
	}

	static float GetEnvMultiplier(EnvMan envMan, ZoneSystem zoneSys, Biome biome, Vector3 position)
	{
		var result = 1f;

		// directional/ambient light
		var dirLight = envMan.m_dirLight.color * envMan.m_dirLight.intensity;
		var light = Mathf.Max(dirLight.Magnitude(), RenderSettings.ambientLight.Magnitude());
		result += (light - 1f) * LightMultiplier;

		// visibility penalty from active weather systems
		// NOTE: we check the systems themselves, instead of just the current environment,
		// because the current env changes instantly, whilst the systems transition between eachother,
		// this keeps things smooth.
		var visPenalty = .0f;
		foreach (var sys in envMan.GetCurrentEnvironment().m_psystems) {
			visPenalty += GetVisibilityPenalty(sys, light);
		}

		// fog
		result += -Mathf.Clamp(RenderSettings.fogDensity * 10f + visPenalty, .0f, 1.5f) * FogMultiplier;

		// altitude
		var yOffset = position.y - zoneSys.m_waterLevel;
		var altitude = (Mathf.Clamp(yOffset, .0f, 300f) / 100f * Mathf.Max(.05f, 1f - visPenalty)) * AltitudeMultiplier;
		result += altitude;

		// forest
		result += GetForestPenalty(biome, position, -(ForestMultiplier * (1f + altitude)));

		return result;
	}

	static float GetForestPenalty(Biome biome, Vector3 position, float @base)
	{
		if (biome == Biome.Meadows && WorldGenerator.InForest(position))
			return @base;

		if (biome == Biome.BlackForest)
			return @base - .25f * LightMultiplier;

		if (biome == Biome.Plains) {
			var forestMult = WorldGenerator.GetForestFactor(position) < .8f
				? @base
				: .0f;
			return .1f * LightMultiplier * forestMult;
		}

		return 0f;
	}

	static float GetVisibilityPenalty(GameObject envSystem, float light)
	{
		if (!m_visPenalties.TryGetValue(envSystem.name, out var visInfo))
			return 0.0f;

		var result = visInfo.Penalty;
		if (visInfo.UseLight)
			result *= light;

		return result;
	}

	readonly record struct VisInfo(float Penalty, bool UseLight);
}
