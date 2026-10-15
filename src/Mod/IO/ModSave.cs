// SPDX-License-Identifier: GPL-3.0-or-later
namespace StolenByte.ValheimX;

// =============================================================================
static class ModSave
{
	static readonly string m_basePath = Path.Combine(Paths.PluginPath, Mod.GUID);

	public static string BasePath
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => m_basePath;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static IEnumerable<string> GetCharacters()
		=> EnumerateEntries(BasePath, Directory.EnumerateDirectories);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static string GetCharacterDirectory(string name)
		=> Path.Combine(BasePath, Path.SanitizeFileName(name));

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static string GetCharacterDirectory(PlayerProfile pp)
		=> GetCharacterDirectory(pp.m_filename);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static IEnumerable<string> GetCharacterFiles(string name)
		=> EnumerateEntries(GetCharacterDirectory(name), Directory.EnumerateFiles);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static IEnumerable<string> GetCharacterFiles(PlayerProfile pp)
		=> GetCharacterFiles(pp.m_filename);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static string GetCharacterFilePath(PlayerProfile pp, string fileName)
		=> Path.Combine(GetCharacterDirectory(pp), fileName);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static string GetCharacterFilePath(string name, string fileName)
		=> Path.Combine(GetCharacterDirectory(name), fileName);

	static IEnumerable<string> EnumerateEntries(string basePath, Func<string, IEnumerable<string>> enumerator)
	{
		if (Directory.Exists(basePath)) {
			foreach (var entry in enumerator(basePath)) {
				yield return Path.GetFileName(entry);
			}
		}
	}
}
