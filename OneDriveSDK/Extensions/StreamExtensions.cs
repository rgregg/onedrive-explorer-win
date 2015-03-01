using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace OneDrive.Extensions
{
    /// <summary>
    /// Extends CopyToAsync to support cancel and progress reporting.
    /// </summary>
    public static class StreamExtensions
    {
        public static async Task CopyToWithProgressAsync(this Stream source, Stream output, Action<long> reportBytesTrasnfered = null, int bufferSize = 64 * 1024)
        {
            await CopyToWithProgressAsync(source, output, CancellationToken.None, reportBytesTrasnfered, bufferSize);
        }
        public static async Task CopyToWithProgressAsync(this Stream source, Stream output, CancellationToken cancelToken, Action<long> reportBytesTrasnfered = null, int bufferSize = 64 * 1024)
        {
            byte[] buffer = new byte[bufferSize];

            long bytesTransfered = 0;
            int bytesRead;
            while ((bytesRead = await source.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                if (cancelToken.IsCancellationRequested)
                {
                    cancelToken.ThrowIfCancellationRequested();
                }

                await output.WriteAsync(buffer, 0, bytesRead);
                bytesTransfered += bytesRead;

                if (null != reportBytesTrasnfered)
                {
                    reportBytesTrasnfered(bytesTransfered);
                }
            }
        }

        public static async Task WriteWithProgressAsync(this Stream source, byte[] sourceBuffer, int offset, int length, CancellationToken cancelToken, Action<long> reportBytesTransfered = null, int bufferSize = 64 * 1024)
        {
            long bytesTransfered = 0;

            for (int index = offset; index < (offset + length); index += bufferSize)
            {
                cancelToken.ThrowIfCancellationRequested();

                int lengthToWrite = Math.Min(bufferSize, (offset + length) - index);
                await source.WriteAsync(sourceBuffer, index, lengthToWrite);
                bytesTransfered += lengthToWrite;

                if (reportBytesTransfered != null)
                    reportBytesTransfered(bytesTransfered);
            }
        }

        public static bool TryGetLength(this Stream source, out long length)
        {
            try 
            {
                length = source.Length;
                return true;
            }
            catch (Exception ex)
            {
                length = 0;
                return false;
            }
        }
    }
}
