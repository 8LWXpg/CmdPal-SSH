using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using SSH.Classes;
using SSH.Commands;
using SSH.Pages;

namespace SSH;

public sealed partial class SshCommandsProvider : CommandProvider
{
	private readonly ICommandItem[] _commands;
	private readonly IFallbackCommandItem[] _fallbackCommands;
	private readonly SettingsManager _settingsManager = new();

	public SshCommandsProvider()
	{
		DisplayName = "SSH";
		Icon = IconHelpers.FromRelativePath("Assets/Square44x44Logo.png");
		Settings = _settingsManager.Settings;
		_commands =
		[
			new CommandItem(new SshPage(_settingsManager)) { Title = DisplayName }
		];
		_fallbackCommands =
		[
			new SshFallbackCommandItem(_settingsManager)
		];
	}

	public override ICommandItem[] TopLevelCommands() => _commands;

	public override IFallbackCommandItem[] FallbackCommands() => _fallbackCommands;
}
