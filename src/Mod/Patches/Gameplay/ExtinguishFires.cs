// SPDX-License-Identifier: GPL-3.0-or-later
namespace StolenByte.ValheimX;

using Constants;

// =============================================================================
[HarmonyPatch]
static class ExtinguishFires
{
	const string Extinguish = nameof(Extinguish);
	const string Light = nameof(Light);

	static readonly ObjectSet m_firePlaces = new(
		Pieces.Hammer.FirePit,
		Pieces.Hammer.FirePitIron,
		Pieces.Hammer.Hearth,
		Pieces.Hammer.StandingIronTorch,
		Pieces.Hammer.StandingWoodTorch,
		Pieces.Hammer.StandingIronTorchBlue,
		Pieces.Hammer.StandingIronTorchGreen,
		Pieces.Hammer.HangingBrazier,
		Pieces.Hammer.StandingBrazier,
		Pieces.Hammer.StandingBrazierBlue,
		Pieces.Hammer.Sconce
	);

	[HarmonyPatch(typeof(Fireplace), nameof(Fireplace.Interact)), HarmonyPrefix]
	static bool Interact(Fireplace __instance, Humanoid user, bool hold, bool alt)
	{
		if (!alt || __instance.m_canTurnOff || !m_firePlaces.Contains(__instance))
			return true;

		if (!hold && GetFuel(__instance) > 0f) {
			__instance.m_nview.InvokeRPC("RPC_ToggleOn");
		}

		return false;
	}

	[HarmonyPatch(typeof(Fireplace), nameof(Fireplace.GetHoverText)), HarmonyPostfix]
	static string GetHoverText(string text, Fireplace __instance)
	{
		if (!__instance.m_canTurnOff && m_firePlaces.Contains(__instance)) {
			if (GetFuel(__instance) > 0f) {
				var isLit = GetState(__instance) == 1;
				var keyBind = Localization.instance.Localize("$KEY_AltPlace + $KEY_Use");
				return $"{text}\n[<color=yellow><b>{keyBind}</b></color>] {(isLit ? Extinguish : Light)}";
			}
		}
		return text;
	}

	static float GetFuel(Fireplace fire)
		=> fire.m_nview.GetZDO().GetFloat(ZDOVars.s_fuel);

	static int GetState(Fireplace fire, int defaultValue = 1)
		=> fire.m_nview.GetZDO().GetInt(ZDOVars.s_state, defaultValue);
}
