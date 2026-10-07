// SPDX-License-Identifier: GPL-3.0-or-later
namespace StolenByte.ValheimX;

// =============================================================================
[Mod]
public sealed partial class Mod : BaseUnityPlugin
{
	public Mod()
	{
		Log.Init(Logger);
	}

	public void Awake()
	{
		Config.BindAll();
		ItemPatcher.BindPatches(Config);
		StaticInit.InitAll(Logger);
		PatchAll();
	}

	public void OnDestroy()
	{
		Config.Save();
		Log.Destroy();
	}
}
