// SPDX-License-Identifier: GPL-3.0-or-later
namespace StolenByte.ValheimX;

// =============================================================================
public static class StringParser
{
	public const NumberStyles DecimalStyle = NumberStyles.Integer | NumberStyles.AllowDecimalPoint;
	public static readonly IFormatProvider InvariantFormat = NumberFormatInfo.InvariantInfo;

	public static T? Parse<T>(string s)
	{
		throw new Exception($"unsupported type {typeof(T)}");
	}

	public static bool? ParseBool(string s)
	{
		if (s == "0")
			return false;
		if (s == "1")
			return true;
		return bool.TryParse(s, out var result) ? result : null;
	}

	public static int? ParseInt(string s)
		=> int.TryParse(s, out var result) ? result : null;

	public static long? ParseLong(string s)
		=> long.TryParse(s, out var result) ? result : null;

	public static float? ParseFloat(string s)
		=> float.TryParse(s, DecimalStyle, InvariantFormat, out var result) ? result : null;

	public static double? ParseDouble(string s)
		=> double.TryParse(s, DecimalStyle, InvariantFormat, out var result) ? result : null;
}
