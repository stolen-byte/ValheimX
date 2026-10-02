// SPDX-License-Identifier: GPL-3.0-or-later
namespace StolenByte.ValheimX;

// =============================================================================
[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public sealed class ItemPatchAttribute : Attribute
{
	public readonly string? Section;
	public readonly string Key;
	public readonly string Description;

	public ItemPatchAttribute(string key, string description)
	{
		Key = key;
		Description = description;
	}

	public ItemPatchAttribute(string section, string key, string description)
		: this(key, description)
	{
		Section = section;
	}
}
