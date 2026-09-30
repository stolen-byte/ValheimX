// SPDX-License-Identifier: GPL-3.0-or-later
namespace StolenByte.ValheimX.Configure;

// =============================================================================
static class FileInfoExtensions
{
	public static string RelativePath(this FileInfo fi, DirectoryInfo root)
		=> Path.GetRelativePath(root.FullName, fi.FullName);
}
