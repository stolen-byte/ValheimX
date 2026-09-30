// SPDX-License-Identifier: GPL-3.0-or-later
namespace StolenByte.ValheimX.Tasks;

static class Extensions
{
	public static bool IsEmpty(this ReadOnlySpan<char> str)
		=> str.Trim().Length == 0;
}
