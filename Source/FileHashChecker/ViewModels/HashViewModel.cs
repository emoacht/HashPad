using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using FileHashChecker.Common;
using FileHashChecker.Models;

namespace FileHashChecker.ViewModels
{
	public class HashViewModel : NotificationObject
	{
		#region Property

		public HashType HashType { get; }
		public string Name => $"{HashType.ToString().ToUpper()} Hash";

		public string Hash
		{
			get { return _hash; }
			set { SetProperty(ref _hash, value); }
		}
		private string _hash;

		public bool IsEnabled
		{
			get { return _isEnabled; }
			set { SetProperty(ref _isEnabled, value); }
		}
		private bool _isEnabled;

		public bool IsReading
		{
			get { return _isReading; }
			set { SetProperty(ref _isReading, value); }
		}
		private bool _isReading;

		public double ProgressRate
		{
			get { return _progressRate; }
			set { SetProperty(ref _progressRate, value); }
		}
		private double _progressRate;

		public bool HasMatch
		{
			get { return _hasMatch; }
			set { SetProperty(ref _hasMatch, value); }
		}
		private bool _hasMatch;

		#endregion

		public HashViewModel(HashType hashType)
		{
			this.HashType = hashType;
		}

		public async Task GetHashAsync(Stream stream)
		{
			if (stream == null)
				throw new ArgumentNullException(nameof(stream));

			if (!IsEnabled)
				return;

			Hash = string.Empty;
			HasMatch = false;
			ProgressRate = 0;

			IsReading = true;
			try
			{
				var progress = new Progress<StreamProgress>(x => ProgressRate = x.Rate);
				Hash = await HashChecker.GetHashAsync(stream, HashType, progress, CancellationToken.None);
				SystemSounds.Asterisk.Play();
			}
			catch (Exception ex)
			{
				Hash = ex.Message;
			}
			finally
			{
				IsReading = false;
			}
		}

		public void CompareHash(string compareToTarget)
		{
			HasMatch = !string.IsNullOrEmpty(Hash) && Hash.Equals(compareToTarget, StringComparison.OrdinalIgnoreCase);
		}

		public void ClearHash()
		{
			Hash = string.Empty;
			HasMatch = false;
		}
	}
}