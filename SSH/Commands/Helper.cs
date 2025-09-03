using Microsoft.CommandPalette.Extensions.Toolkit;
using System.ComponentModel;
using System.Diagnostics;

namespace SSH.Commands;

public static class Helper
{
	public static bool OpenInShell(string path, string? arguments = null, string? workingDir = null,
		ShellHelpers.ShellRunAsType runAs = ShellHelpers.ShellRunAsType.None, bool runWithHiddenWindow = false)
	{
		using var process = new Process();
		process.StartInfo.FileName = path;
		process.StartInfo.WorkingDirectory = string.IsNullOrWhiteSpace(workingDir) ? string.Empty : workingDir;
		process.StartInfo.Arguments = string.IsNullOrWhiteSpace(arguments) ? string.Empty : arguments;
		process.StartInfo.WindowStyle = runWithHiddenWindow ? ProcessWindowStyle.Hidden : ProcessWindowStyle.Normal;
		process.StartInfo.UseShellExecute = true;
		process.StartInfo.Verb = runAs switch
		{
			ShellHelpers.ShellRunAsType.Administrator => "RunAs",
			ShellHelpers.ShellRunAsType.OtherUser => "RunAsUser",
			_ => process.StartInfo.Verb
		};

		try
		{
			process.Start();
			return true;
		}
		catch (Win32Exception)
		{
			// Log.Exception("Unable to open " + path + ": " + ex.Message, ex, MethodBase.GetCurrentMethod().DeclaringType, "OpenInShell", "D:\\a\\_work\\1\\s\\src\\modules\\launcher\\Wox.Infrastructure\\Helper.cs", 192);
			return false;
		}
	}
}