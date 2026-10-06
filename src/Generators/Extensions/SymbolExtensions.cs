// SPDX-License-Identifier: GPL-3.0-or-later
namespace StolenByte.ValheimX.Generators;

// =============================================================================
static class SymbolExtensions
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static string? DisplayName(this INamespaceSymbol sym)
		=> !sym.IsGlobalNamespace ? sym.ToDisplayString() : null;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ITypeSymbol? GetMemberType(this ISymbol symbol)
	{
		return symbol switch {
			IPropertySymbol prop => prop.Type,
			IFieldSymbol field => field.Type,
			_ => null,
		};
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsReadOnly(this ISymbol symbol)
	{
		return symbol switch {
			IPropertySymbol prop => prop.IsReadOnly,
			IFieldSymbol field => field.IsReadOnly,
			_ => false,
		};
	}

	public static bool IsNumeric(this ITypeSymbol type)
	{
		if (type.IsNativeIntegerType)
			return true;

		return type.IsValueType && type.SpecialType switch {
			SpecialType.System_Byte or SpecialType.System_SByte => true,
			SpecialType.System_Int16 or SpecialType.System_Int32 or SpecialType.System_Int64 => true,
			SpecialType.System_UInt16 or SpecialType.System_UInt32 or SpecialType.System_UInt64 => true,
			SpecialType.System_Single or SpecialType.System_Double => true,
			_ => false,
		};
	}

	public static bool IsBuiltinType(this INamedTypeSymbol type)
	{
		if (type.IsGenericType && type.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T) {
			if (type.TypeArguments is { Length: 1 } tArgs) {
				type = (INamedTypeSymbol)tArgs[0];
			} else {
				return false;
			}
		}
		return type.SpecialType != SpecialType.None;
	}

	public static bool IsNullable(this ITypeSymbol type)
		=> type.NullableAnnotation == NullableAnnotation.Annotated;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool MatchesType(this SymbolEqualityComparer comparer, ITypeSymbol sym, Type type, SemanticModel sm)
		=> comparer.Equals(sym, sm.GetSymbolForType(type));

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool MatchesType(this ITypeSymbol sym, Type type, SemanticModel sm)
		=> SymbolEqualityComparer.Default.MatchesType(sym, type, sm);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool HasErrors(this IEnumerable<TypedConstant> items)
		=> items.Any(x => x.Kind == TypedConstantKind.Error);

	public static string ToCode(this TypedConstant value)
	{
		var result = value.ToCSharpString();
		if (value.Type?.SpecialType == SpecialType.System_Single)
			return result + "f";
		return result;
	}
}
