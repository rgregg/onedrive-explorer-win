using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneDrive
{
    public class ServiceCommand
    {
        public Uri ServiceUrl { get; set; }
        public string HttpVerb { get; set; }
        public Type ResponseType { get; set; }

        public Dictionary<string, string> HttpHeaders { get; private set; }

        public string ContentType { get; set; }
        public string Body { get; set; }

        public ServiceCommand()
        {
            HttpVerb = "GET";
            HttpHeaders = new Dictionary<string, string>();
        }


        public string RawHttpRequest()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("{0} {1} HTTP/1.1", HttpVerb, ServiceUrl.AbsoluteUri);
            
            if (null != ContentType)
            {
                sb.AppendLine("Content-Type: {0}", ContentType);
            }

            foreach(var header in HttpHeaders)
            {
                sb.AppendLine("{0}: {1}", header.Key, header.Value);
            }

            if (null != Body)
            {
                long contentLength = ApiConstants.ServiceTextEncoding.GetByteCount(Body);
                sb.AppendLine("Content-Length: {0}", contentLength);
                sb.AppendLine();
                sb.Append(Body);
            }

            return sb.ToString();
        }

    }

    public class ServiceResponse
    {
        private ServiceResponse()
        {

        }

        public int StatusCode { get; private set; }
        public string StatusMessage { get; private set; }
        public IReadOnlyDictionary<string, string> HttpHeaders { get; private set; }

        public string ContentType { get; private set; }
        public string Body { get; private set; }

        public static ServiceResponse FromRawHttpResponse(string value)
        {
            throw new NotImplementedException();
        }

        public T ToDataModel<T>()
        {
            throw new NotImplementedException();
        }
    }
}
