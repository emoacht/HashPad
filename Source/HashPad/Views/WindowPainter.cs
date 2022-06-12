using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using Microsoft.Win32;

using static HashPad.Views.WindowHelper;

namespace HashPad.Views;

internal static class WindowPainter
{
	#region Win32

	[DllImport("Dwmapi.dll", SetLastError = true)]
	private static extern int DwmSetWindowAttribute(
		IntPtr hwnd,
		DWMWA dwAttribute,
		[In] ref uint pvAttribute, // IntPtr
		uint cbAttribute);

	// Derived from dwmapi.h included in Windows 11 SDK
	private enum DWMWA : uint
	{
		DWMWA_NCRENDERING_ENABLED = 1,        // [get] Is non-client rendering enabled/disabled
		DWMWA_NCRENDERING_POLICY,             // [set] DWMNCRENDERINGPOLICY - Non-client rendering policy
		DWMWA_TRANSITIONS_FORCEDISABLED,      // [set] Potentially enable/forcibly disable transitions
		DWMWA_ALLOW_NCPAINT,                  // [set] Allow contents rendered in the non-client area to be visible on the DWM-drawn frame.
		DWMWA_CAPTION_BUTTON_BOUNDS,          // [get] Bounds of the caption button area in window-relative space.
		DWMWA_NONCLIENT_RTL_LAYOUT,           // [set] Is non-client content RTL mirrored
		DWMWA_FORCE_ICONIC_REPRESENTATION,    // [set] Force this window to display iconic thumbnails.
		DWMWA_FLIP3D_POLICY,                  // [set] Designates how Flip3D will treat the window.
		DWMWA_EXTENDED_FRAME_BOUNDS,          // [get] Gets the extended frame bounds rectangle in screen space
		DWMWA_HAS_ICONIC_BITMAP,              // [set] Indicates an available bitmap when there is no better thumbnail representation.
		DWMWA_DISALLOW_PEEK,                  // [set] Don't invoke Peek on the window.
		DWMWA_EXCLUDED_FROM_PEEK,             // [set] LivePreview exclusion information
		DWMWA_CLOAK,                          // [set] Cloak or uncloak the window
		DWMWA_CLOAKED,                        // [get] Gets the cloaked state of the window
		DWMWA_FREEZE_REPRESENTATION,          // [set] BOOL, Force this window to freeze the thumbnail without live update
		DWMWA_PASSIVE_UPDATE_MODE,            // [set] BOOL, Updates the window only when desktop composition runs for other reasons
		DWMWA_USE_HOSTBACKDROPBRUSH,          // [set] BOOL, Allows the use of host backdrop brushes for the window.
		DWMWA_USE_IMMERSIVE_DARK_MODE = 20,   // [set] BOOL, Allows a window to either use the accent color, or dark, according to the user Color Mode preferences.
		DWMWA_WINDOW_CORNER_PREFERENCE = 33,  // [set] WINDOW_CORNER_PREFERENCE, Controls the policy that rounds top-level window corners
		DWMWA_BORDER_COLOR,                   // [set] COLORREF, The color of the thin border around a top-level window
		DWMWA_CAPTION_COLOR,                  // [set] COLORREF, The color of the caption
		DWMWA_TEXT_COLOR,                     // [set] COLORREF, The color of the caption text
		DWMWA_VISIBLE_FRAME_BORDER_THICKNESS, // [get] UINT, width of the visible border around a thick frame window
		DWMWA_SYSTEMBACKDROP_TYPE,            // [get, set] SYSTEMBACKDROP_TYPE, Controls the system-drawn backdrop material of a window, including behind the non-client area.
		DWMWA_LAST
	}

	// Derived from dwmapi.h included in Windows 11 SDK
	private enum DWMWCP : uint
	{
		/// <summary>
		/// Let the system decide whether or not to round window corners
		/// </summary>
		DWMWCP_DEFAULT = 0,

		/// <summary>
		/// Never round window corners
		/// </summary>
		DWMWCP_DONOTROUND = 1,

		/// <summary>
		/// Round the corners if appropriate
		/// </summary>
		DWMWCP_ROUND = 2,

		/// <summary>
		/// Round the corners if appropriate, with a small radius
		/// </summary>
		DWMWCP_ROUNDSMALL = 3
	}

	private enum DWMSBT : uint
	{
		DWMSBT_AUTO = 0,        // [Default] Let DWM automatically decide the system-drawn backdrop for this window.
		DWMSBT_NONE,            // Do not draw any system backdrop.
		DWMSBT_MAINWINDOW,      // Draw the backdrop material effect corresponding to a long-lived window.
		DWMSBT_TRANSIENTWINDOW, // Draw the backdrop material effect corresponding to a transient window.
		DWMSBT_TABBEDWINDOW     // Draw the backdrop material effect corresponding to a window with a tabbed title bar.
	}

	#endregion

	public static bool SetWindowCorners(Window window, CornerPreference corner)
	{
		if (Environment.OSVersion.Version < new Version(10, 0, 22000) /* Windows 11 */)
			return false;

		var windowHandle = new WindowInteropHelper(window).Handle;

		uint value;
		switch (corner)
		{
			case CornerPreference.NotRound:
				value = (uint)DWMWCP.DWMWCP_DONOTROUND;
				break;
			case CornerPreference.Round:
				value = (uint)DWMWCP.DWMWCP_ROUND;
				break;
			default:
				return false;
		}

		return (DwmSetWindowAttribute(
			windowHandle,
			DWMWA.DWMWA_WINDOW_CORNER_PREFERENCE,
			ref value,
			(uint)Marshal.SizeOf(value)) == S_OK);
	}

	public static bool SetMicaBackground(Window window)
	{
		if (Environment.OSVersion.Version < new Version(10, 0, 22621) /* Windows 11 22621 */)
			return false;

		var windowHandle = new WindowInteropHelper(window).Handle;

		uint value = (uint)DWMSBT.DWMSBT_MAINWINDOW;
		return (DwmSetWindowAttribute(
			windowHandle,
			DWMWA.DWMWA_SYSTEMBACKDROP_TYPE,
			ref value,
			(uint)Marshal.SizeOf<uint>()) == S_OK);
	}

	/// <summary>
	/// Whether the color theme for applications is dark
	/// </summary>
	public static bool IsAppDarkTheme => _isAppDarkTheme.Value;
	private static readonly Lazy<bool> _isAppDarkTheme = new(() => IsDarkTheme("AppsUseLightTheme"));

	private static bool IsDarkTheme(string valueName)
	{
		const string keyName = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"; // HKCU

		using var key = Registry.CurrentUser.OpenSubKey(keyName);

		return (key?.GetValue(valueName) is 0);
	}
}

/// <summary>
/// Corner preferences of window
/// </summary>
public enum CornerPreference
{
	/// <summary>
	/// None
	/// </summary>
	None = 0,

	/// <summary>
	/// Not round
	/// </summary>
	NotRound,

	/// <summary>
	/// Round
	/// </summary>
	Round
}