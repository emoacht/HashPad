using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Windows.Foundation.Collections;
using Windows.Storage;

namespace HashPad.Models
{
	/// <summary>
	/// Accessor to <see cref="Windows.Storage.ApplicationData.LocalSettings"/>
	/// </summary>
	/// <remarks>
	/// Usable data types are shown at:
	/// https://docs.microsoft.com/en-us/windows/uwp/design/app-settings/store-and-retrieve-app-data#types-of-app-data
	/// plus byte[] and Enum.
	/// </remarks>
	internal class SettingsAccessor
	{
		private static IPropertySet Values => ApplicationData.Current.LocalSettings.Values;

		private static readonly object _lock = new object();

		public static T GetValue<T>(in string propertyName)
		{
			return TryGetValue(out T propertyValue, propertyName)
				? propertyValue
				: default;
		}

		public static bool TryGetValue<T>(out T propertyValue, in string propertyName)
		{
			lock (_lock)
			{
				if (Values.TryGetValue(propertyName, out object value))
				{
					propertyValue = (T)value;
					return true;
				}
				propertyValue = default;
				return false;
			}
		}

		public static void SetValue<T>(T propertyValue, in string propertyName)
		{
			lock (_lock)
			{
				var value = typeof(T).IsEnum
					? Convert.ChangeType(propertyValue, Enum.GetUnderlyingType(typeof(T)))
					: propertyValue;

				// Add or change value.
				Values[propertyName] = value;
			}
		}

		public static bool RemoveValue(in string propertyName)
		{
			lock (_lock)
			{
				return Values.Remove(propertyName);
			}
		}
	}
}