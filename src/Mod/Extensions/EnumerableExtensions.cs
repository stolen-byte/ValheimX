// SPDX-License-Identifier: GPL-3.0-or-later
namespace StolenByte.ValheimX;

// =============================================================================
static class EnumerableExtensions
{
	public static IEnumerable<T> Intersperse<T>(this IEnumerable<T> source, T element)
	{
		using (var iter = source.GetEnumerator()) {
			if (iter.MoveNext()) {
				yield return iter.Current;
				while (iter.MoveNext()) {
					yield return element;
					yield return iter.Current;
				}
			}
		}
	}
}
