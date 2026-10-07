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

	// because its annoying that `EmitDelegate` doesn't allow chaining, like every other method
	public static ILCursor EmitFunc<T>(this ILCursor cursor, T cb)
		where T : Delegate
	{
		cursor.EmitDelegate(cb);
		return cursor;
	}

	public static ILCursor EmitBrTrue<T>(this ILCursor cursor, ILLabel label, T cb)
		where T : Delegate
		=> cursor.EmitFunc(cb).Emit(OpCodes.Brtrue_S, label);

	public static ILCursor EmitBrFalse<T>(this ILCursor cursor, ILLabel label, T cb)
		where T : Delegate
		=> cursor.EmitFunc(cb).Emit(OpCodes.Brfalse_S, label);
}
