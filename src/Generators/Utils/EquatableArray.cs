// SPDX-License-Identifier: GPL-3.0-or-later
namespace StolenByte.ValheimX.Generators;

// =============================================================================
internal static class EquatableArray
{
	public static EquatableArray<T> Empty<T>()
		where T : IEquatable<T>
		=> new(null);

	public static EquatableArray<T> ToEquatableArray<T>(this T[] arr)
		where T : IEquatable<T>
		=> new(arr);

	public static EquatableArray<T> ToEquatableArray<T>(this IEnumerable<T> e)
		where T : IEquatable<T>
	{
		if (e is EquatableArray<T> same)
			return same;
		return new(e.ToArray());
	}

	public static EquatableArray<T> AsEquatableArray<T>(this ImmutableArray<T> arr)
		where T : IEquatable<T>
		=> new(arr);
}

internal readonly struct EquatableArray<T> : IEquatable<EquatableArray<T>>, IEnumerable<T>
	where T : IEquatable<T>
{
	private readonly T[]? m_inner;

	/// <remarks>
	/// For performance reasons, the <paramref name="inner"/> array is not cloned,
	/// it is expected that ownership is transferred here.
	/// </remarks>
	public EquatableArray(T[]? inner)
	{
		m_inner = inner;
	}

	public EquatableArray(ImmutableArray<T> inner)
	{
		m_inner = Unsafe.As<ImmutableArray<T>, T[]?>(ref inner);
	}

	public ref readonly T this[int index]
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref AsImmutableArray().ItemRef(index);
	}

	public bool IsEmpty
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => AsImmutableArray().IsEmpty;
	}

	public int Length
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => AsImmutableArray().Length;
	}

	public bool Equals(EquatableArray<T> other)
		=> AsSpan().SequenceEqual(other.AsSpan());

	public override bool Equals([NotNullWhen(true)] object? obj)
		=> obj is EquatableArray<T> arr && Equals(arr);

	public override int GetHashCode()
	{
		if (m_inner is not T[] arr)
			return 0;

		var hash = default(HashCode);
		foreach (ref readonly var item in AsSpan()) {
			hash.Add(item);
		}

		return hash.ToHashCode();
	}

	public ReadOnlySpan<T> AsSpan()
		=> AsImmutableArray().AsSpan();

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ImmutableArray<T> AsImmutableArray()
		=> Unsafe.As<T[]?, ImmutableArray<T>>(ref Unsafe.AsRef(in this.m_inner));

	public static EquatableArray<T> FromImmutableArray(ImmutableArray<T> arr)
		=> new(arr);

	public ImmutableArray<T>.Enumerator GetEnumerator()
		=> AsImmutableArray().GetEnumerator();

	IEnumerator<T> IEnumerable<T>.GetEnumerator()
		=> ((IEnumerable<T>)AsImmutableArray()).GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator()
		=> ((IEnumerable<T>)AsImmutableArray()).GetEnumerator();

	public static implicit operator EquatableArray<T>(ImmutableArray<T> arr)
		=> FromImmutableArray(arr);

	public static implicit operator ImmutableArray<T>(EquatableArray<T> arr)
		=> arr.AsImmutableArray();

	public static bool operator ==(EquatableArray<T> left, EquatableArray<T> right)
		=> left.Equals(right);

	public static bool operator !=(EquatableArray<T> left, EquatableArray<T> right)
		=> !left.Equals(right);
}
