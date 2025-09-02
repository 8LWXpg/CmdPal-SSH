using SSH.Classes.Config;

namespace SSH.Classes;

public static class SshProfile
{
	public static readonly string configPath =
		Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".ssh", "config");

	/// <summary>
	///     Cached hosts that only updates when config changes
	/// </summary>
	private static List<SshHost>? _cachedHosts;

	private static readonly Lock _lock = new();
	private static readonly FileSystemWatcher[] _fileWatchers;

	/// <summary>
	///     Parse config and initialize file watchers
	/// </summary>
	static SshProfile()
	{
		var config = new Parser(configPath);
		_cachedHosts = config.Nodes.ConvertAll(node => new SshHost(node));
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
						_cachedHosts = null;
					}
				};
				fileWatcher.EnableRaisingEvents = true;

				return fileWatcher;
			})
		];
	}

	public static List<SshHost> Hosts
	{
		get
		{
			lock (_lock)
			{
				_cachedHosts ??= new Parser(configPath).Nodes.ConvertAll(node => new SshHost(node));
				return _cachedHosts;
			}
		}
	}
}