using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace OneDrive
{
    internal static class HttpExtensionMethods
    {
        internal static HttpResponseType ToHttpResponseType(this int httpStatusCode)
        {
            if (httpStatusCode >= 100 && httpStatusCode < 200)
                return HttpResponseType.Informational;
            else if (httpStatusCode >= 200 && httpStatusCode < 300)
                return HttpResponseType.Success;
            else if (httpStatusCode >= 300 && httpStatusCode < 400)
                return HttpResponseType.Redirection;
            else if (httpStatusCode >= 400 && httpStatusCode < 500)
                return HttpResponseType.ClientError;
            else
                return HttpResponseType.ServerError;
        }

        internal static HttpResponseType ToHttpResponseType(this System.Net.HttpStatusCode statusCode)
        {
            return ToHttpResponseType((int)statusCode);
        }

        internal static string CleanContentType(this string contentTypeHeaderValue)
        {
            if (!contentTypeHeaderValue.Contains(";"))
                return contentTypeHeaderValue;

            string[] parts = contentTypeHeaderValue.Split(';');
            if (parts.Length > 0)
                return parts[0].Trim();
            else
                return null;
        }

        internal static async Task<T> ConvertToDataModel<T>(this Http.IHttpResponse response) where T : ODDataModel
        {
            return (T)(await ConvertToDataModel(response, typeof(T)));
        }

        internal static async Task<ODDataModel> ConvertToDataModel(this Http.IHttpResponse response, Type t)
        {
            using (Stream stream = await response.GetResponseStreamAsync())
            {
                var reader = new StreamReader(stream);
                string data = await reader.ReadToEndAsync();
                
                ODDataModel result = data.ConvertToDataModel(t);
                return result;
            }
        }

        internal static T ConvertToDataModel<T>(this string jsonString) where T : ODDataModel
        {
            return (T)ConvertToDataModel(jsonString, typeof(T));
        }

        internal static ODDataModel ConvertToDataModel(this string jsonString, Type t)
        {
            try
            {
                ODDataModel result = (ODDataModel)Newtonsoft.Json.JsonConvert.DeserializeObject(jsonString, t);
#if DEBUG
                result.OriginalJson = jsonString;
#endif
                return result;
            }
            catch (Exception ex)
            {
                throw new ODSerializationException(ex.Message, jsonString, ex);
            }
        }

        internal static string ConvertToHttpResponse(this MultipartContent content)
        {
            if (!content.ContentType.Equals("application/http", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("ContentType of part doesn't match expected value");
            }

            return content.TextContent;
        }
    }

    public enum HttpResponseType
    {
        Informational,
        Success,
        Redirection,
        ClientError,
        ServerError
    }
}
