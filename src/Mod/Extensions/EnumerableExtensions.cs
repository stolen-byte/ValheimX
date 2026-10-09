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

	public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> source)
	{
		foreach (var item in source)
			collection.Add(item);
	}

	public static IEnumerable<T> NonNull<T>(this IEnumerable<T?> enumerable)
		where T : class
		=> enumerable.Where(static (x) => x is not null)!;
}
