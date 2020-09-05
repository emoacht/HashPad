using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace FileHashChecker.Models
{
	internal class HashChecker
	{
		public static async Task<string> ComputeHashAsync(string filePath, HashType type, CancellationToken cancellationToken)
		{
			using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
				return await ComputeHashAsync(fs, type, null, cancellationToken);
		}

		public static async Task<string> ComputeHashAsync(Stream stream, HashType type, IProgress<StreamProgress> progress, CancellationToken cancellationToken)
		{
			using (var algorithm = type.GetAlgorithm())
				return await ComputeHashAsync(stream, algorithm, progress, cancellationToken);
		}

		#region Base

		private static readonly TimeSpan _progressInterval = TimeSpan.FromMilliseconds(100);
		private const uint COR_E_OBJECTDISPOSED = 0x80131622;

		private static async Task<string> ComputeHashAsync(Stream stream, HashAlgorithm algorithm, IProgress<StreamProgress> progress, CancellationToken cancellationToken)
		{
			if (0 < stream.Position)
				stream.Seek(0, SeekOrigin.Begin);

			var computeTask = Task.Run(() => ComputeHash(stream, algorithm));

			var progressTask = Task.Run(async () =>
			{
				while (stream.Position < stream.Length)
				{
					try
					{
						await Task.Delay(_progressInterval, cancellationToken);
					}
					catch (TaskCanceledException)
					{
						// Close the stream to stop compute task.
						stream.Close();
					}

					progress?.Report(new StreamProgress(position: stream.Position, length: stream.Length));
				}
			}, cancellationToken);

			try
			{
				// Wait for both tasks to complete.
				// If compute task completes, progress task will stop when it finds completion of compute
				// task by the position of stream.
				// If progress task is canceled, a TaskCanceledException will be thrown and it will trigger
				// closure of stream so as to make compute task stop with an ObjectDisposedException.
				await Task.WhenAll(computeTask, progressTask);
			}
			catch (ObjectDisposedException ode) when ((uint)ode.HResult == COR_E_OBJECTDISPOSED)
			{
				throw new OperationCanceledException("Computing hash value is cancelled.", ode, cancellationToken);
			}

			return await computeTask;
		}

		private static string ComputeHash(Stream stream, HashAlgorithm algorithm)
		{
			var hashBytes = algorithm.ComputeHash(stream);
			var hashString = BitConverter.ToString(hashBytes).Replace("-", "");

			return hashString;
		}

		#endregion
	}
}