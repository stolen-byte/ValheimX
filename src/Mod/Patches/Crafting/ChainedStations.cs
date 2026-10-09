// SPDX-License-Identifier: GPL-3.0-or-later
namespace StolenByte.ValheimX;

// =============================================================================
[HarmonyPatch, BindConfig(ConfigSections.Crafting)]
static partial class ChainedStations
{
	static int m_cacheKey = 0;
	static int m_visitGen = 0;
	static int[] m_visited = new int[0];
	static int[] m_origins = new int[0];
	static CraftingStation? m_cachedResult = null;

	[ConfigValue("ChainedStations", "Crafting station build ranges chain from each other")]
	static ConfigEntry<bool> m_enabled = null!;

	[HarmonyPatch(typeof(CraftingStation), nameof(CraftingStation.HaveBuildStationInRange)), HarmonyPrefix]
	static bool HaveBuildStationInRange(string name, Vector3 point, ref CraftingStation? __result)
	{
		if (!m_enabled.Value)
			return true;

		var key = (name, (int)point.x, (int)point.z, CraftingStation.m_allStations.Count).GetHashCode();
		if (key != m_cacheKey) {
			m_cacheKey = key;
			m_cachedResult = FindStationChained(name, point, CraftingStation.m_allStations);
		}

		__result = m_cachedResult;
		return false;
	}

	// Returns the *first* station in the chain to a matching station, or null.
	static CraftingStation? FindStationChained(string name, Vector3 point, List<CraftingStation> allStations)
	{
		int n = allStations.Count;

		if (m_visited.Length < n) {
			Array.Resize(ref m_visited, n);
			Array.Resize(ref m_origins, n);
		}

		if (m_visitGen == int.MaxValue) {
			Array.Clear(m_visited, 0, m_visited.Length);
			m_visitGen = 1;
		} else {
			m_visitGen++;
		}

		var q = new Queue<int>(n);

		// search stations in immediate range
		for (var i = 0; i < n; ++i) {
			var cur = allStations[i];

			if (cur.IsInBuildRange(point)) {
				if (cur.m_name == name) {
					return cur;
				}

				m_visited[i] = m_visitGen;
				m_origins[i] = i;
				q.Enqueue(i);
			}
		}

		// BFS search of neighbouring stations
		while (q.Count > 0) {
			var curIdx = q.Dequeue();
			var originIdx = m_origins[curIdx];
			var cur = allStations[curIdx];

			for (var i = 0; i < n; ++i) {
				if (m_visited[i] == m_visitGen) {
					continue;
				}

				var neigh = allStations[i];
				if (cur.IsInBuildRange(neigh.transform.position)) {
					if (neigh.m_name == name) {
						return allStations[originIdx];
					}

					m_visited[i] = m_visitGen;
					m_origins[i] = originIdx;
					q.Enqueue(i);
				}
			}
		}

		return null;
	}
}
