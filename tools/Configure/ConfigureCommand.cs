// SPDX-License-Identifier: GPL-3.0-or-later
namespace StolenByte.ValheimX.Configure;

using System.CommandLine;
using System.CommandLine.Help;
using System.Diagnostics;
using Serilog;

// =============================================================================
class ConfigureCommand : RootCommand
{
	const string SolutionFileName = $"{Project.ModName}.slnx";
	private readonly string m_argName;

	public ConfigureCommand()
		: base("ValheimX Configuration Tool")
	{
		Options.Clear();

		var props = AddOption<bool?>("-p", "--props", "Generate Build.props only");
		var refs = AddOption<bool?>("-r", "--refs", "Generate publicized reference assemblies only");

		var gameDir = AddArg<DirectoryInfo>(
			"game-dir",
			"Path to Valheim game directory",
			ArgumentArity.ZeroOrOne
		).AcceptExistingOnly();

		Add(new HelpOption());

		m_argName = gameDir.Name;
		SetAction((pr, ct) => Run(
			pr.GetValue(gameDir),
			pr.GetValue(props),
			pr.GetValue(refs),
			ct
		));
	}

	public async Task<int> Run(DirectoryInfo? gameDir, bool? props, bool? refs, CancellationToken ct)
	{
		if (!refs.HasValue && !props.HasValue) {
			refs = true;
			props = true;
		}

		var slnx = GetSolutionFile(new(Directory.GetCurrentDirectory()));
		var mod = Project.FromSolution(slnx, Project.ModName);

		gameDir = EvaluateGameDir(gameDir, mod);
		if (gameDir is null) {
			throw new Error(
				"{Var} not found in previous config, please provide the {Opt} argument.",
				PropNames.GameDir,
				m_argName
			);
		}

		Log.Information("Using game directory {Dir}", gameDir.FullName);
		ct.ThrowIfCancellationRequested();

		var watch = Stopwatch.StartNew();
		var tasks = GetGeneratorTasks(props, refs);
		await Task.WhenAll(tasks.Select(task => task.Run(mod, gameDir, ct)));
		watch.Stop();

		Log.Information("Done in {Time}", watch.Elapsed);

		return 0;
	}

	IEnumerable<IGeneratorTask> GetGeneratorTasks(bool? props, bool? refs)
	{
		if (refs == true)
			yield return new ReferenceGenerator();
		if (props == true)
			yield return new BuildPropsGenerator();
	}

	FileInfo GetSolutionFile(DirectoryInfo rootDir)
	{
		for (var cur = rootDir; cur is not null; cur = cur.Parent) {
			var candidate = Path.Combine(cur.FullName, SolutionFileName);
			if (File.Exists(candidate)) {
				return new(candidate);
			}
		}
		throw new FileNotFoundException("unable to find root solution file", SolutionFileName);
	}

	DirectoryInfo? EvaluateGameDir(DirectoryInfo? cliValue, Project mod)
	{
		if (cliValue is not null)
			return cliValue;
		else if (mod.GetPropertyValue(PropNames.GameDir) is { } path)
			return new(path);
		return null;
	}

	Option<T> AddOption<T>(string? shortName, string longName, string description)
	{
		var opt = shortName switch {
			{ } => new Option<T>(longName, shortName),
			null => new Option<T>(longName),
		};

		opt.Description = description;
		Add(opt);
		return opt;
	}

	Argument<T> AddArg<T>(string name, string description, ArgumentArity? arity = null)
	{
		var arg = new Argument<T>(name) {
			Description = description,
			Arity = arity ?? ArgumentArity.ExactlyOne,
		};
		Add(arg);
		return arg;
	}
}
