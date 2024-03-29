﻿using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

using HashPad.ViewModels;
using HashPad.Views.Converters;

namespace HashPad.Views;

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
			new PropertyMetadata(
				false,
				(d, e) =>
				{
					if ((bool)e.NewValue)
						((MainWindow)d)._mainWindowViewModel.Update();
				}));

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

		WindowPainter.SetWindowCorners(this, CornerPreference.Round);

		if (WindowPainter.SetMicaBackground(this))
		{
			this.Resources["App.BackgroundColor"] = this.Resources["App.BackgroundColor.Mica"];
		}

		this.TitleBar.MouseDown += (_, e) =>
		{
			e.Handled = true;

			if (IsMenuOpen)
				IsMenuOpen = false;
			else if (Mouse.LeftButton == MouseButtonState.Pressed)
			{
				// Prevent Aero Snap.
				this.ResizeMode = ResizeMode.NoResize;

				this.DragMove();
			}
		};
		this.TitleBar.MouseUp += (_, _) =>
		{
			this.ResizeMode = ResizeMode.CanResize;
		};
		// Handle MouseLeave event because MouseUp event may not fire.
		this.TitleBar.MouseLeave += (_, _) =>
		{
			this.ResizeMode = ResizeMode.CanResize;
		};

		this.MenuPain.MouseDown += (_, e) => { e.Handled = true; };
		this.MouseDown += (_, e) => { IsMenuOpen = false; };
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

		await _mainWindowViewModel.CheckCommand.ExecuteAsync(args);
	}

	private void OnSizeChanged(object sender, SizeChangedEventArgs e)
	{
		if (e.HeightChanged && (Mouse.LeftButton != MouseButtonState.Pressed)) // Not being dragged
			WindowHelper.EnsureWindowLocation(this, e.NewSize);
	}

	private async void OnDrop(object sender, DragEventArgs e)
	{
		var paths = ((DataObject)e.Data).GetFileDropList().Cast<string>();

		await _mainWindowViewModel.CheckCommand.ExecuteAsync(paths);
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