
namespace FileHashChecker.Models
{
    internal class StreamProgress
    {
        public long Position { get; private set; }
        public long Length { get; private set; }

        public double Percentage
        {
            get { return (Length != 0L) ? (double)Position / (double)Length : 0D; }
        }

        public StreamProgress(long position, long length)
        {
            this.Position = position;
            this.Length = length;
        }
    }
}