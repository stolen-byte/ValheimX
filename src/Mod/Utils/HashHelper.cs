// SPDX-License-Identifier: GPL-3.0-or-later
namespace StolenByte.ValheimX;

// =============================================================================
public static class HashHelper
{
	/// <summary>
	/// Based on in-game `GetStableHashCode`, which seems to be a variant of DJB2.
	/// This version however disregards the text '(Clone)' in the object name, to allow
	/// comparing objects to their base-prefabs.
	/// </summary>
	public static int GetBaseHashCode(string s)
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static bool IsStopChar(char ch)
			=> ch == '\0' || ch == ' ' || ch == '(';

		unchecked {
			int h1 = 5381;
			int h2 = h1;

			for (var i = 0; i < s.Length && !IsStopChar(s[i]); i += 2) {
				h1 = ((h1 << 5) + h1) ^ (int)s[i];

				if (i == s.Length - 1 || IsStopChar(s[i + 1]))
					break;

				h2 = ((h2 << 5) + h2) ^ (int)s[i + 1];
			}

			return h1 + h2 * 1566083941;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int GetStableHashCodeBase(this string s)
		=> HashHelper.GetBaseHashCode(s);
}
