// SPDX-License-Identifier: GPL-3.0-or-later
namespace StolenByte.ValheimX.Generators;

using RoslynDiagnostic = Microsoft.CodeAnalysis.Diagnostic;

// =============================================================================
sealed record Diagnostic
{
	public readonly DiagnosticDescriptor Descriptor;
	public readonly SourceLocation? Location;
	public readonly object?[]? FormatArgs;

	public Diagnostic(DiagnosticDescriptor descriptor, SourceLocation? location, params object?[] args)
	{
		Descriptor = descriptor;
		Location = location;
		FormatArgs = args;
	}

	public RoslynDiagnostic ToRoslynDiagnostic()
	{
		return RoslynDiagnostic.Create(
			Descriptor,
			Location?.ToLocation(),
			FormatArgs
		);
	}
}
