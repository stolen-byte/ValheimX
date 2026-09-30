// SPDX-License-Identifier: GPL-3.0-or-later
namespace StolenByte.ValheimX.Generators;

// =============================================================================
sealed record SourceLocation(string FilePath, TextSpan Source, LinePositionSpan Line)
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Location ToLocation()
		=> Location.Create(FilePath, Source, Line);

	public static SourceLocation? From(Location? loc)
	{
		if (loc?.SourceTree is null)
			return null;
		return new(loc.SourceTree.FilePath, loc.SourceSpan, loc.GetLineSpan().Span);
	}
}

// =============================================================================
static class SourceLocationExtensions
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static SourceLocation? SourceLocation(this ISymbol symbol)
		=> StolenByte.ValheimX.Generators.SourceLocation.From(symbol.Locations.FirstOrDefault());

	public static SourceLocation? SourceLocation(this AttributeData attribute)
	{
		Location? loc = null;
		if (attribute.ApplicationSyntaxReference is { } sRef)
			loc = sRef.SyntaxTree.GetLocation(sRef.Span);
		return StolenByte.ValheimX.Generators.SourceLocation.From(loc);
	}

	public static SourceLocation? SourceLocation(this SyntaxNode node)
		=> StolenByte.ValheimX.Generators.SourceLocation.From(node.GetLocation());
}
