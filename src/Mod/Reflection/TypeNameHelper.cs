// SPDX-License-Identifier: GPL-3.0-or-later
namespace StolenByte.ValheimX;

// =============================================================================
static class TypeNameHelper
{
	private static readonly Dictionary<Type, string> m_builtIn = new() {
		{TypeCache.Void, "void"},
		{TypeCache.Bool, "bool"},
		{TypeCache.Byte, "byte"},
		{TypeCache.SByte, "sbyte"},
		{TypeCache.Char, "char"},
		{TypeCache.Int, "int"},
		{TypeCache.UInt, "uint"},
		{TypeCache.Short, "short"},
		{TypeCache.UShort, "ushort"},
		{TypeCache.Long, "long"},
		{TypeCache.ULong, "ulong"},
		{TypeCache.Float, "float"},
		{TypeCache.Double, "double"},
		{TypeCache.Decimal, "decimal"},
		{TypeCache.Object, "object"},
		{TypeCache.String, "string"},
	};

	public static string GetDisplayName(Type type)
	{
		var builder = new StringBuilder(type.Name.Length);
		ResolveType(builder, type);
		return builder.ToString();
	}

	private static void ResolveType(StringBuilder builder, Type type)
	{
		if (type.IsGenericType) {
			if (Nullable.GetUnderlyingType(type) is { } nType) {
				ResolveType(builder, nType);
				builder.Append('?');
			} else {
				ResolveGeneric(builder, type, type.GetGenericArguments());
			}
		} else if (type.IsArray) {
			ResolveArray(builder, type);
		} else if (m_builtIn.TryGetValue(type, out var name)) {
			builder.Append(name);
		} else {
			if (type.IsNested && !type.IsGenericTypeParameter) {
				ResolveType(builder, type.DeclaringType!);
				builder.Append('+');
			}
			builder.Append(type.Name);
		}
	}

	private static void ResolveGeneric(StringBuilder builder, Type type, ReadOnlySpan<Type> genericTypes)
	{
		var offset = 0;

		if (type.IsNested) {
			offset = type.DeclaringType!.GetGenericArguments().Length;
			ResolveGeneric(builder, type.DeclaringType, genericTypes.Slice(0, offset));
			builder.Append('+');
		}

		var partIdx = type.Name.IndexOf('`');
		if (partIdx <= 0) {
			builder.Append(type.Name);
			return;
		}

		builder.Append(type.Name, 0, partIdx);

		builder.Append('<');
		for (var i = offset; i < genericTypes.Length; ++i) {
			ResolveType(builder, genericTypes[i]);
			if (i < genericTypes.Length - 1) {
				builder.Append(',');
				if (!genericTypes[i + 1].IsGenericParameter)
					builder.Append(' ');
			}
		}
		builder.Append('>');
	}

	private static void ResolveArray(StringBuilder builder, Type type)
	{
		var innerType = type;
		while (innerType.IsArray)
			innerType = innerType.GetElementType()!;

		ResolveType(builder, innerType);

		while (type.IsArray) {
			builder
				.Append('[')
				.Append(',', type.GetArrayRank() - 1)
				.Append(']');

			type = type.GetElementType()!;
		}
	}
}
