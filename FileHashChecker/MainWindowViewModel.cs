using FileHashChecker.Common;
using FileHashChecker.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Media;
using System.Threading;
using System.Threading.Tasks;

namespace FileHashChecker
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

        public bool IsReady
        {
            get { return !Sha1IsReading && !Md5IsReading; }
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
                RaisePropertyChanged("IsReady");
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
                RaisePropertyChanged("IsReady");
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
            if (String.IsNullOrEmpty(filePath))
                return;

            Md5Hash = Sha1Hash = String.Empty;
            Md5Progress = Sha1Progress = 0;

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
                            Sha1Hash = await HashChecker.GetSha1HashAsync(fs, progress, CancellationToken.None);
                            SystemSounds.Asterisk.Play();
                        }
                        finally
                        {
                            Sha1IsReading = false;
                        }
                    }

                    if (Md5IsEnabled)
                    {
                        Md5IsReading = true;
                        try
                        {
                            var progress = new Progress<StreamProgress>(x => Md5Progress = x.Percentage);
                            Md5Hash = await HashChecker.GetMd5HashAsync(fs, progress, CancellationToken.None);
                            SystemSounds.Exclamation.Play();
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
                Md5Hash = Sha1Hash = ex.Message;
            }
        }

        private void CompareHash()
        {
            Sha1HasMatch = !String.IsNullOrEmpty(Sha1Hash) && Sha1Hash.Equals(CompareToTarget, StringComparison.OrdinalIgnoreCase);
            Md5HasMatch = !String.IsNullOrEmpty(Md5Hash) && Md5Hash.Equals(CompareToTarget, StringComparison.OrdinalIgnoreCase);
        }
    }
}