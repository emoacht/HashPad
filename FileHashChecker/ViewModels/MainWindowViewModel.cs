using System;
using System.Collections.Generic;
using System.IO;
using System.Media;
using System.Threading;
using System.Threading.Tasks;

using FileHashChecker.Common;
using FileHashChecker.Models;

namespace FileHashChecker.ViewModels
{
	public class MainWindowViewModel : NotificationObject
	{
		public MainWindowViewModel()
		{
		}

		#region Property

		public string SourceFilePath
		{
			get { return _sourceFilePath; }
			set
			{
				_sourceFilePath = value;
				RaisePropertyChanged();
			}
		}
		private string _sourceFilePath;

		public string CompareToTarget
		{
			get { return _compareToTarget; }
			set
			{
				_compareToTarget = value;
				RaisePropertyChanged();
				CompareHash();
			}
		}
		private string _compareToTarget;

		public bool IsReading => Sha1IsReading || Sha256IsReading || Md5IsReading;

		public double CombinedProgress
		{
			get
			{
				if (Sha1IsReading)
					return Sha1Progress;

				if (Sha256IsReading)
					return Sha256Progress;

				if (Md5IsReading)
					return Md5Progress;

				return 0D;
			}
		}

		#region SHA1

		public string Sha1Hash
		{
			get { return _sha1Hash; }
			set
			{
				_sha1Hash = value;
				RaisePropertyChanged();
				CompareHash();
			}
		}
		private string _sha1Hash;

		public bool Sha1IsEnabled
		{
			get { return _sha1IsEnabled; }
			set
			{
				_sha1IsEnabled = value;
				RaisePropertyChanged();
			}
		}
		private bool _sha1IsEnabled = true;

		public bool Sha1IsReading
		{
			get { return _sha1IsReading; }
			set
			{
				_sha1IsReading = value;
				RaisePropertyChanged();
				RaisePropertyChanged(nameof(IsReading));
			}
		}
		private bool _sha1IsReading;

		public double Sha1Progress
		{
			get { return _sha1Progress; }
			set
			{
				_sha1Progress = value;
				RaisePropertyChanged();
				RaisePropertyChanged(nameof(CombinedProgress));
			}
		}
		private double _sha1Progress;

		public bool Sha1HasMatch
		{
			get { return _sha1HasMatch; }
			set
			{
				_sha1HasMatch = value;
				RaisePropertyChanged();
			}
		}
		private bool _sha1HasMatch;

		#endregion

		#region SHA256

		public string Sha256Hash
		{
			get { return _sha256Hash; }
			set
			{
				_sha256Hash = value;
				RaisePropertyChanged();
				CompareHash();
			}
		}
		private string _sha256Hash;

		public bool Sha256IsEnabled
		{
			get { return _sha256IsEnabled; }
			set
			{
				_sha256IsEnabled = value;
				RaisePropertyChanged();
			}
		}
		private bool _sha256IsEnabled;

		public bool Sha256IsReading
		{
			get { return _sha256IsReading; }
			set
			{
				_sha256IsReading = value;
				RaisePropertyChanged();
				RaisePropertyChanged(nameof(IsReading));
			}
		}
		private bool _sha256IsReading;

		public double Sha256Progress
		{
			get { return _sha256Progress; }
			set
			{
				_sha256Progress = value;
				RaisePropertyChanged();
				RaisePropertyChanged(nameof(CombinedProgress));
			}
		}
		private double _sha256Progress;

		public bool Sha256HasMatch
		{
			get { return _sha256HasMatch; }
			set
			{
				_sha256HasMatch = value;
				RaisePropertyChanged();
			}
		}
		private bool _sha256HasMatch;

		#endregion

		#region MD5

		public string Md5Hash
		{
			get { return _md5Hash; }
			set
			{
				_md5Hash = value;
				RaisePropertyChanged();
				CompareHash();
			}
		}
		private string _md5Hash;

		public bool Md5IsEnabled
		{
			get { return _md5IsEnabled; }
			set
			{
				_md5IsEnabled = value;
				RaisePropertyChanged();
			}
		}
		private bool _md5IsEnabled = false;

		public bool Md5IsReading
		{
			get { return _md5IsReading; }
			set
			{
				_md5IsReading = value;
				RaisePropertyChanged();
				RaisePropertyChanged(nameof(IsReading));
			}
		}
		private bool _md5IsReading;

		public double Md5Progress
		{
			get { return _md5Progress; }
			set
			{
				_md5Progress = value;
				RaisePropertyChanged();
				RaisePropertyChanged(nameof(CombinedProgress));
			}
		}
		private double _md5Progress;

		public bool Md5HasMatch
		{
			get { return _md5HasMatch; }
			set
			{
				_md5HasMatch = value;
				RaisePropertyChanged();
			}
		}
		private bool _md5HasMatch;

		#endregion

		#endregion

		public async Task CheckFileAsync(IEnumerable<string> filePaths)
		{
			if (filePaths == null)
				return;

			foreach (var filePath in filePaths)
			{
				if (!File.Exists(filePath))
					continue;

				SourceFilePath = filePath;
				await GetHashAsync(filePath);
				return;
			}
		}

		public async Task GetHashAsync(string filePath)
		{
			if (string.IsNullOrWhiteSpace(filePath))
				return;

			Md5Hash = Sha256Hash = Sha1Hash = string.Empty;
			Md5Progress = Sha256Progress = Sha1Progress = 0;

			try
			{
				using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
				{
					if (Sha1IsEnabled)
					{
						Sha1IsReading = true;
						try
						{
							var progress = new Progress<StreamProgress>(x => Sha1Progress = x.Percentage);
							Sha1Hash = await HashChecker.GetHashAsync(fs, HashType.Sha1, progress, CancellationToken.None);
							SystemSounds.Asterisk.Play();
						}
						finally
						{
							Sha1IsReading = false;
						}
					}

					if (Sha256IsEnabled)
					{
						Sha256IsReading = true;
						try
						{
							var progress = new Progress<StreamProgress>(x => Sha256Progress = x.Percentage);
							Sha256Hash = await HashChecker.GetHashAsync(fs, HashType.Sha256, progress, CancellationToken.None);
							SystemSounds.Asterisk.Play();
						}
						finally
						{
							Sha256IsReading = false;
						}
					}

					if (Md5IsEnabled)
					{
						Md5IsReading = true;
						try
						{
							var progress = new Progress<StreamProgress>(x => Md5Progress = x.Percentage);
							Md5Hash = await HashChecker.GetHashAsync(fs, HashType.Md5, progress, CancellationToken.None);
							SystemSounds.Asterisk.Play();
						}
						finally
						{
							Md5IsReading = false;
						}
					}
				}
			}
			catch (Exception ex)
			{
				Md5Hash = Sha256Hash = Sha1Hash = ex.Message;
			}
		}

		private void CompareHash()
		{
			Sha1HasMatch = !string.IsNullOrEmpty(Sha1Hash) && Sha1Hash.Equals(CompareToTarget, StringComparison.OrdinalIgnoreCase);
			Sha256HasMatch = !string.IsNullOrEmpty(Sha256Hash) && Sha256Hash.Equals(CompareToTarget, StringComparison.OrdinalIgnoreCase);
			Md5HasMatch = !string.IsNullOrEmpty(Md5Hash) && Md5Hash.Equals(CompareToTarget, StringComparison.OrdinalIgnoreCase);
		}
	}
}