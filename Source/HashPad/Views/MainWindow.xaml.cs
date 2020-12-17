using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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

using HashPad.ViewModels;
using HashPad.Views.Converters;

namespace HashPad.Views
{
	public partial class MainWindow : Window
	{
		#region Property

		public bool IsMenuOpen
		{
			get { return (bool)GetValue(IsMenuOpenProperty); }
			set { SetValue(IsMenuOpenProperty, value); }
		}
		public static readonly DependencyProperty IsMenuOpenProperty =
			DependencyProperty.Register(
				"IsMenuOpen",
				typeof(bool),
				typeof(MainWindow),
				new PropertyMetadata(false));

		#endregion

		private readonly MainWindowViewModel _mainWindowViewModel;

		public MainWindow()
		{
#if DEBUG
			SetCulture("en");
#endif
			InitializeComponent();

			_mainWindowViewModel = (MainWindowViewModel)this.DataContext;

			this.SetBinding(
				AllowDropProperty,
				new Binding(nameof(MainWindowViewModel.IsReading))
				{
					Source = _mainWindowViewModel,
					Mode = BindingMode.OneWay,
					Converter = new BooleanInverseConverter()
				});

			this.Loaded += OnLoaded;
			this.SizeChanged += OnSizeChanged;
			this.Drop += OnDrop;
		}

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			this.TitleBar.MouseDown += (_, e) =>
			{
				e.Handled = true;

				if (IsMenuOpen)
					IsMenuOpen = false;
				else if (Mouse.LeftButton == MouseButtonState.Pressed)
					this.DragMove();
			};
			this.MenuPain.MouseDown += (_, e) => { e.Handled = true; };
			this.FilePathBox.MouseDown += (_, e) => { IsMenuOpen = false; };
			this.ExpectedValueBox.MouseDown += (_, e) => { IsMenuOpen = false; };
			this.MouseDown += (_, e) => { IsMenuOpen = false; };
		}

		protected override void OnStateChanged(EventArgs e)
		{
			// Prevent Aero Snap.
			if (WindowState == WindowState.Maximized)
				WindowState = WindowState.Normal;

			base.OnStateChanged(e);
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			base.OnClosing(e);

			_mainWindowViewModel.Close();

			WindowHelper.SaveWindowLocation(this);
		}

		private async void OnLoaded(object sender, RoutedEventArgs e)
		{
			var args = Environment.GetCommandLineArgs().Skip(1).ToArray(); // The first element is this executable file path.

			WindowHelper.SetWindowLocation(this, args.Any());

			await _mainWindowViewModel.CheckFileAsync(args);
		}

		private void OnSizeChanged(object sender, SizeChangedEventArgs e)
		{
			if (e.HeightChanged && (Mouse.LeftButton != MouseButtonState.Pressed)) // Not being dragged
				WindowHelper.EnsureWindowLocation(this, e.NewSize);
		}

		private async void OnDrop(object sender, DragEventArgs e)
		{
			var paths = ((DataObject)e.Data).GetFileDropList().Cast<string>();

			await _mainWindowViewModel.CheckFileAsync(paths);
		}

		private void Menu_Click(object sender, RoutedEventArgs e)
		{
			IsMenuOpen = !IsMenuOpen;
		}

		private void Minimize_Click(object sender, RoutedEventArgs e)
		{
			this.WindowState = WindowState.Minimized;
		}

		private void Close_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}

		private async void Browse_Click(object sender, RoutedEventArgs e)
		{
			await _mainWindowViewModel.SelectFileAsync();
		}

		private void Read_Click(object sender, RoutedEventArgs e)
		{
			_mainWindowViewModel.ReadClipboard();
		}

		private async void Compute_Click(object sender, RoutedEventArgs e)
		{
			await _mainWindowViewModel.ComputeHashValuesAsync();
		}

		private void Stop_Click(object sender, RoutedEventArgs e)
		{
			_mainWindowViewModel.Cancel();
		}

		private void Site_MouseDown(object sender, MouseButtonEventArgs e) => OpenUrl(Properties.Resources.SiteUrl);
		private void License_MouseDown(object sender, MouseButtonEventArgs e) => OpenUrl(Properties.Resources.LicenseUrl);

		private static void OpenUrl(string url) => Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });

		private void SetCulture(string cultureName)
		{
			var culture = System.Globalization.CultureInfo.GetCultures(System.Globalization.CultureTypes.AllCultures)
				.FirstOrDefault(x => x.Name == cultureName);
			if (culture is null)
				return;

			System.Threading.Thread.CurrentThread.CurrentCulture = culture;
			System.Threading.Thread.CurrentThread.CurrentUICulture = culture;
		}
	}
}