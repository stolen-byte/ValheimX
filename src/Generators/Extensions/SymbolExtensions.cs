// SPDX-License-Identifier: GPL-3.0-or-later
namespace StolenByte.ValheimX.Generators;

// =============================================================================
static class SymbolExtensions
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static string? DisplayName(this INamespaceSymbol sym)
		=> !sym.IsGlobalNamespace ? sym.ToDisplayString() : null;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static INamedTypeSymbol GetTypeByName(this SemanticModel sm, string fqn)
		=> sm.Compilation.GetTypeByMetadataName(fqn) ?? throw new InvalidOperationException($"Unable to find type {fqn}");
}
