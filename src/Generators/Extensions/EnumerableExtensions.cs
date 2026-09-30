// SPDX-License-Identifier: GPL-3.0-or-later
namespace StolenByte.ValheimX.Generators;

// =============================================================================
static class EnumerableExtensions
{
	public static IEnumerable<T> Intersperse<T>(this IEnumerable<T> source, T element)
	{
		using (var iter = source.GetEnumerator()) {
			if (iter.MoveNext()) {
				yield return iter.Current;
				while (iter.MoveNext()) {
					yield return element;
					yield return iter.Current;
				}
			}
		}
	}

	public static IncrementalValuesProvider<T> NonNull<T>(this IncrementalValuesProvider<T?> e)
		where T : class
		=> e.Where(static (x) => x is not null)!;

	public static IncrementalValuesProvider<T> NonNull<T>(this IncrementalValuesProvider<T?> e)
		where T : struct
		=> e.Where(static (x) => x.HasValue).Select(static (x, _) => x!.Value);

	public static IEnumerable<T> NonNull<T>(this IEnumerable<T?> e)
		where T : class
		=> e.Where(static (x) => x is not null)!;

	public static bool ReportAll(this EquatableArray<Diagnostic> diagnostics, SourceProductionContext ctx)
	{
		var hasErrors = false;
		foreach (ref readonly var diag in diagnostics.AsSpan()) {
			ctx.ReportDiagnostic(diag.ToRoslynDiagnostic());
			hasErrors = hasErrors || diag.Descriptor.DefaultSeverity == DiagnosticSeverity.Error;
		}
		return hasErrors;
	}
}
