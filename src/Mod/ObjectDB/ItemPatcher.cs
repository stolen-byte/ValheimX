// SPDX-License-Identifier: GPL-3.0-or-later
namespace StolenByte.ValheimX;

// =============================================================================
public static partial class ItemPatcher
{
	static readonly ConcurrentQueue<IItemPatch> m_pending = new();
	static readonly ConcurrentBag<IItemPatch> m_patches = new();
	static bool m_isRunning = false;

	static ItemPatcher()
	{
		PrefabDB.Updated += Update;
	}

	public static void Add<T>(ItemPatch<T> patch)
	{
		m_pending.Enqueue(patch);
		if (m_isRunning) {
			AddPending();
		}
	}

	static void ApplyAll()
	{
		foreach (var patch in m_patches) {
			ApplyPatch(patch);
		}
	}

	static void AddPending()
	{
		while (m_pending.TryDequeue(out var patch)) {
			if (InitPatch(patch) && ApplyPatch(patch)) {
				m_patches.Add(patch);
			}
		}
	}

	static void ResetAll()
	{
		foreach (var patch in m_patches) {
			patch.Reset();
		}
	}

	static bool InitPatch(IItemPatch patch)
	{
		try {
			patch.Init();
			return true;
		} catch (Exception ex) {
			Log.Error($"Exception in {patch.Name}.Init: {ex}");
			return false;
		}
	}

	static bool ApplyPatch(IItemPatch patch)
	{
		try {
			if (patch.Enabled) {
				patch.Apply();
			}
			return true;
		} catch (Exception ex) {
			Log.Error($"Exception in {patch.Name}.Apply: {ex}");
			return false;
		}
	}

	static void Update(object _, PrefabDB.UpdateEventArgs args)
	{
		if (Game.IsShuttingDown())
			return;

		if (args.HasItems) {
			ApplyAll();
			AddPending();
			m_isRunning = true;
		} else {
			ResetAll();
			m_isRunning = false;
		}
	}
}
