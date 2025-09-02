using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using SSH.Properties;
using SSH.Terminal;

namespace SSH.Commands;

internal sealed partial class OpenTerminalCommand(
	string host,
	string title,
	WindowMode mode,
	TerminalType type,
	bool suppressTitleChange) : InvokableCommand
{
	public override string Name => Resources.open_in_terminal;

	public override ICommandResult Invoke()
	{
		TerminalHelper.OpenTerminal(host, title, mode, type, suppressTitleChange);
		return CommandResult.GoHome();
	}
}