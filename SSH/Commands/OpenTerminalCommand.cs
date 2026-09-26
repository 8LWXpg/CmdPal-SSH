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
		// Pass on the foreground rights Command Palette just granted us (via
		// CoAllowSetForegroundWindow) immediately, while they are still valid, so the
		// terminal we launch can take foreground/keyboard focus.
		_ = NativeMethods.AllowSetForegroundWindow(NativeMethods.ASFW_ANY);

		// Windows Terminal doesn't activate its own window, so remember which WT windows
		// already exist, launch, then find the (new) terminal window and focus it ourselves.
		var existingWtWindows = type == TerminalType.WindowsTerminal ? TerminalFocus.Snapshot() : null;
		var launched = TerminalHelper.OpenTerminal(host, title, mode, type, suppressTitleChange);
		if (launched && existingWtWindows is not null)
		{
			TerminalFocus.FocusInBackground(existingWtWindows);
		}

		return CommandResult.Dismiss();
	}
}
