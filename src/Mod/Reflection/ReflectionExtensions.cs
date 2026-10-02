// SPDX-License-Identifier: GPL-3.0-or-later
namespace StolenByte.ValheimX;

// =============================================================================
public static class ReflectionExtensions
{
	public static bool IsAssignableTo(this Type @from, Type to)
		=> to.IsAssignableFrom(@from);

	public static bool IsAssignableTo<To>(this Type @from)
		=> IsAssignableTo(@from, typeof(To));

	public static bool IsConcrete(this Type type)
		=> !type.IsInterface && (!type.IsAbstract || type.IsSealed);

	public static bool IsStatic(this Type type)
		=> type.IsAbstract && type.IsSealed;

	/// <summary>
	/// Returns the type name, with generic parameters resolved.
	/// </summary>
	public static string DisplayName(this Type type)
		=> TypeNameHelper.GetDisplayName(type);
}
