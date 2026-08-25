using System.Diagnostics.CodeAnalysis;
using SSH.Classes.Config;

namespace SSH.Classes;

public static class SshProfile
{
	public static readonly string configPath =
		Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".ssh", "config");
	private static readonly Lock _lock = new();
	private static readonly FileSystemWatcher[] _fileWatchers;

	/// <summary>
	///     Parse config and initialize file watchers
	/// </summary>
	static SshProfile()
	{
		var config = new Parser(configPath);
		Hosts = config.Nodes.ConvertAll(node => new SshHost(node));
		HashSet<string> includes = config.Includes;
		_fileWatchers =
		[
			.. includes.Select(inc =>
			{
				var fileWatcher = new FileSystemWatcher
				{
					Path = Path.GetDirectoryName(inc) ?? string.Empty,
					Filter = Path.GetFileName(inc),
					NotifyFilter = NotifyFilters.LastWrite
				};

				fileWatcher.Changed += (_, _) =>
				{
					lock (_lock)
					{
						Hosts = null;
					}
				};
				fileWatcher.EnableRaisingEvents = true;

				return fileWatcher;
			})
		];
	}

	[AllowNull]
	public static List<SshHost> Hosts
	{
		get
		{
			lock (_lock)
			{
				field ??= new Parser(configPath).Nodes.ConvertAll(node => new SshHost(node));
				return field;
			}
		}

		private set;
	}
}
