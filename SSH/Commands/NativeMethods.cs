using System.Runtime.InteropServices;

namespace SSH.Commands;

internal static partial class NativeMethods
{
	/// <summary>Special value for <see cref="AllowSetForegroundWindow"/>: allow any process.</summary>
	internal const int ASFW_ANY = unchecked((int)0xFFFFFFFF);

	internal const int SW_RESTORE = 9;
	internal const uint INPUT_KEYBOARD = 1;
	internal const uint KEYEVENTF_KEYUP = 0x0002;
	internal const ushort VK_MENU = 0x12;

	/// <summary>
	/// Lets another process (here: the terminal we are about to launch) take the foreground.
	/// Command Palette grants our COM server foreground rights via CoAllowSetForegroundWindow,
	/// but those rights are not inherited by child processes (wt.exe -> WindowsTerminal.exe),
	/// so we explicitly pass them on before starting the terminal.
	/// </summary>
	[LibraryImport("user32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	internal static partial bool AllowSetForegroundWindow(int dwProcessId);

	// ---- Window enumeration / focus (used by TerminalFocus) ----

	[LibraryImport("user32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static unsafe partial bool EnumWindows(delegate* unmanaged<nint, nint, int> lpEnumFunc, nint lParam);

	[LibraryImport("user32.dll", EntryPoint = "GetClassNameW", SetLastError = true)]
	private static unsafe partial int GetClassNameW(nint hWnd, char* lpClassName, int nMaxCount);

	[LibraryImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	internal static partial bool IsWindowVisible(nint hWnd);

	[LibraryImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	internal static partial bool IsIconic(nint hWnd);

	[LibraryImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	internal static partial bool ShowWindow(nint hWnd, int nCmdShow);

	[LibraryImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	internal static partial bool SetForegroundWindow(nint hWnd);

	[LibraryImport("user32.dll")]
	internal static partial nint GetForegroundWindow();

	[LibraryImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	internal static partial bool BringWindowToTop(nint hWnd);

	[LibraryImport("user32.dll")]
	private static unsafe partial uint GetWindowThreadProcessId(nint hWnd, uint* lpdwProcessId);

	[LibraryImport("kernel32.dll")]
	internal static partial uint GetCurrentThreadId();

	[LibraryImport("user32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	internal static partial bool AttachThreadInput(uint idAttach, uint idAttachTo, [MarshalAs(UnmanagedType.Bool)] bool fAttach);

	[LibraryImport("user32.dll", SetLastError = true)]
	private static unsafe partial uint SendInput(uint cInputs, INPUT* pInputs, int cbSize);

	// INPUT layout for x64: 4-byte type + 4 padding + 32-byte union (sized by MOUSEINPUT) = 40 bytes.
	[StructLayout(LayoutKind.Sequential)]
	private struct INPUT
	{
		public uint Type;
		public InputUnion U;
	}

	[StructLayout(LayoutKind.Explicit)]
	private struct InputUnion
	{
		[FieldOffset(0)] public MOUSEINPUT Mi;
		[FieldOffset(0)] public KEYBDINPUT Ki;
	}

	[StructLayout(LayoutKind.Sequential)]
	private struct MOUSEINPUT
	{
		public int Dx;
		public int Dy;
		public uint MouseData;
		public uint DwFlags;
		public uint Time;
		public nint DwExtraInfo;
	}

	[StructLayout(LayoutKind.Sequential)]
	private struct KEYBDINPUT
	{
		public ushort WVk;
		public ushort WScan;
		public uint DwFlags;
		public uint Time;
		public nint DwExtraInfo;
	}

	/// <summary>Returns all top-level windows in Z-order (topmost first).</summary>
	internal static unsafe List<nint> GetTopLevelWindows()
	{
		var list = new List<nint>();
		var handle = GCHandle.Alloc(list);
		try
		{
			_ = EnumWindows(&EnumWindowsCallback, GCHandle.ToIntPtr(handle));
		}
		finally
		{
			handle.Free();
		}

		return list;
	}

	[UnmanagedCallersOnly]
	private static int EnumWindowsCallback(nint hWnd, nint lParam)
	{
		if (GCHandle.FromIntPtr(lParam).Target is List<nint> list)
		{
			list.Add(hWnd);
		}

		return 1; // continue enumeration
	}

	internal static unsafe string GetClassName(nint hWnd)
	{
		const int Max = 256;
		var buffer = stackalloc char[Max];
		var len = GetClassNameW(hWnd, buffer, Max);
		return len > 0 ? new string(buffer, 0, len) : string.Empty;
	}

	internal static unsafe uint GetWindowThreadId(nint hWnd) => GetWindowThreadProcessId(hWnd, null);

	/// <summary>Taps the Alt key (down + up) so Windows treats us as having the last input event,
	/// which lifts the foreground lock for SetForegroundWindow.</summary>
	internal static unsafe uint SendAltTap()
	{
		var inputs = stackalloc INPUT[2];
		inputs[0] = new INPUT { Type = INPUT_KEYBOARD, U = new InputUnion { Ki = new KEYBDINPUT { WVk = VK_MENU } } };
		inputs[1] = new INPUT { Type = INPUT_KEYBOARD, U = new InputUnion { Ki = new KEYBDINPUT { WVk = VK_MENU, DwFlags = KEYEVENTF_KEYUP } } };
		return SendInput(2, inputs, sizeof(INPUT));
	}
}
