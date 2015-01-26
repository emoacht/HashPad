using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace FileHashChecker.Models
{
    internal class HashChecker
    {
        #region SHA1

        public static async Task<string> GetSha1HashAsync(string filePath, CancellationToken cancellationToken)
        {
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                return await GetSha1HashAsync(fs, null, cancellationToken);
            }
        }

        public static async Task<string> GetSha1HashAsync(Stream stream, IProgress<StreamProgress> progress, CancellationToken cancellationToken)
        {
            using (var algorithm = new SHA1CryptoServiceProvider())
            {
                return await GetHashAsync(stream, algorithm, progress, cancellationToken);
            }
        }

        #endregion


        #region MD5

        public static async Task<string> GetMd5HashAsync(string filePath, CancellationToken cancellationToken)
        {
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                return await GetMd5HashAsync(fs, null, cancellationToken);
            }
        }

        public static async Task<string> GetMd5HashAsync(Stream stream, IProgress<StreamProgress> progress, CancellationToken cancellationToken)
        {
            using (var algorithm = new MD5CryptoServiceProvider())
            {
                return await GetHashAsync(stream, algorithm, progress, cancellationToken);
            }
        }

        #endregion


        #region Base

        private static async Task<string> GetHashAsync(Stream stream, HashAlgorithm algorithm, IProgress<StreamProgress> progress, CancellationToken cancellationToken)
        {
            if (0 < stream.Position)
                stream.Seek(0, SeekOrigin.Begin);

            var getTask = Task.Run(() => GetHash(stream, algorithm));

            var monitorTask = Task.Run(async () =>
            {
                while (stream.Position < stream.Length)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(100), cancellationToken);

                    if (progress != null)
                        progress.Report(new StreamProgress(position: stream.Position, length: stream.Length));
                }
            });

            // Wait on both tasks to complete. If wait on only one task, the stream may be disposed before 
            // the other task will complete and so cause an ObjectDisposedException.
            await Task.WhenAll(getTask, monitorTask);

            return await getTask;
        }

        private static string GetHash(Stream stream, HashAlgorithm algorithm)
        {
            var hashBytes = algorithm.ComputeHash(stream);
            var hashString = BitConverter.ToString(hashBytes).Replace("-", "");

            return hashString;
        }

        #endregion
    }
}