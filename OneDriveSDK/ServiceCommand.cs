using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace OneDrive
{
    public class ServiceCommand
    {
        public Uri ServiceUrl { get; set; }
        public string HttpVerb { get; set; }
        public Type ResponseDataModelType { get; set; }

        public Dictionary<string, string> HttpHeaders { get; private set; }

        public string ContentType { get; set; }
        public string Body { get; set; }

        public ODItemReference ItemReference { get; set; }

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
        public Http.IHttpResponse HttpResponse { get; set; }
        public Type ExpectedDataModelType { get; set; }

        public ServiceCommand OriginalCommand { get; set; }

        internal ServiceResponse(Http.IHttpResponse response, ServiceCommand command)
        {
            this.HttpResponse = response;
            this.ExpectedDataModelType = command.ResponseDataModelType;
            this.OriginalCommand = command;
        }

        public bool WasError
        {
            get { return HttpResponse.StatusCode >= HttpStatusCode.BadRequest; }
        }

        public async Task<ODError> GetError()
        {
            return await HttpResponse.ConvertToDataModel<ODError>();
        }

        public async Task<ODDataModel> GetDataModel()
        {
            ODDataModel model = await HttpResponse.ConvertToDataModel(ExpectedDataModelType);
            return model;
        }

        public async Task<T> GetDataModel<T>() where T : ODDataModel
        {
            return (T)(await GetDataModel());
        }
        
    }
}
