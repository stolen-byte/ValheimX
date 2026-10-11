// SPDX-License-Identifier: GPL-3.0-or-later
namespace StolenByte.ValheimX;

// =============================================================================
public static class SortedListExtensions
{
	public static SortedList<T> ToSortedList<T>(this IEnumerable<T> items)
	{
		if (items is SortedList<T> list)
			return list;
		return new(items);
	}
}

// =============================================================================
public class SortedList<T> : IReadOnlyList<T>
{
	const int MaxLength = 2147483591;
	const int DefaultCapacity = 16;
	static readonly T[] Empty = new T[0];
	T[] m_items;
	int m_size = 0;
	int m_version = 0;
	IComparer<T> m_comparer;

	public SortedList()
		: this(0, Comparer<T>.Default)
	{
	}

	public SortedList(IComparer<T> comparer)
		: this(0, comparer)
	{
	}

	public SortedList(int capacity)
		: this(capacity, Comparer<T>.Default)
	{
	}

	public SortedList(int capacity, IComparer<T> comparer)
	{
		if (capacity < 0)
			throw new ArgumentOutOfRangeException(nameof(capacity), "capacity cannot be less than zero");

		if (capacity == 0)
			m_items = Empty;
		else
			m_items = new T[capacity];

		m_comparer = comparer;
	}

	public SortedList(IEnumerable<T> items)
		: this(items, Comparer<T>.Default)
	{
	}

	public SortedList(IEnumerable<T> items, IComparer<T> comparer)
		: this(0, comparer)
	{
		AddRange(items);
	}

	public int Count => m_size;
	public int Capacity => m_items.Length;
	public bool IsEmpty => m_size == 0;

	int Available
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => m_items.Length - m_size;
	}

	public T this[int index]
	{
		get
		{
			if ((uint)index >= (uint)m_size)
				throw new ArgumentOutOfRangeException(nameof(index), "index must be less than size");
			return m_items[index];
		}
	}

	public int EnsureCapacity(int capacity)
	{
		if (capacity < 0)
			throw new ArgumentOutOfRangeException(nameof(capacity), "capacity must be >= 0");

		if (m_items.Length < capacity) {
			Grow(capacity);
		}
		return m_items.Length;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Add(T item)
	{
		m_version++;
		if ((uint)m_size >= (uint)m_items.Length) {
			Grow(m_size + 1);
		}

		var j = m_size;
		while (j > 0 && m_comparer.Compare(m_items[j - 1], item) > 0)
			m_items[j--] = m_items[j];

		m_items[j] = item;
		m_size += 1;
	}

	public void AddRange(IEnumerable<T> items)
	{
		// Note: no self-defence against adding from self
		// eg: list.AddRange(list)
		// because: why the hell would you?
		if (items is ICollection<T> coll) {
			var count = coll.Count;
			if (count > 0) {
				if (Available < count) {
					Grow(checked(m_size + count));
				}
			}
		}
		foreach (var item in items) {
			Add(item);
		}
	}

	public bool Remove(T item)
	{
		var idx = IndexOf(item);
		if (idx >= 0) {
			RemoveAt(idx);
			return true;
		}
		return false;
	}

	public void RemoveAt(int index)
	{
		if ((uint)index >= (uint)m_size)
			throw new ArgumentOutOfRangeException(nameof(index), "index must be less than size");

		m_size--;
		if (index < m_size)
			Array.Copy(m_items, index + 1, m_items, index, m_size - index);
		if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
			m_items[m_size] = default!;

		m_version++;
	}

	public void RemoveRange(int index, int count)
	{
		if (index < 0)
			throw new ArgumentOutOfRangeException(nameof(index), "index must be >= 0");
		if (index > m_size)
			throw new ArgumentOutOfRangeException(nameof(index), "index must be < size");
		if (count < 0)
			throw new ArgumentOutOfRangeException(nameof(count), "count must be >= 0");
		if (m_size - index < count)
			throw new ArgumentOutOfRangeException(nameof(count), "index and count were out of bounds for the array, or count is greater than the number of elements from the given index");

		if (count > 0) {
			m_size -= count;
			if (index < m_size)
				Array.Copy(m_items, index + count, m_items, index, m_size - index);

			m_version++;
			if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
				Array.Clear(m_items, m_size, count);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Clear()
	{
		m_version++;
		if (m_size > 0 && RuntimeHelpers.IsReferenceOrContainsReferences<T>()) {
			Array.Clear(m_items, 0, m_size);
		}
		m_size = 0;
	}

	public bool Contains(T item)
		=> m_size != 0 && IndexOf(item) >= 0;

	public void CopyTo(T[] dest)
		=> Array.Copy(m_items, 0, dest, 0, m_size);

	public int IndexOf(T item)
	{
		var min = 0;
		var max = m_size - 1;

		while (min <= max) {
			var m = min + ((max - min) >> 1);
			var check = m_items[m];
			var result = m_comparer.Compare(check, item);

			if (result < 0) {
				min = m + 1;
			} else if (result > 0) {
				max = m - 1;
			} else {
				return m;
			}
		}

		return -1;
	}

	public ReadOnlySpan<T> AsReadOnlySpan()
		=> new(m_items, 0, m_size);

	public IEnumerator<T> GetEnumerator()
		=> new Enumerator(this);

	IEnumerator IEnumerable.GetEnumerator()
			=> GetEnumerator();

	void Grow(int capacity)
	{
		if (capacity > MaxLength)
			throw new ArgumentOutOfRangeException(nameof(capacity), "capacity too high");

		var newCap = m_items.Length == 0 ? DefaultCapacity : m_items.Length * 2;
		if ((uint)newCap > MaxLength)
			newCap = MaxLength;
		if (newCap < capacity)
			newCap = capacity;

		if (newCap > 0) {
			T[] newArr = new T[newCap];
			if (m_size > 0) {
				Array.Copy(m_items, newArr, m_size);
			}
			m_items = newArr;
		} else {
			m_items = Empty;
		}
	}

	// =============================================================================
	public struct Enumerator : IEnumerator<T>, IEnumerator
	{
		readonly SortedList<T> m_list;
		int m_index;
		T? m_current;
		int m_version;

		internal Enumerator(SortedList<T> list)
		{
			m_list = list;
			m_version = list.m_version;
		}

		public T Current => m_current!;
		object IEnumerator.Current => m_current!;

		public bool MoveNext()
		{
			var list = m_list;

			if (m_version != m_list.m_version)
				throw new InvalidOperationException("the list was modified while enumerating");

			if ((uint)m_index < (uint)list.m_size) {
				m_current = list.m_items[m_index];
				m_index++;
				return true;
			}
			m_current = default;
			m_index = -1;
			return false;
		}

		public void Reset()
		{
			if (m_version != m_list.m_version)
				throw new InvalidOperationException("the list was modified while enumerating");

			m_index = 0;
			m_current = default;
		}

		public void Dispose()
		{
		}
	}
}
