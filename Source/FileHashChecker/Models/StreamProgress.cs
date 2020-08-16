
namespace FileHashChecker.Models
{
	/// <summary>
	/// Progress data of Stream for IProgress
	/// </summary>
	internal class StreamProgress
	{
		public long Position { get; }
		public long Length { get; }

		public double Rate => (0L < Length) ? (double)Position / (double)Length : 0D;

		public StreamProgress(long position, long length)
		{
			this.Position = position;
			this.Length = length;
		}
	}
}