// SPDX-License-Identifier: GPL-3.0-or-later
namespace StolenByte.ValheimX.Generators;

// =============================================================================
static class Diagnostics
{
	public static EquatableArray<Diagnostic> Empty = EquatableArray.Empty<Diagnostic>();

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static EquatableArray<Diagnostic> From(params Diagnostic[] diagnostics)
		=> new(diagnostics);

	// =============================================================================
	static class Descriptors
	{
		const string Category = "ValheimX";
		const string ID = "VXLIB";

		public static readonly DiagnosticDescriptor MultipleModsDefined = new(
			$"{ID}001",
			$"Multiple types marked with {TypeNames.Internal.ModAttribute}",
			"Only a single type may be marked as a Mod",
			Category,
			DiagnosticSeverity.Error,
			isEnabledByDefault: true
		);

		public static readonly DiagnosticDescriptor TypeNotStatic = new(
			$"{ID}002",
			"Type must be static",
			"Type '{0}' must be declared static",
			Category,
			DiagnosticSeverity.Error,
			isEnabledByDefault: true
		);

		public static readonly DiagnosticDescriptor ReadOnlyConfigEntry = new(
			$"{ID}004",
			"Config value must be writable",
			"Config value '{0}' must be writable",
			Category,
			DiagnosticSeverity.Error,
			isEnabledByDefault: true
		);

		public static readonly DiagnosticDescriptor InvalidParameterType = new(
			$"{ID}005",
			"Config parameter type mismatch",
			"{0} value of '{1}' should be of type {2}",
			Category,
			DiagnosticSeverity.Error,
			isEnabledByDefault: true
		);

		public static readonly DiagnosticDescriptor InvalidRangeType = new(
			$"{ID}006",
			"Config value cannot have Min/Max parameters",
			"Config value '{0}' of type {1} cannot have min/max parameters",
			Category,
			DiagnosticSeverity.Error,
			isEnabledByDefault: true
		);

		public static readonly DiagnosticDescriptor MissingConfigValueAttribute = new(
			$"{ID}007",
			$"Config entry should have a [ConfigValue] attribute",
			"Config entry '{0}' is missing a [ConfigValue] attribute, and will have a default key & empty description",
			Category,
			DiagnosticSeverity.Warning,
			isEnabledByDefault: true
		);
	}

	// =============================================================================
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Diagnostic MultipleModsDefined(SourceLocation? location)
		=> new(Descriptors.MultipleModsDefined, location);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Diagnostic TypeNotStatic(ISymbol type)
		=> new(Descriptors.TypeNotStatic, type.SourceLocation(), type.Name);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Diagnostic ReadOnlyConfigValue(ISymbol member)
		=> new(Descriptors.ReadOnlyConfigEntry, member.SourceLocation(), member.Name);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Diagnostic InvalidParameterType(string name, ISymbol sym, ITypeSymbol type)
		=> new(Descriptors.InvalidParameterType, sym.SourceLocation(), name, sym.Name, type.ToDisplayString());

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Diagnostic InvalidRangeType(ISymbol member, ITypeSymbol type)
		=> new(Descriptors.InvalidRangeType, member.SourceLocation(), member.Name, type.ToDisplayString());

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Diagnostic MissingConfigValueAttribute(ISymbol member)
		=> new(Descriptors.MissingConfigValueAttribute, member.SourceLocation(), member.Name);
}
