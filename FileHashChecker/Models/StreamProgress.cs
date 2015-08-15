
namespace FileHashChecker.Models
{
	internal class StreamProgress
	{
		public long Position { get; }
		public long Length { get; }

		public double Percentage => (0L < Length) ? (double)Position / (double)Length : 0D;

		public StreamProgress(long position, long length)
		{
			this.Position = position;
			this.Length = length;
		}
	}
}