using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;


namespace OneDrive
{
    internal class MultipartBuilder
    {
        public string Boundary { get; set; }

        public string Format { get; set; }

        public string ContentType { get { return string.Format("{1}; boundary=\"{0}\"", Boundary, Format); } }

        public List<MultipartContent> Parts { get; private set; }

        public MultipartBuilder()
        {
            Format = "multipart/related";
            Boundary = "A100x";
            Parts = new List<MultipartContent>();
        }

        public async Task WriteToStreamAsync(Stream outputStream)
        {
            using (StreamWriter writer = new StreamWriter(outputStream, ApiConstants.ServiceTextEncoding, ApiConstants.StreamWriterBufferSize, true))
            {

                foreach (MultipartContent content in Parts)
                {
                    await writer.WriteAsync("--");
                    await writer.WriteLineAsync(Boundary);

                    // Headers
                    await writer.WriteAsync(content.GetHeaderText());
                    await writer.FlushAsync();

                    // Body
                    if (content.StreamContent != null)
                    {
                        await content.StreamContent.CopyToAsync(outputStream);
                    }
                    else if (content.TextContent != null)
                    {
                        await writer.WriteAsync(content.TextContent);
                    }
                    await writer.WriteLineAsync();
                    await writer.WriteLineAsync();
                }

                await writer.WriteAsync("--");
                await writer.WriteAsync(Boundary);
                await writer.WriteLineAsync("--");

                await writer.FlushAsync();
            }
        }
    }

    internal class MultipartContent
    {
        public string ContentId { get; set; }
        
        public string ContentType { get; set; }

        public string ContentTransferEncoding { get; set; }

        public Dictionary<string, string> Headers { get; private set; }

        public string TextContent { get; set; }

        public Stream StreamContent { get; set; }

        public MultipartContent()
        {
            Headers = new Dictionary<string, string>();
            ContentTransferEncoding = "Binary";
        }

        public void AddHeader(string name, string value)
        {
            if (name.Equals("content-type", StringComparison.OrdinalIgnoreCase))
                ContentType = value;
            if (name.Equals("content-transfer-encoding", StringComparison.OrdinalIgnoreCase))
                ContentTransferEncoding = value;
            if (name.Equals("content-id", StringComparison.OrdinalIgnoreCase))
                ContentId = value;
            else
                Headers[name] = value;
        }

        public string GetHeaderText()
        {
            StringBuilder sb = new StringBuilder();
            if (!string.IsNullOrEmpty(ContentId))
            {
                sb.AppendLine("Content-ID: " + ContentId);
            }
            if (!string.IsNullOrEmpty(ContentType))
            {
                sb.AppendLine("Content-Type: " + ContentType);
            }
            if (!string.IsNullOrEmpty(ContentTransferEncoding))
            {
                sb.AppendLine("Content-Transfer-Encoding: " + ContentTransferEncoding);
            }
            foreach (var header in Headers)
            {
                sb.AppendLine(header.Key + ": " + header.Value);
            }
            sb.AppendLine();
            return sb.ToString();
        }
    }
}
