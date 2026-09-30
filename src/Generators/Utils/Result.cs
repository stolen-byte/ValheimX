// SPDX-License-Identifier: GPL-3.0-or-later
namespace StolenByte.ValheimX.Generators;

// =============================================================================
/// <remarks>8 + sizeof(T) bytes</remarks>
readonly record struct Result<T>(T Value, EquatableArray<Diagnostic> Diagnostics)
{
	public readonly T Value = Value;
	public readonly EquatableArray<Diagnostic> Diagnostics = Diagnostics;

	public bool HasDiagnostics => !Diagnostics.IsEmpty;

	public Result(T value, IEnumerable<Diagnostic> diagnostics)
		: this(value, diagnostics.ToEquatableArray())
	{ }

	/// <returns>true if any of the diagnostics are errors</returns>
	public bool ReportDiagnostics(SourceProductionContext ctx)
		=> Diagnostics.ReportAll(ctx);
}

// =============================================================================
internal static class ResultExtensions
{
	public static ref readonly T ValueRef<T>(this in Result<T> result)
		=> ref result.Value;
}
