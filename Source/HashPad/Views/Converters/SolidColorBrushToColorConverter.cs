using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace HashPad.Views.Converters
{
	[ValueConversion(typeof(SolidColorBrush), typeof(Color))]
	public class SolidColorBrushToColorConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is not SolidColorBrush sourceValue)
				return DependencyProperty.UnsetValue;

			return sourceValue.Color;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException();
		}
	}
}