// SPDX-License-Identifier: GPL-3.0-or-later
namespace StolenByte.ValheimX;

// =============================================================================
interface IItemPatch
{
	string Name { get; }
	bool Enabled { get; }

	void Init();
	void Reset();
	void Apply();
	void Remove();
}

// =============================================================================
public abstract class ItemPatch<T> : IItemPatch
{
	static ConfigEntry<bool> m_enabled = null!;

	private ImmutableArray<T>? m_items = null;
	private bool m_applied = false;

	public string Name { get; }

	protected ImmutableArray<T> Items => m_items ??= LoadItems();

	public bool Enabled
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => m_enabled?.Value ?? true;
	}

	protected ItemPatch()
	{
		Name = GetType().DisplayName();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Init()
		=> OnInit();

	public void Reset()
	{
		m_items = null;
	}

	public void Apply()
	{
		if (!m_applied) {
			foreach (var item in Items)
				Apply(item);
			m_applied = true;
		}
	}

	public void Remove()
	{
		if (m_applied) {
			foreach (var item in Items)
				Remove(item);
			m_applied = false;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal ItemPatch<T> Configure(ConfigFile cfg, string? section, string key, string description)
	{
		m_enabled = cfg.Bind(section ?? Name, key, true, description);
		m_enabled.SettingChanged += EnabledChanged;
		return this;
	}

	protected virtual void OnInit()
	{ }

	protected abstract void Apply(T item);
	protected abstract void Remove(T item);
	protected abstract IEnumerable<T> GetItems();

	protected virtual void OnEnabledChanged(bool enabled)
	{ }

	ImmutableArray<T> LoadItems()
	{
		var result = new List<T>();
		foreach (var item in GetItems()) {
			result.Add(item);
		}
		return result.ToImmutableArray();
	}

	void EnabledChanged(object _, EventArgs __)
	{
		if (m_enabled.Value) {
			Apply();
		} else {
			Remove();
		}
		OnEnabledChanged(m_enabled.Value);
	}
}
