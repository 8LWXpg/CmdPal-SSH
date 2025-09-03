using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using SSH.Classes;
using SSH.Commands;
using SSH.Properties;

namespace SSH.Pages;

internal sealed partial class SshPage : ListPage
{
	private readonly SettingsManager _settingsManager;

	public SshPage(SettingsManager settingsManager)
	{
		Icon = IconHelpers.FromRelativePaths("Assets/SSH.light.svg", "Assets/SSH.dark.svg");
		Title = Resources.plugin_name;
		Name = Resources.plugin_description;
		_settingsManager = settingsManager;
	}

	public override IListItem[] GetItems()
	{
		List<ListItem> results = SshProfile.Hosts.ConvertAll(host => new ListItem(
			new OpenTerminalCommand(
				host.Host,
				host.Host,
				_settingsManager.OpenMode,
				_settingsManager.TerminalType,
				_settingsManager.SuppressTitleChange)
		)
		{ Title = host.Host, Subtitle = $"{host.User}@{host.HostName}", Icon = Icon });

		return [.. results];
	}
}