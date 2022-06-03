using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using Windows.Services.Store;
using WinRT.Interop;

namespace HashPad.Models
{
	internal static class StoreHelper
	{
		/// <summary>
		/// Checks if updated packages are available.
		/// </summary>
		/// <returns>True if available</returns>
		/// <remarks>
		/// If the packages are installed locally or published but as Package flights, this method
		/// will not work correctly.
		/// </remarks>
		public static async Task<bool> CheckUpdateAsync()
		{
			if (!PlatformInfo.IsPackaged || !NetworkInterface.GetIsNetworkAvailable())
				return false;

			var context = StoreContext.GetDefault();

			try
			{
				var updates = await context.GetAppAndOptionalStorePackageUpdatesAsync();
				return (updates.Count > 0);
			}
			catch
			{
				return false;
			}
		}

		/// <summary>
		/// Proceeds to download and install updated packages.
		/// </summary>
		/// <param name="window">Owner window</param>
		/// <returns>True if successfully finished downloading and installing</returns>
		public static async Task<bool> ProceedUpdateAsync(Window window)
		{
			if (window is null)
				throw new ArgumentNullException(nameof(window));

			if (!PlatformInfo.IsPackaged || !NetworkInterface.GetIsNetworkAvailable())
				return false;

			var context = StoreContext.GetDefault();

			try
			{
				var updates = await context.GetAppAndOptionalStorePackageUpdatesAsync();
				if (updates.Count == 0)
					return false;

				SetOwnerWindow(context, window);

				var result = await context.RequestDownloadAndInstallStorePackageUpdatesAsync(updates);
				return (result.OverallState == StorePackageUpdateState.Completed);
			}
			catch
			{
				return false;
			}
		}

		private static void SetOwnerWindow(StoreContext context, Window window)
		{
			var handle = new WindowInteropHelper(window).Handle;
			InitializeWithWindow.Initialize(context, handle);
		}
	}
}