using Microsoft.CommandPalette.Extensions.Toolkit;
using SSH.Properties;
using SSH.Terminal;
using System.Globalization;

namespace SSH.Classes;

internal sealed class SettingsManager : JsonSettingsManager
{
	public WindowMode OpenMode => (WindowMode)int.Parse(_openMode.Value ?? "0", NumberStyles.Integer, CultureInfo.InvariantCulture);

	private readonly ChoiceSetSetting _openMode = new(
		nameof(OpenMode),
		Resources.open_mode,
		Resources.open_mode_desc,
		[
			new ChoiceSetSetting.Choice(Resources.open_mode_default, "0"),
			new ChoiceSetSetting.Choice(Resources.open_mode_new_tab, "1"),
			new ChoiceSetSetting.Choice(Resources.open_mode_quake, "2")
		]);

	public TerminalType TerminalType => (TerminalType)int.Parse(_terminalType.Value ?? "0", NumberStyles.Integer, CultureInfo.InvariantCulture);

	private readonly ChoiceSetSetting _terminalType = new(
		nameof(TerminalType),
		Resources.terminal,
		Resources.terminal_desc,
		[
			new ChoiceSetSetting.Choice("Windows Terminal", "0"),
			new ChoiceSetSetting.Choice("WezTerm", "1"),
		]);

	public bool SuppressTitleChange => _suppressTitleChange.Value;

	private readonly ToggleSetting _suppressTitleChange = new(
		nameof(SuppressTitleChange),
		Resources.suppress_title_change,
		Resources.suppress_title_change_desc,
		false);

	public SettingsManager()
	{
		FilePath = SettingsJsonPath();
		Settings.Add(_openMode);
		Settings.Add(_terminalType);
		Settings.Add(_suppressTitleChange);

		LoadSettings();

		Settings.SettingsChanged += (_, _) => SaveSettings();
	}

	private static string SettingsJsonPath()
	{
		var directory = Utilities.BaseSettingsPath("SshExtension");
		_ = Directory.CreateDirectory(directory);
		return Path.Combine(directory, "settings.json");
	}
}