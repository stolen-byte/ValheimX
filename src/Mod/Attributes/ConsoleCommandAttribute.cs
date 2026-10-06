// SPDX-License-Identifier: GPL-3.0-or-later
namespace StolenByte.ValheimX;

// =============================================================================
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class ConsoleCommandAttribute : Attribute
{
	public readonly string? Name;
	public readonly string? Description;

	public bool IsCheat = false;
	public bool IsNetwork = false;
	public bool IsSecret = false;
	public bool IsDev = false;
	public bool IsRemote = false;

	public bool ServerOnly = false;
	public bool AdminOnly = false;

	public string Execute = nameof(Execute);
	public string? TabComplete = null;

	public ConsoleCommandAttribute()
		: this(null, null)
	{ }

	public ConsoleCommandAttribute(string description)
		: this(null, description)
	{
	}

	public ConsoleCommandAttribute(string? name, string? description)
	{
		Name = name;
		Description = description;
	}
}
