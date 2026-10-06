// SPDX-License-Identifier: GPL-3.0-or-later
namespace StolenByte.ValheimX;

// =============================================================================
class MissingArgumentException : Exception
{
	public MissingArgumentException(string name)
		: base($"missing required argument '{name}'")
	{ }
}

class InvalidArgumentValueException : Exception
{
	public InvalidArgumentValueException(string name, string value)
		: base($"invalid value '{value}' for argument '{name}'")
	{ }
}

public static class ConsoleEventArgsExtensions
{
	public static string GetRequired(this Terminal.ConsoleEventArgs args, int index, string name)
		=> Get(args, index, name) ?? throw new MissingArgumentException(name);

	public static T GetRequired<T>(this Terminal.ConsoleEventArgs args, int index, string name, Func<string, T?> parser)
		where T : struct
	{
		var value = GetRequired(args, index, name);
		if (parser(value) is not { } result)
			throw new InvalidArgumentValueException(name, value);
		return result;
	}

	public static string? Get(this Terminal.ConsoleEventArgs args, int index, string name)
	{
		++index;
		if (index >= args.Length)
			return null;
		return args[index];
	}

	public static T? Get<T>(
		this Terminal.ConsoleEventArgs args,
		int index,
		string name,
		Func<string, T?> parser)
		where T : struct
	{
		if (Get(args, index, name) is not string value)
			return null;
		if (parser(value) is not T result)
			throw new InvalidArgumentValueException(name, value);
		return result;
	}
}
