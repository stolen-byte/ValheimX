// SPDX-License-Identifier: GPL-3.0-or-later
namespace StolenByte.ValheimX;

// =============================================================================
[Mod]
public sealed partial class Mod : BaseUnityPlugin
{
	public void Awake()
	{
		Config.BindAll();
		PatchAll();
	}

	public void OnDestroy()
	{
		Config.Save();
	}
}
