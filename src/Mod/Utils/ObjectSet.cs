// SPDX-License-Identifier: GPL-3.0-or-later
namespace StolenByte.ValheimX;

// =============================================================================
/// <summary>
/// Fixed-size collection which stores only the name hashcodes of objects.
/// </summary>
/// <remarks>
/// Useful for simple lookups where you only need to know if the set contains an object,
/// not what the object is.
/// </remarks>
public class ObjectSet : IEnumerable<int>
{
	private readonly int[] m_hashes;

	public int Length
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => m_hashes.Length;
	}

	public bool IsEmpty
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => Length == 0;
	}

	public ObjectSet(params ReadOnlySpan<int> hashCodes)
	{
		m_hashes = new int[hashCodes.Length];
		for (var i = 0; i < hashCodes.Length; ++i) {
			InsertSorted(i, hashCodes[i]);
		}
	}

	public ObjectSet(params ReadOnlySpan<string> names)
	{
		m_hashes = new int[names.Length];
		for (var i = 0; i < names.Length; ++i) {
			InsertSorted(i, names[i].GetStableHashCodeBase());
		}
	}

	public bool Contains(int hashCode)
	{
		// standard binary search
		int min = 0;
		int max = Length - 1;

		while (min <= max) {
			// not using standard `(min + max) / 2`
			// because: in theory, `m` can overflow if min+max exceeds `int.MaxValue`.
			// However unlikely that is, we can't guarantee it.
			int m = min + ((max - min) >> 1);
			int h = m_hashes[m];

			if (h < hashCode) {
				min = m + 1;
			} else if (h > hashCode) {
				max = m - 1;
			} else {
				return true;
			}
		}

		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Contains(string name)
		=> Contains(name.GetStableHashCodeBase());

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Contains(UnityEngine.Object obj)
		=> Contains(obj.name.GetStableHashCodeBase());

	/// <summary>
	/// guarantees hashes are sorted in ascending order, as long as it is
	/// called with an increasing lastIndex, with no gaps.
	/// </summary>
	private void InsertSorted(int lastIndex, int hashCode)
	{
		int j = lastIndex;
		while (j > 0 && m_hashes[j - 1] > hashCode)
			m_hashes[j--] = m_hashes[j];
		m_hashes[j] = hashCode;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public IEnumerator<int> GetEnumerator()
		=> ((IEnumerable<int>)m_hashes).GetEnumerator();

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	IEnumerator IEnumerable.GetEnumerator()
		=> GetEnumerator();
}
