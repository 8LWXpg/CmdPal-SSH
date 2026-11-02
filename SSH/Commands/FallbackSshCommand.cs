using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using SSH.Classes;
using SSH.Properties;
using SSH.Terminal;

namespace SSH.Commands;

internal sealed partial class FallbackSshCommand(SettingsManager settingsManager) : InvokableCommand
{
	private string _query = string.Empty;

	// An empty name makes CmdPal hide the fallback row.
	public override string Name => FindHost(_query) is { } host
		? $"{Resources.plugin_name} {host.Host}"
		: string.Empty;

	public void UpdateQuery(string query) => _query = query;

	public override ICommandResult Invoke()
	{
		if (FindHost(_query) is not SshHost host)
		{
			return CommandResult.KeepOpen();
		}

		var result = TerminalHelper.OpenTerminal(host.Host, host.Host, settingsManager.OpenMode, settingsManager.TerminalType, settingsManager.SuppressTitleChange);
		return result ? CommandResult.GoHome() : CommandResult.KeepOpen();
	}

	/// <summary>
	/// Finds the ssh config host a query refers to. CmdPal owns keyword prefix
	/// stripping, so the raw query is matched first; the "ssh " prefix is only
	/// retried for compatibility.
	/// </summary>
	public static SshHost? FindHost(string query)
	{
		var text = query.StartsWith("ssh ", StringComparison.OrdinalIgnoreCase) ? query[4..].Trim() : query.Trim();
		return string.IsNullOrEmpty(text) ? null : MatchHost(text);
	}

	private static SshHost? MatchHost(string hostQuery) =>
		SshProfile.Hosts.FirstOrDefault(h => h.Host.Equals(hostQuery, StringComparison.OrdinalIgnoreCase))
			?? SshProfile.Hosts.FirstOrDefault(h => h.Host.StartsWith(hostQuery, StringComparison.OrdinalIgnoreCase))
			?? SshProfile.Hosts.FirstOrDefault(h =>
				h.HostName.Contains(hostQuery, StringComparison.OrdinalIgnoreCase)
				|| h.User.Contains(hostQuery, StringComparison.OrdinalIgnoreCase));
}
