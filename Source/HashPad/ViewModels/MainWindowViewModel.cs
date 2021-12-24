using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using Microsoft.Win32;

using HashPad.Models;

namespace HashPad.ViewModels
{
	public class MainWindowViewModel : DependencyObject
	{
		#region Property

		public string SourceFilePath
		{
			get { return (string)GetValue(SourceFilePathProperty); }
			set { SetValue(SourceFilePathProperty, value); }
		}
		public static readonly DependencyProperty SourceFilePathProperty =
			DependencyProperty.Register(
				nameof(SourceFilePath),
				typeof(string),
				typeof(MainWindowViewModel),
				new PropertyMetadata(
					null,
					(d, _) => ((MainWindowViewModel)d).SetCanCompute()));

		public string ExpectedValue
		{
			get { return (string)GetValue(ExpectedValueProperty); }
			set { SetValue(ExpectedValueProperty, value); }
		}
		public static readonly DependencyProperty ExpectedValueProperty =
			DependencyProperty.Register(
				"ExpectedValue",
				typeof(string),
				typeof(MainWindowViewModel),
				new PropertyMetadata(
					null,
					(d, e) =>
					{
						var instance = (MainWindowViewModel)d;
						var value = (string)e.NewValue;

						IsExpectedValueLower = StringHelper.IsOverHalfLower(value);
						instance.SetEnabled(value);
						instance.UpdateHashValues();
						instance.CompareHashValues(value);
					},
					(_, baseValue) => ((string)baseValue).Trim()));

		public static bool IsExpectedValueLower { get; private set; }

		public bool CanCompute
		{
			get { return (bool)GetValue(CanComputeProperty); }
			set { SetValue(CanComputeProperty, value); }
		}
		public static readonly DependencyProperty CanComputeProperty =
			DependencyProperty.Register(
				"CanCompute",
				typeof(bool),
				typeof(MainWindowViewModel),
				new PropertyMetadata(false));

		private void SetCanCompute()
		{
			CanCompute = !string.IsNullOrEmpty(SourceFilePath) && !IsReading;
		}

		public bool IsReading
		{
			get { return (bool)GetValue(IsReadingProperty); }
			set { SetValue(IsReadingProperty, value); }
		}
		public static readonly DependencyProperty IsReadingProperty =
			DependencyProperty.Register(
				nameof(IsReading),
				typeof(bool),
				typeof(MainWindowViewModel),
				new PropertyMetadata(
					false,
					(d, _) => ((MainWindowViewModel)d).SetCanCompute(),
					(d, _) => ((MainWindowViewModel)d).Hashes.Any(x => x.IsReading)));

		public double ProgressRate
		{
			get { return (double)GetValue(ProgressRateProperty); }
			set { SetValue(ProgressRateProperty, value); }
		}
		public static readonly DependencyProperty ProgressRateProperty =
			DependencyProperty.Register(
				nameof(ProgressRate),
				typeof(double),
				typeof(MainWindowViewModel),
				new PropertyMetadata(
					0D,
					null,
					(d, _) => ((MainWindowViewModel)d).Hashes.FirstOrDefault(x => x.IsReading)?.ProgressRate ?? 0D));

		public bool IsSendToAdded
		{
			get { return (bool)GetValue(IsSendToAddedProperty); }
			set { SetValue(IsSendToAddedProperty, value); }
		}
		public static readonly DependencyProperty IsSendToAddedProperty =
			DependencyProperty.Register(
				"IsSendToAdded",
				typeof(bool),
				typeof(MainWindowViewModel),
				new PropertyMetadata(
					ShortcutHelper.Exists(),
					(_, e) =>
					{
						if ((bool)e.NewValue)
							ShortcutManager.Add();
						else
							ShortcutManager.Remove();
					}));

		internal void Update()
		{
			IsSendToAdded = ShortcutHelper.Exists();
		}

		#endregion

		public IReadOnlyCollection<HashViewModel> Hashes { get; }

		public Settings Settings { get; } = new Settings();

		public MainWindowViewModel() : this(HashType.Sha1, HashType.Sha256, HashType.Sha384, HashType.Sha512, HashType.Md5)
		{ }

		public MainWindowViewModel(params HashType[] types)
		{
			if (types is not { Length: > 0 })
				throw new ArgumentNullException(nameof(types));

			Hashes = types.Select(x => new HashViewModel(x)).ToArray();

			foreach (var h in Hashes)
			{
				h.PropertyChanged += (sender, e) =>
				{
					if (e.PropertyName == nameof(HashViewModel.IsTarget))
					{
						var h = (HashViewModel)sender;
						if (h.IsTarget)
							Settings.LastTargetHashType = h.HashType;
					}
				};
			}

			SetEnabled();
		}

