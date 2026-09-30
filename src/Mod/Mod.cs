// SPDX-License-Identifier: GPL-3.0-or-later
namespace StolenByte.ValheimX;

// =============================================================================
[Mod]
public sealed partial class Mod : BaseUnityPlugin
{
	public void Awake()
	{
		PatchAll();
	}
}
