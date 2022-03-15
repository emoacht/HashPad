using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;

using HashPad.Models;

namespace HashPad.ViewModels
{
	public class MainWindowViewModel : ObservableObject
	{
		#region Property

		public string SourceFilePath
		{
			get => _sourceFilePath;
			set
			{
				if (SetProperty(ref _sourceFilePath, value))
				{
					OnCanComputeChanged();
				}
			}
		}
		private string _sourceFilePath;

		public string ExpectedValue
		{
			get => _expectedValue;
			set
			{
				if (SetProperty(ref _expectedValue, value.Trim()))
				{
					IsExpectedValueLower = StringHelper.IsOverHalfLower(_expectedValue);
					SetEnabled(_expectedValue);
					UpdateHashValues();
					CompareHashValues(_expectedValue);
				}
			}
		}
		private string _expectedValue = null;

		public static bool IsExpectedValueLower { get; private set; }

		public bool IsReading
		{
			get => _isReading;
			set
			{
				if (SetProperty(ref _isReading, value))
				{
					OnCanComputeChanged();
				}
			}
		}
		private bool _isReading;

		public double ProgressRate
		{
			get => _progressRate;
			set => SetProperty(ref _progressRate, value);
		}
		private double _progressRate = 0D;

		public bool IsSendToAdded
		{
			get => _isSendToAdded;
			set
			{
				if (SetProperty(ref _isSendToAdded, value))
				{
					if (_isSendToAdded)
						ShortcutManager.Add();
					else
						ShortcutManager.Remove();
				}
			}
		}
		private bool _isSendToAdded = ShortcutHelper.Exists();

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
				h.PropertyChanged += OnHashPropertyChanged;

			SetEnabled();
		}

		private void OnHashPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case nameof(HashViewModel.IsTarget):
					var h = (HashViewModel)sender;
					if (h.IsTarget)
						Settings.LastTargetHashType = h.HashType;

					break;

				case nameof(HashViewModel.IsReading):
					IsReading = Hashes.Any(x => x.IsReading);
					break;

				case nameof(HashViewModel.ProgressRate):
					ProgressRate = Hashes.FirstOrDefault(x => x.IsReading)?.ProgressRate ?? 0D;
					break;
			}
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

			Stop();
		}

		#region Command

		public IAsyncRelayCommand<IEnumerable<string>> CheckCommand => _checkCommand ??= new((paths) => CheckFileAsync(paths));
		private AsyncRelayCommand<IEnumerable<string>> _checkCommand;

		public IAsyncRelayCommand BrowseCommand => _browseCommand ??= new(() => SelectFileAsync());
		private AsyncRelayCommand _browseCommand;

		public IRelayCommand ReadCommand => _readCommand ??= new(() => ReadClipboard());
		private RelayCommand _readCommand;

		public IAsyncRelayCommand ComputeCommand => _computeCommand ??= new(() => ComputeHashValuesAsync(), () => CanCompute);
		private AsyncRelayCommand _computeCommand;

		public IRelayCommand StopCommand => _stopCommand ??= new(() => Stop(), () => CanStop);
		private RelayCommand _stopCommand;

		#endregion

		private async Task CheckFileAsync(IEnumerable<string> filePaths)
		{
			var filePath = filePaths?.FirstOrDefault(x => File.Exists(x));
			if (filePath is null)
				return;

			SourceFilePath = Path.GetFullPath(filePath);

			if (Settings.ComputesAutomatically)
				await ComputeHashValuesAsync();
		}

		private async Task SelectFileAsync()
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

		private void ReadClipboard()
		{
			if (ClipboardHelper.TryReadHexText(out string text) &&
				HashTypeHelper.TryGetHashType(text.Trim().Length, out _))
			{
				ExpectedValue = text;
			}
		}

		private bool CanCompute => !string.IsNullOrEmpty(SourceFilePath) && !IsReading;

		private void OnCanComputeChanged() => ComputeCommand.NotifyCanExecuteChanged();

		private bool CanStop { get; set; }

		private void OnCanStopChanged() => StopCommand.NotifyCanExecuteChanged();

		private CancellationTokenSource _computeTokenSource;

		private void Stop()
		{
			if (CanStop && _computeTokenSource is { IsCancellationRequested: false })
				_computeTokenSource.Cancel();
		}

		private async Task ComputeHashValuesAsync()
		{
			if (!File.Exists(SourceFilePath))
				return;

			if (string.IsNullOrWhiteSpace(ExpectedValue) && Settings.ReadsAutomatically)
				ReadClipboard();

			ClearHashValues();

			try
			{
				_computeTokenSource = new CancellationTokenSource();
				CanStop = true;
				OnCanStopChanged();

				using (var fs = new FileStream(SourceFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
				{
					while (true)
					{
						var h = Hashes.FirstOrDefault(x => x.IsTarget && !x.HasRead);
						if (h is null)
							break;

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
				CanStop = false;
				OnCanStopChanged();
				_computeTokenSource.Dispose();
				_computeTokenSource = null;
			}
		}

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