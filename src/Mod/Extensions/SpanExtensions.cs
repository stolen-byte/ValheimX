// SPDX-License-Identifier: GPL-3.0-or-later
namespace StolenByte.ValheimX;

using InlineIL;
using static InlineIL.IL.Emit;

// =============================================================================
// Because unity doesn't have access to `CollectionsMarshal`
public static class ListExtensions
{
	public static Span<T> AsSpan<T>(this List<T> list)
	{
		// return new Span<T>(list._items, 0, list._size)
		Ldarg_0();
		Ldfld(new FieldRef(typeof(List<T>), "_items"));
		Ldc_I4_0();
		Ldarg_0();
		Ldfld(new FieldRef(typeof(List<T>), "_size"));
		Newobj(new MethodRef(typeof(Span<T>), ".ctor", typeof(T[]), typeof(int), typeof(int)));
		Ret();

		throw IL.Unreachable();
	}

	public static ReadOnlySpan<T> AsReadOnlySpan<T>(this List<T> list)
	{
		// return new ReadOnlySpan<T>(list._items, 0, list._size);
		Ldarg_0();
		Ldfld(new FieldRef(typeof(List<T>), "_items"));
		Ldc_I4_0();
		Ldarg_0();
		Ldfld(new FieldRef(typeof(List<T>), "_size"));
		Newobj(new MethodRef(typeof(ReadOnlySpan<T>), ".ctor", typeof(T[]), typeof(int), typeof(int)));
		Ret();

		throw IL.Unreachable();
	}
}
