// SPDX-License-Identifier: GPL-3.0-or-later
namespace StolenByte.ValheimX;

using BepInEx.Logging;

// =============================================================================
static class Log
{
	static ManualLogSource? m_source = null;

	internal static void Init(ManualLogSource source)
	{
		m_source = source;
	}

	internal static void Destroy()
	{
		m_source = null;
	}

	public static void Fatal(object data)
		=> Write(LogLevel.Fatal, data);

	public static void Exception(string message, Exception ex)
		=> Write(LogLevel.Error, $"{message}\n{ex}");

	public static void Error(object data)
		=> Write(LogLevel.Error, data);

	public static void Warn(object data)
		=> Write(LogLevel.Warning, data);

	public static void Info(object data)
		=> Write(LogLevel.Info, data);

	public static void Message(object data)
		=> Write(LogLevel.Message, data);

	public static void Debug(object data)
		=> Write(LogLevel.Debug, data);

	static void Write(LogLevel level, object data)
		=> m_source?.Log(level, data);
}
