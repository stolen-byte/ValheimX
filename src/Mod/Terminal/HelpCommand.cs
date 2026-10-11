// SPDX-License-Identifier: GPL-3.0-or-later
namespace StolenByte.ValheimX;

// =============================================================================
[ConsoleCommand("help", "Show list of available console commands matching name, or all.")]
static partial class HelpCommand
{
	static void Execute(Terminal ctx, string? name = null)
	{
		var commands = new SortedList<string>();
		foreach (var cmd in Terminal.commands) {
			if (!cmd.Value.ShowCommand(ctx))
				continue;

			if (name is not null && !cmd.Key.Contains(name, StringComparison.OrdinalIgnoreCase))
				continue;

			commands.Add($"{cmd.Key} - {cmd.Value.Description}");
		}

		foreach (var line in commands) {
			ctx.AddString(line);
		}
	}
}
