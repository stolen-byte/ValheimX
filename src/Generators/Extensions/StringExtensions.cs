// SPDX-License-Identifier: GPL-3.0-or-later
namespace StolenByte.ValheimX.Generators;

// =============================================================================
static class StringExtensions
{
	public static string Quoted(this string s)
		=> $"\"{s}\"";

	public static string Unquote(this string s)
	{
		if (s.Length >= 2) {
			int i = s[0] == '"' ? 1 : 0;
			int j = s.EndsWith("\"") ? s.Length - 1 : s.Length;
			s = s[i..j];
		}
		return s;
	}

	public static string AsString(this bool b)
		=> b ? "true" : "false";

	public static bool IsEmpty(this string s)
		=> s.AsSpan().Trim().IsEmpty;

	public static bool IsEmpty(this StringBuilder builder)
		=> builder.Length == 0;

	public static bool EndsWith(this StringBuilder builder, char ch)
		=> builder.Length > 0 && builder[builder.Length - 1] == ch;

	public static bool EndsWith(this StringBuilder builder, params char[] chars)
	{
		if (builder.Length > 0) {
			var lastChar = builder[builder.Length - 1];
			foreach (var ch in chars) {
				if (ch == lastChar)
					return true;
			}
		}
		return false;
	}

	public static StringBuilder TrimEnd(this StringBuilder builder, char ch)
	{
		for (var i = builder.Length - 1; i >= 0 && builder[i] == ch; --i)
			builder.Remove(i, 1);
		return builder;
	}
}
