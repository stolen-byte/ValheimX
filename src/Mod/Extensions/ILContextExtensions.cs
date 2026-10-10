// SPDX-License-Identifier: GPL-3.0-or-later
namespace StolenByte.ValheimX;

// =============================================================================
public static class ILContextExtensions
{
	public static VariableDefinition CreateLocal(this ILContext ctx, TypeReference type)
	{
		var newLocal = new VariableDefinition(type);
		ctx.Method.Body.Variables.Add(newLocal);
		return newLocal;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static VariableDefinition CreateLocal(this ILContext ctx, Type type)
		=> CreateLocal(ctx, ctx.Import(type));

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static VariableDefinition CreateLocal<T>(this ILContext ctx)
		=> CreateLocal(ctx, ctx.Import(typeof(T)));

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ILLabel CreateLabelAt(this ILCursor cursor, int offset)
	{
		cursor.Index += offset;
		var label = cursor.MarkLabel();
		cursor.Index -= offset;
		return label;
	}

	public static ILCursor EmitBrTrue<T>(this ILCursor cursor, ILLabel label, T cb)
		where T : Delegate
		=> cursor.EmitCall(cb).Emit(OpCodes.Brtrue_S, label);

	public static ILCursor EmitBrFalse<T>(this ILCursor cursor, ILLabel label, T cb)
		where T : Delegate
		=> cursor.EmitCall(cb).Emit(OpCodes.Brfalse_S, label);

	// because its annoying that `EmitDelegate` doesn't allow chaining, like every other method
	public static ILCursor EmitCall<T>(this ILCursor cursor, T cb)
		where T : Delegate
	{
		cursor.EmitDelegate(cb);
		return cursor;
	}

	public static ILCursor EmitCall(this ILCursor cursor, Type container, string name)
		=> EmitCallInternal(cursor, Reflection.Method(container, name), name, false);

	public static ILCursor EmitCall(this ILCursor cursor, Type container, string name, params Type[] args)
		=> EmitCallInternal(cursor, Reflection.Method(container, name, args), name, false);

	public static ILCursor EmitCall<T>(this ILCursor cursor, string name)
		=> EmitCall(cursor, typeof(T), name);

	public static ILCursor EmitCall<T>(this ILCursor cursor, string name, params Type[] args)
		=> EmitCall(cursor, typeof(T), name, args);

	public static ILCursor EmitCallVirt(this ILCursor cursor, Type container, string name)
		=> EmitCallInternal(cursor, Reflection.Method(container, name), name, true);

	public static ILCursor EmitCallVirt(this ILCursor cursor, Type container, string name, params Type[] args)
		=> EmitCallInternal(cursor, Reflection.Method(container, name, args), name, true);

	public static ILCursor EmitCallVirt<T>(this ILCursor cursor, string name)
		=> EmitCallVirt(cursor, typeof(T), name);

	public static ILCursor EmitCallVirt<T>(this ILCursor cursor, string name, params Type[] args)
		=> EmitCallVirt(cursor, typeof(T), name, args);

	static ILCursor EmitCallInternal(ILCursor cursor, MethodInfo? mi, string name, bool virtualCall)
	{
		if (mi is null)
			throw new Exception($"Unable to find method '{name}'");
		return cursor.Emit(virtualCall ? OpCodes.Callvirt : OpCodes.Call, mi);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ApplyPatch(this ILContext ctx, string name, Action<ILContext, ILCursor> patch)
	{
		try {
			var cursor = new ILCursor(ctx);
			patch(ctx, cursor);
		} catch (Exception ex) {
			Log.Error($"{name}: {ex.Message}");
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ApplyPatch<T1>(
		this ILContext ctx,
		string name,
		T1 arg1,
		Action<ILContext, ILCursor, T1> patch)
	{
		try {
			var cursor = new ILCursor(ctx);
			patch(ctx, cursor, arg1);
		} catch (Exception ex) {
			Log.Error($"{name}: {ex.Message}");
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ApplyPatch<T1, T2>(
		this ILContext ctx,
		string name,
		T1 arg1,
		T2 arg2,
		Action<ILContext, ILCursor, T1, T2> patch)
	{
		try {
			var cursor = new ILCursor(ctx);
			patch(ctx, cursor, arg1, arg2);
		} catch (Exception ex) {
			Log.Error($"{name}: {ex.Message}");
		}
	}
}
