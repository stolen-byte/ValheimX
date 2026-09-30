// SPDX-License-Identifier: GPL-3.0-or-later
namespace StolenByte.ValheimX;

// =============================================================================
[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public sealed class BindConfigAttribute : Attribute
{
	public readonly string? Section;

	public BindConfigAttribute(string? section = null)
	{
		Section = section;
	}
}

[AttributeUsage(
	AttributeTargets.Field | AttributeTargets.Property,
	Inherited = false,
	AllowMultiple = false
)]
public class ConfigValueAttribute : Attribute
{
	public readonly string Key;
	public readonly string Description;

	public string? Section { get; set; }
	public object? Default { get; set; }
	public object? Min { get; set; }
	public object? Max { get; set; }

	/// <inheritdoc cref="ConfigurationManagerAttributes.IsAdvanced" />
	public bool Advanced { get; set; }

	/// <inheritdoc cref="ConfigurationManagerAttributes.ShowRangeAsPercent" />
	public bool Percentage { get; set; }

	/// <summary>
	/// Set the display order of this config entry, lower number = higher position.
	/// </summary>
	public int Order { get; set; }

	/// <summary>
	/// Set to the name of a static method matching the signature
	/// <c>void OnChanged(object sender, EventArgs args)</c>
	/// to automatically wire up its <see cref="ConfigEntry{T}.SettingChanged" /> event.
	/// </summary>
	/// <remarks>
	/// the <c>sender</c> parameter will be the <see cref="ConfigEntry{T}"/> instance, and
	/// the <c>args</c> parameter will be an instance of <see cref="SettingChangedEventArgs"/>
	/// </remarks>
	public string? OnChange = null;

	public ConfigValueAttribute(string key, string description)
	{
		Key = key;
		Description = description;
	}
}
