// SPDX-License-Identifier: GPL-3.0-or-later
namespace StolenByte.ValheimX.Configure;

using System.CommandLine;
using System.Runtime.CompilerServices;
using Microsoft.Build.Locator;
using Serilog;

// =============================================================================
class ProgramException : Exception
{
	public string Template { get; }
	public object?[]? Args { get; }

	public ProgramException(string template, params object?[]? args)
		: base()
	{
		Template = template;
		Args = args;
	}
}

class Error : ProgramException
{
	public Error(string template, params object?[]? args)
		: base(template, args)
	{ }
}

// =============================================================================
class Program
{
	static async Task<int> Main(string[] args)
	{
		try {
			Log.Logger = new LoggerConfiguration()
				.MinimumLevel.Information()
				.WriteTo.Console(outputTemplate: "[{Level:u4}] {Message:lj}{NewLine}{Exception}")
				.CreateLogger();

			MSBuildLocator.RegisterDefaults();
			return await RunCli(new ConfigureCommand(), args);
		}
		catch (OperationCanceledException) {
			Log.Warning("Operation Cancelled, Exiting.");
			return 0;
		}
		catch (Error e) {
			// "expected" errors that don't require a stack-trace
			Log.Fatal(e.Template, e.Args);
			return 1;
		}
		catch (ProgramException ex) {
			Log.Fatal(ex, ex.Template, ex.Args);
			return 1;
		}
		catch (Exception ex) {
			Log.Fatal(ex, "Unhandled Exception, bailing!");
			return 1;
		}
		finally {
			Log.CloseAndFlush();
		}
	}

	// Because MSBuildLocator/MSBuild is fubar
	[MethodImpl(MethodImplOptions.NoInlining)]
	static Task<int> RunCli(ConfigureCommand cmd, string[] args)
	{
		var parseConfig = new ParserConfiguration {
			EnablePosixBundling = true,
			ResponseFileTokenReplacer = null,
		};

		return cmd.Parse(args, parseConfig).InvokeAsync(new() {
			// Most stupid option i've ever seen, this doesn't even function as a timeout?
			// Setting this higher than default (2s) just in case that ever changes.
			// Either way, we need the ctrl-c handling, saves me writing it myself.
			ProcessTerminationTimeout = TimeSpan.FromMinutes(2),
			EnableDefaultExceptionHandler = false,
		});
	}
}
