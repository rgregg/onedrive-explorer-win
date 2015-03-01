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
