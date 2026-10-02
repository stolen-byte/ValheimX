// SPDX-License-Identifier: GPL-3.0-or-later
namespace StolenByte.ValheimX;

// =============================================================================
public static class ImmutableArray
{
	public static ImmutableArray<T> Create<T>(params ReadOnlySpan<T> items)
	{
		if (items.IsEmpty)
			return ImmutableArray<T>.Empty;
		return new(items.ToArray());
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ImmutableArray<T> Create<T>(IEnumerable<T> items)
		=> new(items.ToArray());

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ImmutableArray<T> ToImmutableArray<T>(this ReadOnlySpan<T> items)
		=> Create(items);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ImmutableArray<T> ToImmutableArray<T>(this IEnumerable<T> items)
		=> Create(items);
}

// =============================================================================
public readonly struct ImmutableArray<T>
{
	private readonly T[] m_inner;

	public static readonly ImmutableArray<T> Empty = new(new T[0]);

	public ImmutableArray(T[] inner)
	{
		m_inner = inner;
	}

	public int Length => m_inner.Length;
	public bool IsEmpty => Length == 0;

	public ref readonly T this[int index]
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref m_inner[index];
	}

	public ReadOnlySpan<T> AsSpan()
		=> new(m_inner);

	public ReadOnlySpan<T> AsSpan(int start, int length)
		=> new(m_inner, start, length);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Enumerator GetEnumerator()
		=> new(m_inner);

	// =============================================================================
	public ref struct Enumerator
	{
		private readonly T[] m_array;
		private int m_index;

		internal Enumerator(T[] array)
		{
			m_array = array;
			m_index = -1;
		}

		public ref readonly T Current
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => ref m_array[m_index];
		}

		public bool MoveNext()
		{
			return ++m_index < m_array.Length;
		}
	}
}
