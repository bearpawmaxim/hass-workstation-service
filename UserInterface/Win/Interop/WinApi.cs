using System;
using System.Runtime.InteropServices;

namespace MangaReader.Avalonia.Platform.Win.Interop
{
    /// <summary>
    ///     Win32 API imports.
    /// </summary>
    internal static class WinApi
	{
		private const string User32 = "user32.dll";

        /// <summary>
        ///     Creates, updates or deletes the taskbar icon.
        /// </summary>
        [DllImport("shell32.Dll", CharSet = CharSet.Unicode)]
		public static extern bool Shell_NotifyIcon(NotifyCommand cmd, [In] ref NotifyIconData data);


        /// <summary>
        ///     Creates the helper window that receives messages from the taskar icon.
        /// </summary>
        [DllImport(User32, EntryPoint = "CreateWindowExW", SetLastError = true)]
		public static extern IntPtr CreateWindowEx(int dwExStyle, [MarshalAs(UnmanagedType.LPWStr)] string lpClassName,
			[MarshalAs(UnmanagedType.LPWStr)] string lpWindowName, int dwStyle, int x, int y,
			int nWidth, int nHeight, IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance,
			IntPtr lpParam);


        /// <summary>
        ///     Processes a default windows procedure.
        /// </summary>
        [DllImport(User32)]
		public static extern IntPtr DefWindowProc(IntPtr hWnd, uint msg, IntPtr wparam, IntPtr lparam);

        /// <summary>
        ///     Registers the helper window class.
        /// </summary>
        [DllImport(User32, EntryPoint = "RegisterClassW", SetLastError = true)]
		public static extern short RegisterClass(ref WindowClass lpWndClass);

        /// <summary>
        ///     Registers a listener for a window message.
        /// </summary>
        /// <param name="lpString"></param>
        /// <returns>uint</returns>
        [DllImport(User32, EntryPoint = "RegisterWindowMessageW")]
		public static extern uint RegisterWindowMessage([MarshalAs(UnmanagedType.LPWStr)] string lpString);

        /// <summary>
        ///     Used to destroy the hidden helper window that receives messages from the
        ///     taskbar icon.
        /// </summary>
        /// <param name="hWnd"></param>
        /// <returns>bool</returns>
        [DllImport(User32, SetLastError = true)]
		public static extern bool DestroyWindow(IntPtr hWnd);

        [DllImport(User32, CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern int GetSystemMetrics(int nIndex);
	}
}