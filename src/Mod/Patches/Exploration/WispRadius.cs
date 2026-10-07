// SPDX-License-Identifier: GPL-3.0-or-later
namespace StolenByte.ValheimX;

// =============================================================================
[HarmonyPatch, BindConfig(ConfigSections.Exploration)]
partial class WispRadius : MonoBehaviour
{
	[ConfigValue("WispLightRadius", "Wisp light range. 0 = game default.", Max = 50f, Default = 20f)]
	public static ConfigEntry<float> FieldRange = null!;

	[HarmonyPatch(typeof(Demister), nameof(Demister.Awake)), HarmonyPostfix]
	static void DemisterAwake(Demister __instance)
	{
		var comp = __instance.gameObject.GetOrAddComponent<WispRadius>();
		// doing this here saves us a GetComponent call
		comp.m_instance = __instance;
		comp.m_default = __instance.m_forceField.endRange;
	}

	// =============================================================================
	private Demister? m_instance;
	private float m_default = 0f;

	void Start()
	{
		OnRangeChanged(null!, EventArgs.Empty);
	}

	void OnEnable()
	{
		FieldRange.SettingChanged += OnRangeChanged;
	}

	void OnDisable()
	{
		FieldRange.SettingChanged -= OnRangeChanged;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	void SetRange(float value)
	{
		if (m_instance is not null) {
			if (value < m_default)
				value = m_default;

			m_instance.m_forceField.endRange = value;
		}
	}

	void OnRangeChanged(object _, EventArgs e)
		=> SetRange(FieldRange.Value);
}
