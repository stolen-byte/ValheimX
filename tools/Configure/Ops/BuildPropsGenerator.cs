// SPDX-License-Identifier: GPL-3.0-or-later
namespace StolenByte.ValheimX.Configure;

using System.Xml;
using System.Xml.Linq;
using Serilog;

// =============================================================================
class BuildPropsGenerator : IGeneratorTask
{
	public async Task Run(Project mod, DirectoryInfo gameDir, CancellationToken ct)
	{
		var outFile = new FileInfo(mod.GetPropertyValueOrThrow(PropNames.BuildPropsFile));
		var props = new BuildProps(gameDir);

		ct.ThrowIfCancellationRequested();
		await props.Serialize(outFile, ct);

		Log.Information("Generated build props {Path}", outFile.RelativePath(mod.RootDir));
	}

	// ===========================================================================
	struct BuildProps(DirectoryInfo GameDir)
	{
		public async Task Serialize(FileInfo target, CancellationToken ct)
		{
			var settings = new XmlWriterSettings {
				Encoding = new UTF8Encoding(false),
				Indent = true,
				OmitXmlDeclaration = false,
				NewLineChars = "\n",
				Async = true,
			};

			ct.ThrowIfCancellationRequested();
			var xml = new XDocument(
				new XDeclaration("1.0", "utf-8", null),
				new XElement("Project",
					new XElement("PropertyGroup",
						new XElement("GameDir", new XText(GameDir.FullName))
					)
				)
			);

			ct.ThrowIfCancellationRequested();
			using (var writer = XmlWriter.Create(target.FullName, settings)) {
				await xml.SaveAsync(writer, ct);
			}
		}
	}
}
