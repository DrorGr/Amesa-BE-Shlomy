using System.Net;

namespace AMESA_be.common.Rest
{
    public interface IHttpRequest
    {
        /// <summary>
        /// Http get request which returns raw HttpResponseMessage
        /// </summary>
        /// <param name="url"></param>
        /// <param name="token"></param>
        /// <param name="ClientName"></param>
        /// <returns></returns>
        public Task<HttpResponseMessage?> GetRequest(string url, string token = "", string ClientName = "");

        public Task<T?> GetRequest<T>(string url, string token = "",
            List<KeyValuePair<string, string>>? headers = null, string ClientName = "");

        public Task<Stream?> GetRequest<T>(string url, string token, bool withoutSerialize = true, string ClientName = "");
        public Task<T?> GetRequestBearer<T>(string url, string token = "", string ClientName = "");
        public Task<string> GetStringRequest(string url, string token = "", string ClientName = "");

        public Task<T?> PostRequest<T>(string url, object content, string token = "",
            List<KeyValuePair<string, string>>? headers = null, string ClientName = "");

        public Task<T?> PostRequest<T>(string url, string content, string token = "",
            List<KeyValuePair<string, string>>? headers = null, string ClientName = "");

        public Task<T?> DeleteRequest<T>(string url, object? content = null, string token = "", string ClientName = "");
        public Task<T?> PostRequestBearer<T>(string url, object content, string token = "", string ClientName = "");
        public Task<T?> PatchRequest<T>(string url, object content, string token = "", string ClientName = "");
        public Task<T?> PutRequest<T>(string url, object content, string token = "", string ClientName = "");

        public Task<T> PostNonAMESAMultipartHttpRequest<T>(string url, Dictionary<string, List<string>> files,
            Dictionary<string, object>? data = null, string token = "");

        public Task<T> SendGetNonAMESAHttpRequestAsync<T, U>(string url, Dictionary<string, string>? headers = null);


        /// <summary>
        /// Argus Non Ta request
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <param name="url"></param>
        /// <param name="httpMethod"></param>
        /// <param name="data"></param>
        /// <param name="timeoutMilliseconds"></param>
        /// <param name="cookieContainer"></param>
        /// <param name="headers"></param>
        /// <param name="resultFunc"></param>
        /// <returns></returns>
        T SendNonAMESAHttpRequest<T, U>(string url, HttpMethod httpMethod, U data, int? timeoutMilliseconds = null,
            CookieContainer? cookieContainer = null, Dictionary<string, string>? headers = null,
            Func<HttpWebResponse, T, T>? resultFunc = null);

        /// <summary>
        /// Argus Non Ta request using HttpClient
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <param name="url"></param>
        /// <param name="httpMethod"></param>
        /// <param name="data"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        Task<T> SendNonAMESAHttpRequestAsync<T, U>(string url, HttpMethod httpMethod, U data,
            Dictionary<string, string>? headers = null);

        public Task<HttpResult<T>> Post<T>(string url, object content, string token = "",
            List<KeyValuePair<string, string>>? headers = null, string ClientName = "");

        public Task<HttpResult<T>> Post<T>(string url, string content, string token = "",
            List<KeyValuePair<string, string>>? headers = null, string ClientName = "");
    }
}
