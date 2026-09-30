// SPDX-License-Identifier: GPL-3.0-or-later
namespace StolenByte.ValheimX.Tasks;

using Microsoft.Build.Framework;
using Semver;

// =============================================================================
public class GitVersion : BaseTask
{
	public string? WorkingDirectory { get; set; }
	public string? ExportOverride { get; set; }

	[Output]
	public string Version { get; set; } = string.Empty;

	[Output]
	public string VersionFull { get; set; } = string.Empty;

	protected override void OnExecute()
	{
		var (vShort, vFull) = GetVersion();
		Version = vShort.ToString();
		VersionFull = vFull?.ToString() ?? Version;
	}

	(SemVersion, SemVersion?) GetVersion()
	{
		SemVersion vFull;
		GitDescribeExtra? extra = null;

		if (ExportOverride is not null && !ExportOverride.IsEmpty() && !ExportOverride.StartsWith("$")) {
			(vFull, extra) = FromExport(ExportOverride);
		}
		else {
			try {
				(vFull, extra) = FromGit(new(WorkingDirectory));
			}
			catch (GitException) {
				return (Unversioned(), null);
			}
		}

		var vShort = vFull.WithoutPrereleaseOrMetadata();
		if (extra is { } info) {
			var pre = vFull.PrereleaseIdentifiers.ToList();
			// `info.Distance` is guaranteed to be > 0
			pre.Add(new PrereleaseIdentifier(info.Distance));

			var meta = vFull.MetadataIdentifiers.ToList();
			meta.Add(new MetadataIdentifier(info.Commit));

			vFull = vShort.WithPrerelease(pre).WithMetadata(meta);
		}

		return (vShort, vFull);
	}

	static (SemVersion, GitDescribeExtra?) FromExport(string data)
	{
		var export = GitDescribeResult.Parse(data);
		return (ParseVersion(export.Tag), export.Extra);
	}

	static (SemVersion, GitDescribeExtra?) FromGit(Git instance)
	{
		try {
			var desc = instance.Describe();
			var version = ParseVersion(desc.Tag);
			return (version, desc.Extra);
		}
		catch (GitException) {
			// no tags
			GitDescribeExtra? info = null;

			if (instance.CommitId() is { } id) {
				var dist = instance.DistanceFrom(null);
				if (dist > 0) {
					info = new(dist, id);
				}
			}

			return (Unversioned(), info);
		}
	}

	static SemVersion ParseVersion(string tag)
		=> SemVersion.Parse(tag.TrimStart('v'), SemVersionStyles.Strict);

	static SemVersion Unversioned()
		=> new(0, 1, 0);
}
