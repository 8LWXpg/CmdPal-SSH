using Microsoft.CommandPalette.Extensions.Toolkit;
using SSH.Classes;
using SSH.Properties;

namespace SSH.Commands;

/// <summary>
/// Fallback row for ssh hosts. CmdPal shows <c>DisplayTitle</c> as the row label,
/// so the matched host is surfaced through Title/Subtitle, which are re-read on
/// every query change.
/// </summary>
internal sealed partial class SshFallbackCommandItem(FallbackSshCommand command) :
	FallbackCommandItem(command, Resources.plugin_name, FallbackId)
{
	private const string FallbackId = "com.8lwxpg.ssh.fallback";

	public SshFallbackCommandItem(SettingsManager settingsManager)
		: this(new FallbackSshCommand(settingsManager))
	{
		// CmdPal updates FallbackHandler, which defaults to the inner command.
		// Point it at this item so the overridden UpdateQuery below runs.
		FallbackHandler = this;
		Icon = IconHelpers.FromRelativePath("Assets/Square44x44Logo.png");
	}

	public override void UpdateQuery(string query)
	{
		command.UpdateQuery(query);
		if (FallbackSshCommand.FindHost(query) is not SshHost host)
		{
			Title = string.Empty;
			Subtitle = string.Empty;
			return;
		}

		Title = host.Host;
		Subtitle = Resources.connect_to_host.Replace("{0}", host.Host, StringComparison.Ordinal);
	}
}
