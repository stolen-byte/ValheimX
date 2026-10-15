// SPDX-License-Identifier: GPL-3.0-or-later
namespace StolenByte.ValheimX;

// =============================================================================
public static class IOExtensions
{
	static readonly HashSet<char> InvalidFileNameChars = new(Path.GetInvalidFileNameChars());

	extension(File)
	{
		public static bool TryDelete(string path)
		{
			try {
				File.Delete(path);
				return true;
			} catch (Exception) {
				return false;
			}
		}

		public static void Replace(string sourcePath, string destPath)
		{
			if (File.Exists(destPath)) {
				File.Replace(sourcePath, destPath, null);
			} else {
				File.Move(sourcePath, destPath);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static BinaryReader OpenBinaryRead(string path)
			=> new BinaryReader(File.OpenRead(path));

		public static bool WriteWith(string path, Action<BinaryWriter> writer)
		{
			string? tmpPath = null;

			try {
				var fullPath = Path.GetFullPath(path);
				tmpPath = string.Concat(fullPath, ".", Guid.NewGuid().ToString("N"), ".tmp");

				Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

				using (var fs = new FileStream(tmpPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
				using (var stream = new BinaryWriter(fs)) {
					writer(stream);
				}

				File.Replace(tmpPath, fullPath);

				tmpPath = null;
				return true;
			} catch (Exception ex) {
				Log.Exception($"Exception while writing to file '{path}'", ex);
				return false;
			} finally {
				if (tmpPath is not null) {
					File.TryDelete(tmpPath);
				}
			}
		}
	}

	extension(Directory)
	{
		public static bool TryDelete(string path, bool recursive = true)
		{
			try {
				Directory.Delete(path, recursive);
				return true;
			} catch (Exception) {
				return false;
			}
		}
	}

	extension(Path)
	{
		public static string SanitizeFileName(string fileName)
		{
			// pre-scan so we don't allocate a new string unless absolutely needed
			var hasInvalid = false;
			foreach (var ch in fileName) {
				if (InvalidFileNameChars.Contains(ch)) {
					hasInvalid = true;
					break;
				}
			}

			if (!hasInvalid)
				return fileName;

			return string.Create(fileName.Length, fileName, static (dest, src) => {
				for (var i = 0; i < src.Length; ++i) {
					var ch = src[i];
					dest[i] = InvalidFileNameChars.Contains(ch) ? '_' : ch;
				}
			});
		}

	}
}
