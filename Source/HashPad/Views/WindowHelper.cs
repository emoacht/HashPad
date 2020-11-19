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
		public static extern bool GetWindowPlacement(
			IntPtr hWnd,
			out WINDOWPLACEMENT lpwndpl);

		[DllImport("User32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool SetWindowPlacement(
			IntPtr hWnd,
			[In] ref WINDOWPLACEMENT lpwndpl);

		[StructLayout(LayoutKind.Sequential)]
		public struct WINDOWPLACEMENT
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

		private const uint SW_SHOWNORMAL = 1;

		#endregion

		private const string WindowLocationName = "WindowLocation";

		public static bool SetWindowLocation(Window window, bool alignCursor)
		{
			Windows.Foundation.Point point;

			if (alignCursor && GetCursorPos(out POINT cursorPoint))
			{
				var dpi = VisualTreeHelper.GetDpi(window);
				point.X = cursorPoint.x - (10 * dpi.DpiScaleX);
				point.Y = cursorPoint.y - (10 * dpi.DpiScaleY);
			}
			else if (PlatformInfo.IsPackaged && SettingsAccessor.TryGetValue(out Windows.Foundation.Point savedPoint, WindowLocationName))
			{
				point = savedPoint;
			}
			else
			{
				return false;
			}

			var handle = new WindowInteropHelper(window).Handle;
			if (!GetWindowPlacement(handle, out WINDOWPLACEMENT placement))
				return false;

			placement.rcNormalPosition.Location = point;
			return SetWindowPlacement(handle, ref placement);
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
	}
}