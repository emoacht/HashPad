using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using HashPad.Common;
using HashPad.Models;

namespace HashPad.ViewModels
{
	public class HashViewModel : NotificationObject
	{
		#region Property

		public HashType HashType { get; }
		public string Name => HashType.ToString().ToUpper();

		public string HashValue
		{
			get => MainWindowViewModel.IsExpectedValueLower ? _hashValue?.ToLower() : _hashValue;
			set => SetProperty(ref _hashValue, value);
		}
		private string _hashValue;

		public bool IsTarget
		{
			get => _isTarget;
			set => SetProperty(ref _isTarget, value);
		}
		private bool _isTarget;

		public bool IsReading
		{
			get => _isReading;
			private set => SetProperty(ref _isReading, value);
		}
		private bool _isReading;

		public double ProgressRate
		{
			get => _progressRate;
			private set => SetProperty(ref _progressRate, value);
		}
		private double _progressRate;

		internal bool HasRead => !string.IsNullOrEmpty(HashValue);

		public bool HasMatch
		{
			get => _hasMatch;
			set => SetProperty(ref _hasMatch, value);
		}
		private bool _hasMatch;

		#endregion

		public HashViewModel(HashType hashType) => this.HashType = hashType;

		internal async Task ComputeAsync(Stream stream, CancellationToken cancellationToken)
		{
			if (stream is null)
				throw new ArgumentNullException(nameof(stream));

			if (!IsTarget)
				return;

			HashValue = string.Empty;
			HasMatch = false;
			ProgressRate = 0;

			try
			{
				IsReading = true;

				var progress = new Progress<StreamProgress>(x => ProgressRate = x.Rate);
				HashValue = await HashChecker.ComputeHashAsync(stream, HashType, progress, cancellationToken);
			}
			finally
			{
				IsReading = false;
			}
		}

		internal void Compare(string expectedValue)
		{
			HasMatch = !string.IsNullOrEmpty(HashValue) && HashValue.Equals(expectedValue, StringComparison.OrdinalIgnoreCase);
		}

		internal void Update()
		{
			RaisePropertyChanged(nameof(HashValue));
		}

		internal void Clear()
		{
			HashValue = string.Empty;
			HasMatch = false;
		}
	}
}