// SPDX-License-Identifier: GPL-3.0-or-later
namespace StolenByte.ValheimX;

using Object = UnityEngine.Object;

// =============================================================================
public static class GameObjectExtensions
{
	public static IEnumerable<T> AsComponents<T>(this IEnumerable<GameObject> objects)
		where T : notnull, Component
	{
		foreach (var obj in objects) {
			if (obj.GetComponent<T>() is T component) {
				yield return component;
			}
		}
	}

	public static T GetOrAddComponent<T>(this GameObject obj)
		where T : notnull, Component
	{
		if (obj.TryGetComponent<T>(out var comp))
			return comp;
		return obj.AddComponent<T>();
	}

	/// <summary>
	/// Properly destroys an in-game object, using either its <see cref="Destructible"/> component,
	/// or <see cref="ZNetScene"/>
	/// </summary>
	public static void TriggerDestroy(this GameObject obj)
	{
		if (obj.GetComponent<Destructible>() is { } d) {
			d.DestroyNow();
		} else if (ZNetScene.instance is { } znet && obj.GetComponent<ZNetView>() is not null) {
			znet.Destroy(obj);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int GetNameHash(this Object obj)
		=> HashHelper.GetBaseHashCode(obj.name);
}