		private void SetEnabled(in string expectedValue = null)
		{
			if (expectedValue is null)
			{
				if (Hashes.All(x => !x.IsTarget))
				{
					if (Settings.LastTargetHashType != default)
						Hashes.First(x => x.HashType == Settings.LastTargetHashType).IsTarget = true;
					else
						Hashes.First().IsTarget = true;
				}
			}
			else if (HashTypeHelper.TryGetHashType(expectedValue.Length, out HashType type))
			{
				foreach (var h in Hashes.Where(x => !(x.IsTarget && (x.IsReading || x.HasRead))))
					h.IsTarget = (h.HashType == type);
			}
		}

		public void Close()
		{
			if (!string.IsNullOrWhiteSpace(SourceFilePath))
				Settings.LastSourceFolderPath = Path.GetDirectoryName(SourceFilePath);

			Cancel();
		}

		public async Task CheckFileAsync(IEnumerable<string> filePaths)
		{
			var filePath = filePaths?.FirstOrDefault(x => File.Exists(x));
			if (filePath is null)
				return;

			SourceFilePath = Path.GetFullPath(filePath);

			if (Settings.ComputesAutomatically)
				await ComputeHashValuesAsync();
		}

		public async Task SelectFileAsync()
		{
			var initialFolder = !string.IsNullOrWhiteSpace(SourceFilePath)
				? Path.GetDirectoryName(SourceFilePath)
				: Settings.LastSourceFolderPath;

			var ofd = new OpenFileDialog { InitialDirectory = initialFolder };
			if (ofd.ShowDialog(Application.Current.MainWindow) is not true)
				return;

			SourceFilePath = ofd.FileName;

			if (Settings.ComputesAutomatically)
				await ComputeHashValuesAsync();
		}

		public void ReadClipboard()
		{
			if (ClipboardHelper.TryReadHexText(out string text) &&
				HashTypeHelper.TryGetHashType(text.Trim().Length, out _))
			{
				ExpectedValue = text;
			}
		}

		private CancellationTokenSource _computeTokenSource;
		private bool _canCancel = false;

		public void Cancel()
		{
			if (_canCancel)
				_computeTokenSource.Cancel();
		}

		public async Task ComputeHashValuesAsync()
		{
			if (!File.Exists(SourceFilePath))
				return;

			if (string.IsNullOrWhiteSpace(ExpectedValue) && Settings.ReadsAutomatically)
				ReadClipboard();

			ClearHashValues();

			try
			{
				_computeTokenSource = new CancellationTokenSource();
				_canCancel = true;

				using (var fs = new FileStream(SourceFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
				{
					while (true)
					{
						var h = Hashes.FirstOrDefault(x => x.IsTarget && !x.HasRead);
						if (h is null)
							break;

						SetBindings(h); // Bindings will be overwritten and so only the last set ones remain.

						await h.ComputeAsync(fs, _computeTokenSource.Token);
						h.Compare(ExpectedValue);
					}
				}

				SystemSounds.Asterisk.Play();
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Failed to compute hash values.\r\n{ex}");
			}
			finally
			{
				_canCancel = false;
				_computeTokenSource.Dispose();
				_computeTokenSource = null;
			}
		}

		#region Binding

		private void SetBindings(HashViewModel h)
		{
			BindingOperations.SetBinding(
				this,
				IsReadingProperty,
				new Binding(nameof(HashViewModel.IsReading))
				{
					Source = h,
					Mode = BindingMode.OneWay
				});

			BindingOperations.SetBinding(
				this,
				ProgressRateProperty,
				new Binding(nameof(HashViewModel.ProgressRate))
				{
					Source = h,
					Mode = BindingMode.OneWay
				});
		}

		private void ClearBindings()
		{
			BindingOperations.ClearBinding(
				this,
				IsReadingProperty);

			BindingOperations.ClearBinding(
				this,
				ProgressRateProperty);
		}

		#endregion

		private void UpdateHashValues()
		{
			foreach (var h in Hashes)
				h.Update();
		}

		private void ClearHashValues()
		{
			foreach (var h in Hashes)
				h.Clear();
		}

		private void CompareHashValues(string expectedValue)
		{
			Parallel.ForEach(Hashes, h => h.Compare(expectedValue));
		}
	}
}