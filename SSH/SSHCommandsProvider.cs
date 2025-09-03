using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using SSH.Classes;
using SSH.Pages;

namespace SSH;

public sealed partial class SshCommandsProvider : CommandProvider
{
	private readonly ICommandItem[] _commands;
	private readonly SettingsManager _settingsManager = new();

	public SshCommandsProvider()
	{
		DisplayName = "SSH";
		Icon = IconHelpers.FromRelativePaths("Assets/SSH.light.svg", "Assets/SSH.dark.svg");
		Settings = _settingsManager.Settings;
		_commands =
		[
			new CommandItem(new SshPage(_settingsManager)) { Title = DisplayName }
		];
	}

	public override ICommandItem[] TopLevelCommands() => _commands;
}