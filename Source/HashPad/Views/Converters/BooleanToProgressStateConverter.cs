using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Shell;

namespace HashPad.Views.Converters
{
	[ValueConversion(typeof(bool), typeof(TaskbarItemProgressState))]
	public class BooleanToProgressStateConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is bool sourceValue))
				return DependencyProperty.UnsetValue;

			return sourceValue
				? TaskbarItemProgressState.Normal
				: TaskbarItemProgressState.None;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}