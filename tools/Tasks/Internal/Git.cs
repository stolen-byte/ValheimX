// SPDX-License-Identifier: GPL-3.0-or-later
namespace StolenByte.ValheimX.Tasks;

using System.Diagnostics;
using System.Text.RegularExpressions;

// =============================================================================
/// <summary>
/// Exception thrown by all git commands
/// </summary>
public class GitException : Exception
{
	public GitException()
	{ }

	public GitException(string message)
		: base(message)
	{ }

	public GitException(string message, Exception inner)
		: base(message, inner)
	{ }
}

// =============================================================================
public readonly record struct GitDescribeExtra(int Distance, string Commit);

// =============================================================================
public readonly record struct GitDescribeResult(string Tag, GitDescribeExtra? Extra)
{
	static Regex GIT_DESC_RX = new Regex(@"^(.+?)(?:-(\d+)-g([0-9A-Fa-f]+))?$");

	public static GitDescribeResult Parse(string data)
	{
		var match = GIT_DESC_RX.Match(data);

		if (match.Success) {
			var dGroup = match.Groups[2];
			var cGroup = match.Groups[3];

			if (dGroup.Success && cGroup.Success) {
				return new(
					match.Groups[1].Value,
					new(int.Parse(dGroup.Value, NumberStyles.None, CultureInfo.InvariantCulture), cGroup.Value)
				);
			}
		}

		return new(data, null);
	}
}

// =============================================================================
public class Git
{
	private readonly string m_gitDir;
	private readonly ProcessStartInfo m_psi;

	/// <summary>
	/// Path to the git directory used.
	/// </summary>
	public string RootDir => m_gitDir;

	/// <param name="workingDir">Working directory to use for all git commands</param>
	/// <exception cref="GitException">
	/// Thrown if git cannot find a valid repository from the working directory
	/// </exception>
	public Git(string? workingDir = null)
	{
		m_psi = new ProcessStartInfo {
			FileName = "git",
			RedirectStandardOutput = true,
			RedirectStandardError = true,
			UseShellExecute = false,
			CreateNoWindow = true,
		};

		if (!string.IsNullOrEmpty(workingDir))
			m_psi.WorkingDirectory = workingDir;

		m_gitDir = Run("rev-parse --git-dir").FirstOrDefault();
		if (string.IsNullOrEmpty(m_gitDir))
			throw new GitException("git dir is empty");
	}

	/// <summary>
	/// Returns the most recent commit id, or null if there are no commits.
	/// </summary>
	public string? CommitId()
	{
		try {
			return Run("rev-parse --verify --short=7 HEAD").FirstOrDefault();
		}
		catch (GitException) {
			try {
				Run("show-ref --head --quiet").FirstOrDefault();
			}
			catch (GitException) {
				// no commits
				return null;
			}

			// failed for other reasons, rethrow outer exception
			throw;
		}
	}

	/// <summary>
	/// Returns the commit distance from the specified ref.
	/// If <paramref name="id"/> is null, uses the initial commit instead.
	/// </summary>
	/// <param name="id">Git ref</param>
	public int DistanceFrom(string? id)
	{
		if (id is null) {
			var count = Run("rev-list HEAD").Count();
			return (count > 0) ? count - 1 : count;
		}

		return Run($"rev-list {id}..HEAD").Count();
	}

	/// <summary>
	/// Runs `git describe`
	/// </summary>
	/// <exception cref="GitException">Thrown when there are no tags available</exception>
	public GitDescribeResult Describe()
		=> GitDescribeResult.Parse(Run("describe --tags --abbrev=7").FirstOrDefault());

	/// <exception cref="GitException">
	/// Thrown with `stderr` as its message when git returns a non-zero exit code
	/// </exception>
	public IEnumerable<string> Run(string args)
	{
		m_psi.Arguments = args;

		using (var proc = Process.Start(m_psi)) {
			proc.WaitForExit();

			if (proc.ExitCode != 0)
				throw new GitException(proc.StandardError.ReadToEnd().Trim());

			while (proc.StandardOutput.ReadLine() is { } line) {
				line = line.Trim();
				if (line.Length == 0)
					continue;
				yield return line;
			}
		}

		yield break;
	}
}
