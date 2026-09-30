// SPDX-License-Identifier: GPL-3.0-or-later
namespace StolenByte.ValheimX.Configure;

using BepInEx.AssemblyPublicizer;
using Serilog;

// =============================================================================
class ReferenceGenerator : IGeneratorTask
{
	private readonly AssemblyPublicizerOptions m_options;

	public ReferenceGenerator()
	{
		m_options = new AssemblyPublicizerOptions {
			Target = PublicizeTarget.All,
			PublicizeCompilerGenerated = false,
			IncludeOriginalAttributesAttribute = false,
			Strip = true,
		};
	}

	public async Task Run(Project mod, DirectoryInfo gameDir, CancellationToken ct)
	{
		ct.ThrowIfCancellationRequested();
		var outDir = new DirectoryInfo(mod.GetPropertyValueOrThrow(PropNames.RefAssembliesDir));
		var sourceRelPath = mod.GetPropertyValueOrThrow(PropNames.GameAssembliesRelDir);
		var done = 0;

		ct.ThrowIfCancellationRequested();
		Directory.CreateDirectory(outDir.FullName);

		var names = mod.GetItems(PropNames.GameAssemblies).Select(item => item.EvaluatedInclude);
		await Parallel.ForEachAsync(names, ct, async (name, ct) => {
			ct.ThrowIfCancellationRequested();
			var sourceFile = new FileInfo(Path.Combine(gameDir.FullName, sourceRelPath, name));
			var outFile = new FileInfo(Path.Combine(outDir.FullName, name));

			AssemblyPublicizer.Publicize(sourceFile.FullName, outFile.FullName, m_options);

			Log.Information("Generated {Target}", outFile.RelativePath(mod.RootDir));
			Interlocked.Increment(ref done);
		});

		Log.Information("Generated {Count} assemblies", done);
	}
}
