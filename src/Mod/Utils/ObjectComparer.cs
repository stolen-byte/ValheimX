// SPDX-License-Identifier: GPL-3.0-or-later
namespace StolenByte.ValheimX;

using Object = UnityEngine.Object;

// =============================================================================
public sealed class ObjectComparer : IEqualityComparer<Object>
{
	public static ObjectComparer Default = new();

	public bool Equals(Object x, Object y)
		=> GetHashCode(x) == GetHashCode(y);

	public bool Equals(Object x, string y)
		=> GetHashCode(x) == y.GetStableHashCodeBase();

	public bool Equals(Object x, int y)
		=> GetHashCode(x) == y;

	public int GetHashCode(Object obj)
		=> obj.name.GetStableHashCodeBase();
}
