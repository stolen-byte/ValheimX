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
	}

	// =============================================================================
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Diagnostic MultipleModsDefined(SourceLocation? location)
		=> new(Descriptors.MultipleModsDefined, location);
}
