using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace HashPad.Views.Controls;

public class TextBoxProperty
{
	public static bool GetSelectAllOnFocus(DependencyObject obj)
	{
		return (bool)obj.GetValue(SelectAllOnFocusProperty);
	}
	public static void SetSelectAllOnFocus(DependencyObject obj, bool value)
	{
		obj.SetValue(SelectAllOnFocusProperty, value);
	}
	public static readonly DependencyProperty SelectAllOnFocusProperty =
		DependencyProperty.RegisterAttached(
			"SelectAllOnFocus",
			typeof(bool),
			typeof(TextBoxProperty),
			new PropertyMetadata(false, OnSelectAllOnFocusPropertyChanged));

	private static void OnSelectAllOnFocusPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is not TextBox textBox)
			return;

		if ((bool)e.NewValue)
		{
			textBox.PreviewMouseDown += OnPreviewMouseDown;
			textBox.GotFocus += OnGotFocus;
		}
		else
		{
			textBox.PreviewMouseDown -= OnPreviewMouseDown;
			textBox.GotFocus -= OnGotFocus;
		}

		static void OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
		{
			var textBox = (TextBox)sender;
			if (textBox.IsKeyboardFocusWithin)
				return;

			// Trigger GotFocus event.
			textBox.Focus();
			e.Handled = true;
		}

		static void OnGotFocus(object sender, RoutedEventArgs e)
		{
			var textBox = (TextBox)e.OriginalSource;
			textBox.SelectAll();
		}
	}
}