using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using Microsoft.Win32;

using FileHashChecker.Models;

namespace FileHashChecker.ViewModels
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
						IsExpectedValueLower = StringHelper.IsLower(value);
						instance.SetEnabled(value.Length);
						instance.UpdateHashValues();
						instance.CompareHashValues(value);
					}));

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

		public bool IsSendToEnabled
		{
			get { return (bool)GetValue(IsSendToEnabledProperty); }
			set { SetValue(IsSendToEnabledProperty, value); }
		}
		public static readonly DependencyProperty IsSendToEnabledProperty =
			DependencyProperty.Register(
				"IsSendToEnabled",
				typeof(bool),
				typeof(MainWindowViewModel),
				new PropertyMetadata(
					ShortcutHelper.Exists(),
					(_, e) =>
					{
						if ((bool)e.NewValue)
							ShortcutHelper.Create();
						else
							ShortcutHelper.Remove();
					}));

		#endregion

		public IReadOnlyCollection<HashViewModel> Hashes { get; }

		public MainWindowViewModel() : this(HashType.Sha1, HashType.Sha256, HashType.Sha384, HashType.Sha512, HashType.Md5)
		{ }

		public MainWindowViewModel(params HashType[] types)
		{
			if (!types.Any())
				throw new ArgumentNullException(nameof(types));

			Hashes = types.Select(x => new HashViewModel(x)).ToArray();
			SetEnabled();
		}

		private void SetEnabled(int expectedValueLength = 0)
		{
			if (expectedValueLength == 0)
			{
				if (Hashes.All(x => !x.IsEnabled))
					Hashes.First().IsEnabled = true;
			}
			else if (HashTypeHelper.TryGetHashType(expectedValueLength, out HashType type))
			{
				foreach (var h in Hashes)
					h.IsEnabled = h.HashType == type;
			}
		}

		public async Task CheckFileAsync(IEnumerable<string> filePaths)
		{
			var filePath = filePaths?.FirstOrDefault(x => File.Exists(x));
			if (filePath is null)
				return;

			SourceFilePath = filePath;
			await ComputeHashValuesAsync();
		}

		public async Task SelectFileAsync()
		{
			var ofd = new OpenFileDialog { InitialDirectory = Path.GetDirectoryName(SourceFilePath) };
			if (!(ofd.ShowDialog(Application.Current.MainWindow) == true))
				return;

			SourceFilePath = ofd.FileName;
			await ComputeHashValuesAsync();
		}

		public void ReadClipboard()
		{
			if (ClipboardHelper.TryReadHexText(out string text))
				ExpectedValue = text;
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

			if (string.IsNullOrEmpty(ExpectedValue))
				ReadClipboard();

			ClearHashValues();

			try
			{
				_computeTokenSource = new CancellationTokenSource();
				_canCancel = true;

				using (var fs = new FileStream(SourceFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
				{
					foreach (var h in Hashes.Where(x => x.IsEnabled))
					{
						SetBindings(h); // Bindings will be overwritten and so only the last set ones remain.

						await h.ComputeAsync(fs, _computeTokenSource.Token);
						h.Compare(ExpectedValue);
					}
				}
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