// SPDX-License-Identifier: GPL-3.0-or-later
namespace StolenByte.ValheimX;

// =============================================================================
[ConsoleCommand("help", "Show list of available console commands matching name, or all.")]
static partial class HelpCommand
{
	static void Execute(Terminal ctx, string? name = null)
	{
		var commands = new List<string>();
		foreach (var cmd in Terminal.commands) {
			if (!cmd.Value.ShowCommand(ctx))
				continue;

			if (name is not null && !cmd.Key.Contains(name, StringComparison.OrdinalIgnoreCase))
				continue;

			commands.Add($"{cmd.Key} - {cmd.Value.Description}");
		}

		commands.Sort();
		foreach (var line in commands) {
			ctx.AddString(line);
		}
	}
}
