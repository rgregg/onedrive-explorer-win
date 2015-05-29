namespace OneDrive.Http
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Threading.Tasks;
    using System.IO;

    internal class HttpParser
    {
          public static IHttpResponse ParseHttpResponse(string responseString)
        {
            StringReader reader = new StringReader(responseString);
            string line;
            ParserMode mode = ParserMode.FirstLine;

            StaticHttpResponse response = new StaticHttpResponse();
            Dictionary<string, string> headers = new Dictionary<string, string>();
            response.Headers = headers;

            while ((line = reader.ReadLine()) != null)
            {
                switch (mode)
                {
                    case ParserMode.FirstLine:
                        var components = line.Split(' ');
                        if (components.Length < 3) throw new ArgumentException("responseString does not contain a proper HTTP request first line.");

                        response.StatusCode = (HttpStatusCode)int.Parse(components[1]);
                        response.StatusDescription = components.ComponentsJoinedByString(" ", 2);

                        mode = ParserMode.Headers;
                        break;

                    case ParserMode.Headers:
                        if (string.IsNullOrEmpty(line))
                        {
                            mode = ParserMode.Body;
                            continue;
                        }

                        // Parse each header
                        int split = line.IndexOf(": ");
                        if (split < 1) throw new ArgumentException("requestString contains an invalid header definition");

                        var headerName = line.Substring(0, split);
                        var headerValue = line.Substring(split + 1);
                        headers[headerName] = headerValue;

                        break;

                    case ParserMode.Body:
                        response.SetResponseStream(line + Environment.NewLine + reader.ReadToEnd());
                        break;
                }
            }

            return response;
        }


        public class StaticHttpResponse : IHttpResponse
        {

            public Uri Uri
            {
                get;
                set;
            }

            public HttpStatusCode StatusCode
            {
                get;
                set;
            }

            public string StatusDescription
            {
                get;
                set;
            }

            public string ContentType
            {
                get;
                set;
            }

            public IReadOnlyDictionary<string, string> Headers
            {
                get;
                set;
            }

            private Stream _responseStream;

            public async Task<Stream> GetResponseStreamAsync()
            {
                return _responseStream;
            }

            public void SetResponseStream(string responseString)
            {
                _responseStream = new MemoryStream(ApiConstants.ServiceTextEncoding.GetBytes(responseString));
            }
        }


        private enum ParserMode
        {
            FirstLine,
            Headers,
            Body
        }
    }
}
