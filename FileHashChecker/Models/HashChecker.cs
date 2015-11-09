using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace FileHashChecker.Models
{
	internal class HashChecker
	{
		public static async Task<string> GetHashAsync(string filePath, HashType type, CancellationToken cancellationToken)
		{
			using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
				return await GetHashAsync(fs, type, null, cancellationToken);
		}

		public static async Task<string> GetHashAsync(Stream stream, HashType type, IProgress<StreamProgress> progress, CancellationToken cancellationToken)
		{
			using (var algorithm = GetAlgorithm(type))
				return await GetHashAsync(stream, algorithm, progress, cancellationToken);
		}

		private static HashAlgorithm GetAlgorithm(HashType type)
		{
			switch (type)
			{
				case HashType.Sha1:
					return new SHA1CryptoServiceProvider();
				case HashType.Sha256:
					return new SHA256CryptoServiceProvider();
				case HashType.Sha512:
					return new SHA512CryptoServiceProvider();
				case HashType.Md5:
					return new MD5CryptoServiceProvider();
				default: // HashType.None
					throw new NotSupportedException();
			}
		}

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

					progress?.Report(new StreamProgress(position: stream.Position, length: stream.Length));
				}
			});

			// Wait for both tasks to complete. If wait for only one task, the stream may be disposed before 
			// the completion of the other task and so cause an ObjectDisposedException.
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