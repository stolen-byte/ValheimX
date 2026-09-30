// SPDX-License-Identifier: GPL-3.0-or-later
namespace StolenByte.ValheimX.Generators;

// =============================================================================
static class ClassRefExtensions
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ClassRef ClassRef(this INamedTypeSymbol symbol)
		=> new(symbol.ContainingNamespace.DisplayName(), symbol.Name, symbol.DeclaredAccessibility);
}

// =============================================================================
readonly record struct ClassRef(string? Namespace, string Name, Accessibility Visibility)
{
	public void FormatTo(StringBuilder sb)
	{
		if (Namespace is not null)
			sb.Append(Namespace).Append('.');
		sb.Append(Name);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void FormatTo(CodeBuilder cb)
		=> FormatTo(cb.StringBuilder);

	public string ToDisplayString()
	{
		if (Namespace is null)
			return Name;

		var sb = new StringBuilder();
		FormatTo(sb);
		return sb.ToString();
	}
}
