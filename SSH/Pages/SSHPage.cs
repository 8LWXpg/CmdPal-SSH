using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using SSH.Classes;
using SSH.Commands;
using SSH.Properties;

namespace SSH.Pages;

internal sealed partial class SshPage : ListPage
{
	private readonly SettingsManager _settingsManager;
	private readonly IconInfo _itemIcon;

	public SshPage(SettingsManager settingsManager)
	{
		Icon = IconHelpers.FromRelativePath("Assets/Square44x44Logo.png");
		Title = Resources.plugin_name;
		Name = Resources.plugin_description;
		ShowDetails = true;
		_itemIcon = IconHelpers.FromRelativePaths("Assets/SSH.light.svg", "Assets/SSH.dark.svg");
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
		{
			Title = host.Host,
			Subtitle = $"{host.User}@{host.HostName}",
			Icon = _itemIcon,
			Details = new Details()
			{
				Title = host.Host,
				Metadata = [.. host.Properties.Select((p) =>
					new DetailsElement() { Key = p.Key, Data = new DetailsLink(string.Empty, p.Value) })]
			}
		});

		return [.. results];
	}
}
