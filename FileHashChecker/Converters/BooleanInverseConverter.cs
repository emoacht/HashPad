using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace FileHashChecker.Converters
{
	[ValueConversion(typeof(bool), typeof(bool))]
	public class BooleanInverseConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return ConvertBase(value);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return ConvertBase(value);
		}

		private static object ConvertBase(object value)
		{
			if (!(value is bool))
				return DependencyProperty.UnsetValue;

			return !(bool)value;
		}
	}
}