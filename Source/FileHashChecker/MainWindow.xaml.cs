using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using FileHashChecker.Converters;
using FileHashChecker.ViewModels;

namespace FileHashChecker
{
	public partial class MainWindow : Window
	{
		private readonly MainWindowViewModel _mainWindowViewModel;

		public MainWindow()
		{
			InitializeComponent();

			_mainWindowViewModel = this.DataContext as MainWindowViewModel;
			if (_mainWindowViewModel != null)
			{
				this.Loaded += OnLoaded;
				this.Drop += OnDrop;

				this.SetBinding(
					AllowDropProperty,
					new Binding(nameof(MainWindowViewModel.IsReading))
					{
						Source = _mainWindowViewModel,
						Mode = BindingMode.OneWay,
						Converter = new BooleanInverseConverter()
					});
			}
		}

		private async void OnLoaded(object sender, RoutedEventArgs e)
		{
			var args = Environment.GetCommandLineArgs().Skip(1); // The first element is this executable file path.

			await _mainWindowViewModel.CheckFileAsync(args);
		}

		private async void OnDrop(object sender, DragEventArgs e)
		{
			var paths = ((DataObject)e.Data).GetFileDropList().Cast<string>();

			await _mainWindowViewModel.CheckFileAsync(paths);
		}
	}
}