// SPDX-License-Identifier: GPL-3.0-or-later
namespace StolenByte.ValheimX.Generators;

// =============================================================================
static class BuildProps
{
	public const string RootNamespace = nameof(RootNamespace);
	public const string ModName = nameof(ModName);
	public const string ModVersion = nameof(ModVersion);
	public const string ModAuthor = nameof(ModAuthor);
	public const string ModGUID = nameof(ModGUID);
}

static class TypeNames
{
	public static class Internal
	{
		public const string Namespace = "StolenByte.ValheimX";

		public const string ModAttribute = $"{Namespace}.ModAttribute";
		public const string BindConfigAttribute = $"{Namespace}.BindConfigAttribute";
		public const string ConfigValueAttribute = $"{Namespace}.ConfigValueAttribute";
		public const string ConfigManagerAttributes = $"{Namespace}.ConfigurationManagerAttributes";
		public const string ItemPatchAttribute = $"{Namespace}.ItemPatchAttribute";
	}

	public static class Harmony
	{
		public const string Namespace = "HarmonyLib";

		public const string PatchAttribute = $"{Namespace}.HarmonyPatch";
	}

		public static class BepIn
		{
			public const string Namespace = "BepInEx";
			public const string ConfigNamespace = $"{Namespace}.Configuration";

			public const string ConfigEntry = $"{ConfigNamespace}.ConfigEntry`1";
			public const string ConfigFile = $"{ConfigNamespace}.ConfigFile";
			public const string ConfigDescription = $"{ConfigNamespace}.ConfigDescription";
			public const string AcceptableValueRange = $"{ConfigNamespace}.AcceptableValueRange";
		}
}
