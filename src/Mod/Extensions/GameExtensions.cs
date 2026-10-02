// SPDX-License-Identifier: GPL-3.0-or-later
namespace StolenByte.ValheimX;

// =============================================================================
public static class GameExtensions
{
	extension(Game)
	{
		public static bool IsShuttingDown()
			=> Game.instance is not null && Game.instance.IsShuttingDown();
	}
}
