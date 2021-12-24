using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

using HashPad.Models;

namespace HashPad.Views
{
	internal static class WindowHelper
	{
		#region Win32

		[DllImport("User32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool GetCursorPos(out POINT lpPoint);

		[StructLayout(LayoutKind.Sequential)]
		public struct POINT
		{
			public int x;
			public int y;

			public static implicit operator Windows.Foundation.Point(POINT point) => new Windows.Foundation.Point(point.x, point.y);
			public static implicit operator POINT(Windows.Foundation.Point point) => new POINT { x = (int)point.X, y = (int)point.Y };
		}

		[DllImport("user32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool GetWindowPlacement(
			IntPtr hWnd,
			out WINDOWPLACEMENT lpwndpl);

		[DllImport("User32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool SetWindowPlacement(
			IntPtr hWnd,
			[In] ref WINDOWPLACEMENT lpwndpl);

		[StructLayout(LayoutKind.Sequential)]
		private struct WINDOWPLACEMENT
		{
			public uint length;
			public uint flags;
			public uint showCmd;
			public POINT ptMinPosition;
			public POINT ptMaxPosition;
			public RECT rcNormalPosition;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct RECT
		{
			public int left;
			public int top;
			public int right;
			public int bottom;

			public POINT Location
			{
				get => new POINT { x = left, y = top };
				set => (left, top) = (value.x, value.y);
			}

			public int Width => right - left;
			public int Height => bottom - top;

			public RECT(int left, int top, int right, int bottom)
			{
				this.left = left;
				this.top = top;
				this.right = right;
				this.bottom = bottom;
			}

			public static implicit operator RECT(Windows.Foundation.Rect rect) => new RECT((int)rect.Left, (int)rect.Top, (int)rect.Right, (int)rect.Bottom);
			public static implicit operator Windows.Foundation.Rect(RECT rect) => new Windows.Foundation.Rect(rect.left, rect.top, rect.Width, rect.Height);
		}

		[DllImport("User32.dll")]
		private static extern IntPtr MonitorFromPoint(
			POINT pt,
			MONITOR_DEFAULTTO dwFlags);

		[DllImport("User32.dll")]
		private static extern IntPtr MonitorFromWindow(
			IntPtr hwnd,
			MONITOR_DEFAULTTO dwFlags);

		private enum MONITOR_DEFAULTTO : uint
		{
			/// <summary>
			/// If no display monitor intersects, returns null.
			/// </summary>
			MONITOR_DEFAULTTONULL = 0x00000000,

			/// <summary>
			/// If no display monitor intersects, returns a handle to the primary display monitor.
			/// </summary>
			MONITOR_DEFAULTTOPRIMARY = 0x00000001,

			/// <summary>
			/// If no display monitor intersects, returns a handle to the display monitor that is nearest to the rectangle.
			/// </summary>
			MONITOR_DEFAULTTONEAREST = 0x00000002,
		}

		[DllImport("Shcore.dll", SetLastError = true)]
		private static extern int GetDpiForMonitor(
			IntPtr hmonitor,
			MONITOR_DPI_TYPE dpiType,
			out uint dpiX,
			out uint dpiY);

		private enum MONITOR_DPI_TYPE
		{
			/// <summary>
			/// Effective DPI that incorporates accessibility overrides and matches what Desktop Window Manage (DWM) uses to scale desktop applications
			/// </summary>
			MDT_Effective_DPI = 0,

			/// <summary>
			/// DPI that ensures rendering at a compliant angular resolution on the screen, without incorporating accessibility overrides
			/// </summary>
			MDT_Angular_DPI = 1,

			/// <summary>
			/// Linear DPI of the screen as measured on the screen itself
			/// </summary>
			MDT_Raw_DPI = 2,

			/// <summary>
			/// Default DPI
			/// </summary>
			MDT_Default = MDT_Effective_DPI
		}

		[DllImport("User32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool GetMonitorInfo(
			IntPtr hMonitor,
			ref MONITORINFO lpmi);

		[StructLayout(LayoutKind.Sequential)]
		private struct MONITORINFO
		{
			public uint cbSize;
			public RECT rcMonitor;
			public RECT rcWork;
			public uint dwFlags;
		}

		[DllImport("Dwmapi.dll", SetLastError = true)]
		private static extern int DwmSetWindowAttribute(
			IntPtr hwnd,
			uint dwAttribute,
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

		private const int S_OK = 0x0;

		#endregion

		private const string WindowLocationName = "WindowLocation";

		public static bool SetWindowLocation(Window window, bool alignCursor)
		{
			IntPtr windowHandle;
			WINDOWPLACEMENT windowPlacement;

			if (alignCursor && GetCursorPos(out POINT cursorPoint))
			{
				windowHandle = new WindowInteropHelper(window).Handle;
				if (!GetWindowPlacement(windowHandle, out windowPlacement))
					return false;

				var monitorHandle = MonitorFromPoint(cursorPoint, MONITOR_DEFAULTTO.MONITOR_DEFAULTTONEAREST);
				if (!TryGetMonitorDpi(monitorHandle, out DpiScale targetDpi) ||
					!TryGetMonitorRect(monitorHandle, out _, out Windows.Foundation.Rect workRect))
					return false;

				var originDpi = VisualTreeHelper.GetDpi(window);

				// Move window's location in left-top direction to make it easily draggable.
				var x = cursorPoint.x - (8 * targetDpi.DpiScaleX);
				var y = cursorPoint.y - (8 * targetDpi.DpiScaleY);

				var width = windowPlacement.rcNormalPosition.Width / originDpi.DpiScaleX * targetDpi.DpiScaleX;
				var height = windowPlacement.rcNormalPosition.Height / originDpi.DpiScaleY * targetDpi.DpiScaleY;

				// Make sure window's right-bottom corner is inside the work area of monitor where the cursor is.
				x = Math.Min(x + width, workRect.Right) - width;
				y = Math.Min(y + height, workRect.Bottom) - height;

				// Make sure window's left-top corner as well.
				x = Math.Max(x, workRect.Left);
				y = Math.Max(y, workRect.Top);

				windowPlacement.rcNormalPosition.Location = new Windows.Foundation.Point(x, y);
			}
			else if (PlatformInfo.IsPackaged && SettingsAccessor.TryGetValue(out Windows.Foundation.Point savedPoint, WindowLocationName))
			{
				windowHandle = new WindowInteropHelper(window).Handle;
				if (!GetWindowPlacement(windowHandle, out windowPlacement))
					return false;

				windowPlacement.rcNormalPosition.Location = savedPoint;
			}
			else
			{
				return false;
			}

			return SetWindowPlacement(windowHandle, ref windowPlacement);
		}

		public static void SaveWindowLocation(Window window)
		{
			if (!PlatformInfo.IsPackaged)
				return;

			var handle = new WindowInteropHelper(window).Handle;
			if (!GetWindowPlacement(handle, out WINDOWPLACEMENT placement))
				return;

			SettingsAccessor.SetValue((Windows.Foundation.Point)placement.rcNormalPosition.Location, WindowLocationName);
		}

		public static void EnsureWindowLocation(Window window, Size newSize)
		{
			var windowHandle = new WindowInteropHelper(window).Handle;
			if (!GetWindowPlacement(windowHandle, out WINDOWPLACEMENT windowPlacement))
				return;

			var monitorHandle = MonitorFromWindow(windowHandle, MONITOR_DEFAULTTO.MONITOR_DEFAULTTONEAREST);
			if (!TryGetMonitorRect(monitorHandle, out _, out Windows.Foundation.Rect workRect))
				return;

			var currentDpi = VisualTreeHelper.GetDpi(window);

			var width = newSize.Width * currentDpi.DpiScaleX;
			var height = newSize.Height * currentDpi.DpiScaleY;

			// Make sure window's right-bottom corner is inside the work area of monitor.			
			var x = Math.Min(windowPlacement.rcNormalPosition.left + width, workRect.Right) - width;
			var y = Math.Min(windowPlacement.rcNormalPosition.top + height, workRect.Bottom) - height;

			// Make sure window's left-top corner as well.
			x = Math.Max(x, workRect.Left);
			y = Math.Max(y, workRect.Top);

			POINT newLocation = new Windows.Foundation.Point(x, y);
			if (windowPlacement.rcNormalPosition.Location.Equals(newLocation))
				return;

			windowPlacement.rcNormalPosition.Location = newLocation;
			SetWindowPlacement(windowHandle, ref windowPlacement);
		}

		private static bool TryGetMonitorDpi(IntPtr monitorHandle, out DpiScale dpi)
		{
			const double DefaultPixelsPerInch = 96D;

			if (GetDpiForMonitor(
				monitorHandle,
				MONITOR_DPI_TYPE.MDT_Default,
				out uint dpiX,
				out uint dpiY) == S_OK)
			{
				dpi = new DpiScale(dpiX / DefaultPixelsPerInch, dpiY / DefaultPixelsPerInch);
				return true;
			}
			dpi = default;
			return false;
		}

		private static bool TryGetMonitorRect(IntPtr monitorHandle, out Windows.Foundation.Rect monitorRect, out Windows.Foundation.Rect workRect)
		{
			var monitorInfo = new MONITORINFO { cbSize = (uint)Marshal.SizeOf<MONITORINFO>() };
			if (GetMonitorInfo(monitorHandle, ref monitorInfo))
			{
				monitorRect = monitorInfo.rcMonitor;
				workRect = monitorInfo.rcWork;
				return true;
			}
			monitorRect = default;
			workRect = default;
			return false;
		}

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
				(uint)DWMWA.DWMWA_WINDOW_CORNER_PREFERENCE,
				ref value,
				(uint)Marshal.SizeOf(value)) == S_OK);
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
}