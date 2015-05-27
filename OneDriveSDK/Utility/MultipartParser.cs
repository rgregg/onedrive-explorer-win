using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneDrive
{
    internal static class MultipartParser
    {
        /// <summary>
        /// Read a stream of multipart data and generate a MultipartBuilder from the contents
        /// </summary>
        /// <param name="inputStream"></param>
        /// <returns></returns>
        public static async Task<MultipartBuilder> ParseStreamAsync(string contentType, Stream inputStream)
        {
            MultipartBuilder builder = new MultipartBuilder();

            //multipart/mixed; boundary=batchresponse_02823639-2b8c-4295-bbec-804ac7a3bbca
            string[] contentTypeComponents = contentType.SplitAndTrim(';');
            builder.Format = contentTypeComponents[0];
            foreach (var part in contentTypeComponents)
            {
                const string boundaryHeader = "boundary=";
                if (part.StartsWith(boundaryHeader))
                {
                    builder.Boundary = part.Substring(boundaryHeader.Length);
                }
            }

            StreamReader reader = new StreamReader(inputStream, ApiConstants.ServiceTextEncoding, false, ApiConstants.StreamWriterBufferSize, true);
            StringBuilder currentPartContent = new StringBuilder();
            MultipartContent currentPart = null;
            bool readingHeaders = true;
            while (!reader.EndOfStream)
            {
                string line = await reader.ReadLineAsync();
                if (line.StartsWith("--" + builder.Boundary))
                {
                    // End any existing part
                    if (currentPart != null)
                    {
                        currentPart.TextContent = currentPartContent.ToString();
                        builder.Parts.Add(currentPart);
                        currentPart = new MultipartContent();
                        currentPartContent = new StringBuilder();
                        readingHeaders = true;
                    }
                }
                else if (readingHeaders)
                {
                    // Parse the current line for headers
                    string name, value;
                    if (line.Length > 0)
                    {
                        name = line.GetToNextSeparator(": ", out value);
                        currentPart.AddHeader(name, value);
                    }
                    else
                    {
                        readingHeaders = false;
                    }
                }
                else 
                {
                    currentPartContent.AppendLine(line);
                }
            }

            return builder;
        }

        





    }
}

