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
	internal class WindowHelper
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

		private static bool TryGetMonitorDpi(IntPtr monitorHandle, out DpiScale dpi)
		{
			const double DefaultPixelsPerInch = 96D;

			if (GetDpiForMonitor(
				monitorHandle,
				MONITOR_DPI_TYPE.MDT_Default,
				out uint dpiX,
				out uint dpiY) == 0) // S_OK
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
	}
}