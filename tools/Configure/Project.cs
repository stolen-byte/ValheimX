// SPDX-License-Identifier: GPL-3.0-or-later
namespace StolenByte.ValheimX.Configure;

using System.Xml.Linq;
using Microsoft.Build.Evaluation;
using Serilog;
using MSBuildProject = Microsoft.Build.Evaluation.Project;

// =============================================================================
class Project
{
	public const string ModName = "ValheimX";

	private readonly MSBuildProject m_project;
	private readonly DirectoryInfo m_projectRoot;

	public DirectoryInfo RootDir => m_projectRoot;

	Project(MSBuildProject inner)
	{
		m_project = inner;
		m_projectRoot = new(GetPropertyValueOrThrow(PropNames.ProjectRoot));
	}

	public static Project FromSolution(FileInfo slnxFile, string projectName)
	{
		var slnxDir = slnxFile.Directory!;
		var slnx = XDocument.Load(slnxFile.FullName);
		var projectFileName = $"{projectName}.csproj";

		foreach (var projNode in slnx.Descendants("Project")) {
			if (projNode.Attribute("Path")?.Value is not { } relPath)
				continue;

			if (Path.IsPathRooted(relPath))
				continue;

			var candidate = Path.GetFileName(relPath);
			if (string.Equals(candidate, projectFileName, StringComparison.OrdinalIgnoreCase)) {
				return Load(new(Path.Combine(slnxDir.FullName, relPath)));
			}
		}

		throw new ProgramException(
			"Unable to find project file {Project} in {Sln}",
			projectFileName,
			slnxFile.Name
		);
	}

	public static Project Load(FileInfo projectFile)
	{
		Log.Information("Loading {Source}", projectFile.FullName);
		return new(new(projectFile.FullName));
	}

	public string? GetPropertyValue(string name)
		=> m_project.GetProperty(name)?.EvaluatedValue;

	public string GetPropertyValueOrThrow(string name)
	{
		if (GetPropertyValue(name) is { } value)
			return value;
		throw new ProgramException("Property {Name} was not set in project", name);
	}

	public IEnumerable<ProjectItem> GetItems(string name)
		=> m_project.GetItems(name);

	public IEnumerable<string> GetItemIncludes(string name)
		=> m_project.GetItems(name).Select(item => item.EvaluatedInclude);
}
