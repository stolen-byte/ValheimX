// SPDX-License-Identifier: GPL-3.0-or-later
namespace StolenByte.ValheimX;

// =============================================================================
[ConsoleCommand("List names of all objects within the given radius. (max: 50)", IsDev = true)]
static partial class PrintObjects
{
	static bool Execute(Terminal ctx, float radius = 5.0f, string? name = null)
	{
		if (Player.m_localPlayer is null)
			return false;

		var localPos = Player.m_localPlayer.transform.position;

		ctx.AddString($"Objects within {radius}m");
		foreach (var obj in UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None)) {
			if ((obj is null) || Filter(obj))
				continue;

			if ((name is not null) && !obj.name.ToLowerInvariant().Contains(name.ToLowerInvariant()))
				continue;

			var distance = Vector3.Distance(obj.transform.position, localPos);
			if (distance <= radius) {
				ctx.AddString($"  {obj.name} [{distance}m]");
			}
		}

		return true;
	}

	private static bool Filter(GameObject obj)
		=> (obj.GetComponentInParent<Game>() is not null) ||
			(obj.GetComponentInParent<Player>() is not null) ||
			(obj.GetComponentInParent<Valkyrie>() is not null) ||
			(obj.GetComponentInParent<LocationProxy>() is not null) ||
			(obj.GetComponentInParent<DungeonGenerator>() is not null) ||
			(obj.GetComponentInParent<Room>() is not null) ||
			(obj.GetComponentInParent<FollowPlayer>() is not null);
}
