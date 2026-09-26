using System.Diagnostics;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace SSH.Commands;

/// <summary>
/// Windows Terminal never activates its own window when launched via wt.exe (it hands off to
/// WindowsTerminal.exe or an existing instance), and on Dismiss Command Palette's hidden window
/// keeps/regains foreground. So after launching we find the terminal window ourselves and bring
/// it to the foreground. Command Palette grants our COM server foreground rights
/// (CoAllowSetForegroundWindow); the Alt-key tap and AttachThreadInput are fallbacks.
/// </summary>
internal static class TerminalFocus
{
	private const string WtWindowClass = "CASCADIA_HOSTING_WINDOW_CLASS";
	private const int PollIntervalMs = 50;
	private const int TimeoutMs = 5000;
	private const int FallbackAfterMs = 1500;
	private const int SettleDelayMs = 150;
	private const int RecheckDelayMs = 170;
	private const int MaxAttempts = 3;

	/// <summary>
	/// Windows Terminal top-level windows that are visible right now. A window counts as "new"
	/// later if it is visible then but wasn't visible here: this covers a brand-new window
	/// (Default mode) and an already-existing but hidden Quake window that gets shown.
	/// </summary>
	public static HashSet<nint> Snapshot() => [.. GetTerminalWindows(visibleOnly: true)];

	/// <summary>Finds and focuses the launched terminal on a background task. Never throws.</summary>
	public static void FocusInBackground(HashSet<nint> before)
	{
		_ = Task.Run(async () =>
		{
			try
			{
				await FocusAsync(before).ConfigureAwait(false);
			}
#pragma warning disable CA1031 // Never let an exception escape and crash the COM server.
			catch (Exception ex)
#pragma warning restore CA1031
			{
				Log($"unexpected error: {ex}");
			}
		});
	}

	private static async Task FocusAsync(HashSet<nint> before)
	{
		Log($"snapshot had {before.Count} visible WT window(s); hidden WT windows: {GetTerminalWindows(visibleOnly: false).Count(h => !NativeMethods.IsWindowVisible(h))}");
		var sw = Stopwatch.StartNew();
		nint target = 0;
		while (sw.ElapsedMilliseconds < TimeoutMs)
		{
			var visible = GetTerminalWindows(visibleOnly: true); // Z-order, topmost first
			var fresh = visible.FirstOrDefault(h => !before.Contains(h));
			if (fresh != 0)
			{
				target = fresh;
				Log($"newly visible WT window {Hex(fresh)} found after {sw.ElapsedMilliseconds} ms");
				break;
			}

			// New-tab mode (or Quake already showing): no window became visible, so use the
			// topmost visible WT window, which is the one that just got the new tab.
			if (sw.ElapsedMilliseconds >= FallbackAfterMs && before.Count > 0 && visible.Count > 0)
			{
				target = visible[0];
				Log($"no new WT window after {sw.ElapsedMilliseconds} ms; falling back to topmost existing {Hex(target)}");
				break;
			}

			await Task.Delay(PollIntervalMs).ConfigureAwait(false);
		}

		if (target == 0)
		{
			Log($"no newly visible WT window within {TimeoutMs} ms (and no visible existing one); giving up");
			return;
		}

		// Let Command Palette finish hiding and handing foreground back before we take it.
		await Task.Delay(SettleDelayMs).ConfigureAwait(false);

		for (var attempt = 1; attempt <= MaxAttempts; attempt++)
		{
			if (NativeMethods.GetForegroundWindow() != target)
			{
				Log($"attempt {attempt}: foreground is {Hex(NativeMethods.GetForegroundWindow())}, forcing {Hex(target)}");
				ForceForeground(target);
			}

			// Re-check in case Command Palette's hand-back steals it again.
			await Task.Delay(RecheckDelayMs).ConfigureAwait(false);
		}

		var fg = NativeMethods.GetForegroundWindow();
		Log(fg == target ? $"done: {Hex(target)} is foreground" : $"failed: foreground is {Hex(fg)}, wanted {Hex(target)}");
	}

	private static bool ForceForeground(nint hwnd)
	{
		if (NativeMethods.IsIconic(hwnd))
		{
			Log($"ShowWindow(SW_RESTORE) -> {NativeMethods.ShowWindow(hwnd, NativeMethods.SW_RESTORE)}");
		}

		var ok = NativeMethods.SetForegroundWindow(hwnd);
		if (NativeMethods.GetForegroundWindow() == hwnd)
		{
			Log($"SetForegroundWindow -> {ok}, succeeded");
			return true;
		}

		Log($"SetForegroundWindow -> {ok}, not foreground; trying Alt-key tap");
		var sent = NativeMethods.SendAltTap();
		ok = NativeMethods.SetForegroundWindow(hwnd);
		if (NativeMethods.GetForegroundWindow() == hwnd)
		{
			Log($"Alt tap (SendInput sent {sent}) + SetForegroundWindow -> {ok}, succeeded");
			return true;
		}

		Log($"Alt tap (SendInput sent {sent}) + SetForegroundWindow -> {ok}, not foreground; trying AttachThreadInput");
		var fg = NativeMethods.GetForegroundWindow();
		var fgThread = fg != 0 ? NativeMethods.GetWindowThreadId(fg) : 0;
		var ourThread = NativeMethods.GetCurrentThreadId();
		var attached = fgThread != 0 && fgThread != ourThread && NativeMethods.AttachThreadInput(ourThread, fgThread, true);
		try
		{
			var top = NativeMethods.BringWindowToTop(hwnd);
			ok = NativeMethods.SetForegroundWindow(hwnd);
			Log($"AttachThreadInput({ourThread}->{fgThread}) -> {attached}, BringWindowToTop -> {top}, SetForegroundWindow -> {ok}");
		}
		finally
		{
			if (attached)
			{
				_ = NativeMethods.AttachThreadInput(ourThread, fgThread, false);
			}
		}

		var success = NativeMethods.GetForegroundWindow() == hwnd;
		Log(success ? "AttachThreadInput path succeeded" : "AttachThreadInput path failed");
		return success;
	}

	private static List<nint> GetTerminalWindows(bool visibleOnly) =>
		[.. NativeMethods.GetTopLevelWindows().Where(h =>
			(!visibleOnly || NativeMethods.IsWindowVisible(h)) &&
			NativeMethods.GetClassName(h) == WtWindowClass)];

	private static string Hex(nint h) => $"0x{h:X}";

	private static void Log(string message)
	{
		try
		{
			ExtensionHost.LogMessage($"SSH focus: {message}");
		}
#pragma warning disable CA1031 // Logging must never throw.
		catch
#pragma warning restore CA1031
		{
		}
	}
}
