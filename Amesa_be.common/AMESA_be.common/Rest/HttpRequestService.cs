using AMESA_be.common.Rest;
using AMESA_be.common.Enums;
using AMESA_be.common.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System.Net;
using System.Text;
using AMESA_be.common.Rest;
using System.Net.Http;
using System.Net.Http.Json;

namespace Infra.Services.Common.Rest;

public class HttpRequestService : IHttpRequest
{
    private static readonly string SERVICE_DATETIME_TZ_FORMAT = "yyyy-MM-dd'T'HH:mm:ss.fffZ";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<HttpRequestService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _configuration;
    private readonly JsonSerializerSettings _jsonSettings;

    public HttpRequestService(
        IHttpClientFactory httpClientFactory,
        ILogger<HttpRequestService> logger,
        IHttpContextAccessor httpContextAccessor,
        IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
        _configuration = configuration;
        _jsonSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };
    }

    public async Task<T?> PostRequest<T>(string url, object content, string token = "",
        List<KeyValuePair<string, string>>? headers = null, string ClientName = "")
    {
        _logger.LogInformation("[PostRequest] {Url} - start  - content as object", url);
        var contentToString = JsonConvert.SerializeObject(content, _jsonSettings);
        return await PostRequest<T>(url, contentToString, token, headers, ClientName);
    }

    public async Task<T?> PostRequest<T>(string url, string content, string token = "",
        List<KeyValuePair<string, string>>? headers = null, string ClientName = "")
    {
        _logger.LogInformation("[PostRequest] {Url} - start  - content as string", url);
        using (var httpContent = new StringContent(content, Encoding.UTF8, "application/json"))
        {
            var httpClient = _httpClientFactory.CreateClient(ClientName);
            try
            {
                TryAddHttpClientHeaders(httpClient, token, headers);
                int? timeoutSeconds = _configuration.GetValue<int?>("HttpClients:TimeoutSec");

                httpClient.Timeout = timeoutSeconds.HasValue
                    ? TimeSpan.FromSeconds(timeoutSeconds.Value)
                    : TimeSpan.FromSeconds(100);

                using (var response = await httpClient.PostAsync(url, httpContent))
                {
                    var isSuccess = HandleHttpResponseAndLogs(response, url, "PostRequest");
                    if (!isSuccess)
                    {
                        return default;
                    }

                    var res = await response.Content.ReadAsStringAsync();
                    var serializedResponse = JsonConvert.DeserializeObject<T>(res, _jsonSettings);

                    return serializedResponse!;
                }
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, "[PostRequest] {Url}, Error message: {Exception}", url, e.Message);
                return default;
            }
        }
    }

    public async Task<HttpResult<T>> Post<T>(string url, object content, string token,
        List<KeyValuePair<string, string>>? headers = null, string ClientName = "")
    {
        _logger.LogInformation("[Post] {Url} - start  - content as object", url);
        var contentToString = JsonConvert.SerializeObject(content, _jsonSettings);
        return await Post<T>(url, contentToString, token, headers, ClientName);
    }

    public async Task<HttpResult<T>> Post<T>(string url, string content, string token,
        List<KeyValuePair<string, string>>? headers = null, string ClientName = "")
    {
        _logger.LogInformation("[Post] {Url} - start  - content as string", url);

        using (var httpContent = new StringContent(content, Encoding.UTF8, "application/json"))
        {
            var httpClient = _httpClientFactory.CreateClient(ClientName);
            try
            {
                TryAddHttpClientHeaders(httpClient, token, headers);
                int? timeoutSeconds = _configuration.GetValue<int?>("HttpClients:TimeoutSec");

                httpClient.Timeout = timeoutSeconds.HasValue
                    ? TimeSpan.FromSeconds(timeoutSeconds.Value)
                    : TimeSpan.FromSeconds(100);
                using (var response = await httpClient.PostAsync(url, httpContent))
                {
                    HttpResult<T> result = await HandleHttpResponse<T>(response, url, "PostRequest");
                    return result;
                }
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, "[Post] {Url}, Error message: {Exception}", url, e.Message);
                return HttpResult<T>.Failure(e.Message);
            }
        }
    }

    private void TryAddHttpClientHeaders(HttpClient httpClient, string sentToken,
        List<KeyValuePair<string, string>>? headers = null)
    {
        try
        {
            string token = (sentToken == string.Empty ? GetTokenFromHttpContextAccessor() : sentToken);
            if (!string.IsNullOrEmpty(token))
            {
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", token);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Message}", ex.Message);
        }

        try
        {
            var sessionId = _httpContextAccessor.GetSessionId();
            if (!string.IsNullOrEmpty(sessionId))
            {
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("SessionId", sessionId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Message}", ex.Message);
        }

        if (headers != null && headers.Count > 0)
        {
            try
            {
                foreach (KeyValuePair<string, string> header in headers)
                {
                    string headerName = header.Key;
                    string headerValue = header.Value;
                    httpClient.DefaultRequestHeaders.TryAddWithoutValidation(headerName, headerValue);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Message}", ex.Message);
            }
        }

        foreach (var (key, value) in httpClient.DefaultRequestHeaders)
        {
            var valueString = String.Join(Environment.NewLine, value);
            var header = $"{key}: {valueString}";
            _logger.LogDebug("Headers for: {Header}", header);
        }
    }

    public async Task<T?> PostRequestBearer<T>(string url, object content, string token = "", string ClientName = "")
    {
        var httpClient = _httpClientFactory.CreateClient(ClientName);
        try
        {
            _logger.LogInformation("[PostRequest] {Url} - start", url);
            //Take startTime and endTime of the request, to build timeout parameter in minutes
            DateTime startTime =
                DateTime.ParseExact(content.GetType().GetProperty("startTime").GetValue(content, null).ToString(),
                    "yyyyMMddHHmmss", null);
            DateTime endTime =
                DateTime.ParseExact(content.GetType().GetProperty("endTime").GetValue(content, null).ToString(),
                    "yyyyMMddHHmmss", null);
            double difference = endTime.Subtract(startTime).TotalMinutes;
            //Add 25% of the difference between dates, to make sure that timeout will be enough 
            difference += (difference * 25) / 100;

            //Get the default timeout configuration from appsettings.json
            int.TryParse(_configuration.GetValue<string>("RequestTimeout:timeout"), out int configurationTimeout);

            //Set timeout parameter. 
            httpClient.Timeout = configurationTimeout > difference
                ? TimeSpan.FromMinutes(configurationTimeout)
                : TimeSpan.FromMinutes(difference);

            var httpContent = new StringContent(JsonConvert.SerializeObject(content), Encoding.UTF8,
                "application/json");
            httpClient.DefaultRequestHeaders.Remove("Token");
            httpClient.DefaultRequestHeaders.Add("Authorization", token);
            var response = await httpClient.PostAsync(url, httpContent);
            var isSuccess = HandleHttpResponseAndLogs(response, url, "PostRequest");
            if (!isSuccess)
            {
                return default;
            }

            var serializedResponse =
                JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync(), _jsonSettings);
            return serializedResponse;
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "[PostRequest] {Url}, Error message: {Exception}", url, ex.Message);
            return default;
        }
    }

    public async Task<T?> DeleteRequest<T>(string url, object? content = null, string token = "", string ClientName = "")
    {
        _logger.LogInformation("[DeleteRequest] {Url} - start", url);
        var httpClient = _httpClientFactory.CreateClient(ClientName);

        using (var httpContent = new StringContent(JsonConvert.SerializeObject(content, _jsonSettings),
                   Encoding.UTF8, "application/json"))
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, url);

            if (content != null)
            {
                request.Content = httpContent;
            }

            try
            {
                TryAddHttpClientHeaders(httpClient, token);

                using (var response = await httpClient.SendAsync(request))
                {
                    var isSuccess = HandleHttpResponseAndLogs(response, url, "DeleteRequest");
                    if (!isSuccess)
                    {
                        return default;
                    }

                    var serializedResponse =
                        JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync(), _jsonSettings);

                    return serializedResponse!;
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "[DeleteRequest] {Url}, Error message: {Exception}", url, ex.Message);
                return default;
            }
        }
    }

    public async Task<T?> PatchRequest<T>(string url, object content, string token = "", string ClientName = "")
    {
        _logger.LogInformation("[PatchRequest] {Url} - start", url);

        var httpClient = _httpClientFactory.CreateClient(ClientName);
        using (var httpContent = new StringContent(JsonConvert.SerializeObject(content, _jsonSettings),
                   Encoding.UTF8, "application/json"))
        {
            try
            {
                TryAddHttpClientHeaders(httpClient, token);

                using (var response = await httpClient.PatchAsync(url, httpContent))
                {
                    var isSuccess = HandleHttpResponseAndLogs(response, url, "PatchRequest");
                    if (!isSuccess)
                    {
                        return default;
                    }

                    var serializedResponse =
                        JsonConvert.DeserializeObject<T?>(await response.Content.ReadAsStringAsync(), _jsonSettings);
                    return serializedResponse;
                }
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, "[PatchRequest] {Url}, Error message: {Exception}", url, e.Message);
                return default;
            }
        }
    }

    public async Task<T?> PutRequest<T>(string url, object content, string token = "", string ClientName = "")
    {
        _logger.LogInformation("[PutRequest] {Url} - start", url);
        var httpClient = _httpClientFactory.CreateClient(ClientName);
        using (var httpContent = new StringContent(JsonConvert.SerializeObject(content, _jsonSettings),
                   Encoding.UTF8, "application/json"))
        {
            try
            {
                TryAddHttpClientHeaders(httpClient, token);

                using (var response = await httpClient.PutAsync(url, httpContent))
                {
                    var isSuccess = HandleHttpResponseAndLogs(response, url, "PutRequest");
                    if (!isSuccess)
                    {
                        return default;
                    }

                    var serializedResponse =
                        JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync(), _jsonSettings);

                    return serializedResponse!;
                }
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, "[PutRequest] {Url}, Error message: {Exception}", url, e.Message);
                return default;
            }
        }
    }

    public async Task<T?> GetRequest<T>(string url, string token = "", List<KeyValuePair<string, string>>? headers = null, string ClientName = "")
    {
        _logger.LogInformation("[GetRequest] {Url} - start", url);
        var httpClient = _httpClientFactory.CreateClient(ClientName);

        try
        {
            TryAddHttpClientHeaders(httpClient, token, headers);
            using (var response = await httpClient.GetAsync(url))
            {
                var isSuccess = HandleHttpResponseAndLogs(response, url, "GetRequest");
                if (!isSuccess)
                {
                    return default;
                }

                var responseString = await response.Content.ReadAsStringAsync();
                _logger.LogDebug("[GetRequest] {Url} - finished. Content: {ResponseString}", url, responseString);
                var serializedResponse = JsonConvert.DeserializeObject<T>(responseString, _jsonSettings);
                return serializedResponse;
            }
        }
        catch (Exception e)
        {
            _logger.LogInformation(e, "[GetRequest] {Url}, Error message: {Exception}", url, e.Message);
            return default;
        }
    }

    public async Task<HttpResponseMessage?> GetRequest(string url, string token = "", string ClientName = "")
    {
        _logger.LogInformation("[GetRequest] {Url} - start", url);
        var httpClient = _httpClientFactory.CreateClient(ClientName);
        try
        {
            TryAddHttpClientHeaders(httpClient, token);

            var response = await httpClient.GetAsync(url);
            var isSuccess = HandleHttpResponseAndLogs(response, url, "GetRequest");

            return response;
        }
        catch (Exception e)
        {
            _logger.LogInformation(e, "[GetRequest] {Url}, Error message: {Exception}", url, e.Message);
            return default(HttpResponseMessage);
        }
    }

    public async Task<Stream?> GetRequest<T>(string url, string token, bool withoutSerialize = true, string ClientName = "")
    {
        _logger.LogInformation("[GetRequest] {Url} - start", url);
        var httpClient = _httpClientFactory.CreateClient(ClientName);
        try
        {
            TryAddHttpClientHeaders(httpClient, token);

            var response = await httpClient.GetStreamAsync(url);
            _logger.LogInformation("[GetRequest] {Url} - finished", url);

            return response;
        }
        catch (Exception e)
        {
            _logger.LogCritical(e, "[GetRequest] {Url}, Error message: {Exception}", url, e.Message);
            return default(Stream);
        }
    }

    public async Task<T?> GetRequestBearer<T>(string url, string token = "", string ClientName = "")
    {
        _logger.LogInformation("[GetRequest] {Url} - start", url);
        var httpClient = _httpClientFactory.CreateClient(ClientName);
        try
        {
            TryAddHttpClientHeaders(httpClient, token);

            using (var response = await httpClient.GetAsync(url))
            {
                var isSuccess = HandleHttpResponseAndLogs(response, url, "GetRequest");
                if (!isSuccess)
                {
                    return default(T);
                }

                var serializedResponse = await response.Content.ReadFromJsonAsync<T>();
                return serializedResponse!;
            }
        }
        catch (Exception e)
        {
            _logger.LogCritical(e, "[GetRequest] {Url}, Error message: {Exception}", url, e);
            return default(T);
        }
    }

    public async Task<string> GetStringRequest(string url, string token = "", string ClientName = "")
    {
        _logger.LogInformation("[GetRequest] {Url} - start", url);
        var httpClient = _httpClientFactory.CreateClient(ClientName);
        try
        {
            TryAddHttpClientHeaders(httpClient, token);

            using (var response = await httpClient.GetAsync(url))
            {
                var isSuccess = HandleHttpResponseAndLogs(response, url, "GetRequest");
                if (!isSuccess)
                {
                    return null;
                }

                var responseContent = await response.Content.ReadAsStringAsync();

                return responseContent;
            }
        }
        catch (Exception e)
        {
            _logger.LogInformation(e, "[GetRequest] {Url}, Error message: {Exception}", url, e.Message);
            return null;
        }
    }

    public async Task<T?> PostNonAMESAMultipartHttpRequest<T>(string url, Dictionary<string, List<string>> files,
        Dictionary<string, object>? data = null, string token = "")
    {
        try
        {
            _logger.LogInformation("[PostRequest] {Url} - start", url);
            string resultString = await PostNonAMESAMultipartHttpRequest(url, files, data, token);
            _logger.LogInformation("[PostRequest] {Url} - finished", url);

            T result = default(T);
            if (!string.IsNullOrEmpty(resultString))
            {
                result = JsonConvert.DeserializeObject<T>(resultString, _jsonSettings)!;
            }

            return result!;
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "[PostRequest] {Url}, Error message: {Exception}", url, ex.Message);

            return default(T);
        }
    }

    /// <summary>
    /// Method migrated from Argus without any changes. There is refactored method with same name but async and that using HttpFactory.
    /// 
    /// N-O-T    W-O-R-K-I-N-G!!!!!!!!!!
    /// 
    /// dont use this method
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="U"></typeparam>
    /// <param name="url"></param>
    /// <param name="httpMethod"></param>
    /// <param name="data"></param>
    /// <param name="timeoutMilliseconds"></param>
    /// <param name="cookieContainer"></param>
    /// <param name="headers"></param>
    /// <param name="ResultFunc"></param>
    /// <returns></returns>
    public T SendNonAMESAHttpRequest<T, U>(string url, HttpMethod httpMethod, U data, int? timeoutMilliseconds = null,
        CookieContainer cookieContainer = null, Dictionary<string, string> headers = null,
        Func<HttpWebResponse, T, T> ResultFunc = null)
    {
        _logger.LogInformation("[{HttpMethod}] {Url} - start", httpMethod, url);

        dynamic result = null;
        try
        {
#pragma warning disable SYSLIB0014 // Type or member is obsolete
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
#pragma warning restore SYSLIB0014 // Type or member is obsolete
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = httpMethod.Method;

            // Add headers to the requests
            if (headers != null && headers.Count > 0)
            {
                foreach (string currKey in headers.Keys)
                {
                    httpWebRequest.Headers.Add(currKey, headers[currKey]);
                }
            }

            if (cookieContainer != null)
            {
                httpWebRequest.CookieContainer = cookieContainer;
            }

            if (timeoutMilliseconds != null)
            {
                httpWebRequest.Timeout = timeoutMilliseconds.Value;
            }

            if (data == null)
            {
                httpWebRequest.ContentLength = 0;
            }
            else //if (data != null)
            {
                // Adding the body parameters
                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    var serializerSettings = new JsonSerializerSettings
                    {
                        Formatting = Formatting.Indented,
                        TypeNameHandling = TypeNameHandling.Auto,
                    };
                    serializerSettings.Converters.Add(new IsoDateTimeConverter
                    {
                        DateTimeFormat = SERVICE_DATETIME_TZ_FORMAT
                    });

                    string json = JsonConvert.SerializeObject(data, serializerSettings);

                    streamWriter.Write(json);
                }
            }

            result = MakeNonAMESARequestAndGetResponse<T>(url, result, httpWebRequest, ResultFunc);
            _logger.LogInformation("[{HttpMethod}] {Url} - finished", httpMethod, url);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "[{HttpMethod}] {Url}, Error message: {Exception}", httpMethod, url, ex.Message);

            return default(T);
        }
    }

    private static T MakeNonAMESARequestAndGetResponse<T>(string url, T result, HttpWebRequest httpWebRequest,
        Func<HttpWebResponse, T, T> ResultFunc = null)
    {
        // Getting the server response and serializing it to TA9HttpResult

        var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
        using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
        {
            string resultString = streamReader.ReadToEnd();

            result = JsonConvert.DeserializeObject<T>(resultString);
            if (ResultFunc != null)
            {
                result = ResultFunc(httpResponse, result);
            }

            return result;
        }
    }

    public async Task<T> SendNonAMESAHttpRequestAsync<T, U>(string url, HttpMethod httpMethod, U data,
        Dictionary<string, string>? headers = null)
    {
        var handler = new HttpClientHandler();
        handler.ClientCertificateOptions = ClientCertificateOption.Automatic;
        var httpClient = new HttpClient(handler);
        try
        {
            _logger.LogInformation("[{HttpMethod}] {Url} - start", httpMethod, url);

            // Creating the HTTP request according to the given parameters
            var httpRequest = new HttpRequestMessage
            {
                RequestUri = new Uri(url),
                Method = httpMethod,
                Content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json")
            };

            // Add headers to the request
            if (headers != null && headers.Count > 0)
            {
                foreach (string currKey in headers.Keys)
                {
                    httpRequest.Headers.Add(currKey, headers[currKey]);
                }
            }

            var httpResponseMessage = await httpClient.SendAsync(httpRequest);
            _logger.LogInformation("[{HttpMethod}] {Url} - finished, Status: {StatusCode}", httpMethod, url,
                httpResponseMessage.StatusCode.ToString());

            if (!httpResponseMessage.IsSuccessStatusCode)
            {
                throw new Exception(
                    $"Returned from: {url}\t{httpMethod}\t{data}. Status: ({httpResponseMessage.StatusCode}) {httpResponseMessage.ReasonPhrase}");
            }

            var result = JsonConvert.DeserializeObject<T>(await httpResponseMessage.Content.ReadAsStringAsync());
            _logger.LogInformation("Response from NonTAHttpRequest: {Result}", result);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "[HttpRequest] {Url}, Error message: {Exception}", url, ex.Message);
            return default(T);
        }
        finally
        {
            httpClient.Dispose();
            handler.Dispose();
        }
    }

    public async Task<T> SendGetNonAMESAHttpRequestAsync<T, U>(string url, Dictionary<string, string> headers = null)
    {
        var handler = new HttpClientHandler();
        handler.ClientCertificateOptions = ClientCertificateOption.Automatic;
        var httpClient = new HttpClient(handler);
        try
        {
            _logger.LogInformation("[GetRequest] {Url} - start", url);

            var response = await httpClient.GetAsync(url);
            var isSuccess = HandleHttpResponseAndLogs(response, url, "GetRequest");
            if (!isSuccess)
            {
                return default(T);
            }

            var result = JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync());
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "[GetRequest] {Url}, Error message: {Exception}", url, ex.Message);
            return default(T);
        }
        finally
        {
            httpClient.Dispose();
            handler.Dispose();
        }
    }

    public async Task<string> PostNonAMESAMultipartHttpRequest(string url, Dictionary<string, List<string>> files,
        Dictionary<string, object> data = null, string token = null)
    {
        try
        {
            var httpWebRequest = CreateMultipartRequest(url, files, data, token);

            // Getting the server response and serializing it to TA9HttpResult
            var httpResponse = await httpWebRequest.GetResponseAsync();
            _logger.LogInformation("[PostRequest] {Url} - finished", url);

            string resultString = null;
            using (var streamReader = new StreamReader(((HttpWebResponse)httpResponse).GetResponseStream()))
            {
                resultString = streamReader.ReadToEnd();
            }

            return resultString;
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "[PostRequest] {Url}, Error message: {Exception}", url, ex.Message);

            return default(string);
        }
    }

    /// <summary>
    /// Create multipart form data content with specified file path.
    /// </summary>
    /// <param name="filePath">File path to set</param>
    /// <returns>Multipart content with file data</returns>
    public static MultipartFormDataContent CreateMultipartContent(string filePath)
    {
        FileInfo fileInfo = new FileInfo(filePath);
        byte[] fileContents = File.ReadAllBytes(fileInfo.FullName);
        MultipartFormDataContent multiPartContent = new MultipartFormDataContent();
        ByteArrayContent byteArrayContent = new ByteArrayContent(fileContents);
        byteArrayContent.Headers.Add("Content-Type", "application/octet-stream");
        multiPartContent.Add(byteArrayContent, "file", fileInfo.Name);

        return multiPartContent;
    }

    private HttpWebRequest CreateMultipartRequest(string url, Dictionary<string, List<string>> files,
        Dictionary<string, object> data = null, string token = null, int? timeoutMillis = null)
    {
        string boundary = "----------------------------" + DateTime.Now.Ticks.ToString("x");
        // The first boundary
        byte[] boundaryBytes = Encoding.UTF8.GetBytes("\r\n--" + boundary + "\r\n");
        // The last boundary
        byte[] trailer = Encoding.UTF8.GetBytes("\r\n--" + boundary + "--\r\n");
        // The first time it iterates, we need to make sure it doesn't put too many new paragraphs down or it completely messes up poor webbrick
        byte[] boundaryBytesF = Encoding.ASCII.GetBytes("--" + boundary + "\r\n");

        // Create the request and set parameters 
#pragma warning disable SYSLIB0014 // Type or member is obsolete
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
#pragma warning restore SYSLIB0014 // Type or member is obsolete
        if (timeoutMillis != null && timeoutMillis.HasValue)
        {
            request.Timeout = timeoutMillis.Value;
            request.ReadWriteTimeout = timeoutMillis.Value;
            request.ContinueTimeout = timeoutMillis.Value;
        }

        request.ContentType = "multipart/form-data; boundary=" + boundary;
        request.Method = "POST";
        request.KeepAlive = true;
        request.Credentials = CredentialCache.DefaultCredentials;
        request.Headers.Add("Authorization",
            String.IsNullOrEmpty(token)
                ? _httpContextAccessor.GetHeaderValue(AMESAClaimTypes.Authorization.ToString())
                : token);

        // Get request stream
        Stream requestStream = request.GetRequestStream();
        if (data != null)
        {
            foreach (string key in data.Keys)
            {
                // Write item to stream
                string json = JsonConvert.SerializeObject(data[key]);
                byte[] formItemBytes =
                    Encoding.UTF8.GetBytes(
                        string.Format("Content-Disposition: form-data; name=\"{0}\";\r\n\r\n{1}", key, json));
                requestStream.Write(boundaryBytes, 0, boundaryBytes.Length);
                requestStream.Write(formItemBytes, 0, formItemBytes.Length);
            }
        }

        if (files != null)
        {
            foreach (string key in files.Keys)
            {
                foreach (var fileName in files[key])
                {
                    if (File.Exists(fileName))
                    {
                        int bytesRead = 0;
                        byte[] buffer = new byte[2048];
                        byte[] formItemBytes = Encoding.UTF8.GetBytes(
                            string.Format(
                                "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: application/octet-stream\r\n\r\n",
                                key, Path.GetFileName(fileName)));
                        requestStream.Write(boundaryBytes, 0, boundaryBytes.Length);
                        requestStream.Write(formItemBytes, 0, formItemBytes.Length);

                        using (FileStream fileStream =
                               new FileStream(fileName, FileMode.Open, FileAccess.Read))
                        {
                            while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
                            {
                                // Write file content to stream, byte by byte
                                requestStream.Write(buffer, 0, bytesRead);
                            }

                            fileStream.Close();
                        }
                    }
                }
            }
        }

        // Write trailer and close stream
        requestStream.Write(trailer, 0, trailer.Length);
        requestStream.Close();

        return request;
    }

    private string GetTokenFromHttpContextAccessor()
    {
        if (_httpContextAccessor.HttpContext != null)
        {
            var token = _httpContextAccessor.GetHeaderValue(AMESAHeaders.Authorization.GetValue());
            return token;
        }

        return string.Empty;
    }

    private bool HandleHttpResponseAndLogs(HttpResponseMessage response, string url, string method)
    {
        if (response != null && response.StatusCode == HttpStatusCode.OK)
        {
            _logger.LogInformation("[{Method}] {Url} - finished, StatusCode: {StatusCode}", method, url,
                response.StatusCode.ToString());
            return true;
        }

        if (response != null && response.StatusCode == HttpStatusCode.NoContent)
        {
            _logger.LogInformation("[{Method}] {Url} - finished, StatusCode: {StatusCode}", method, url,
                response.StatusCode.ToString());
            return false;
        }

        _logger.LogInformation("[{Method}] {Url} - finished, StatusCode: {StatusCode} and Payload : {Payload}", method,
            url, response?.StatusCode.ToString(), response?.Content.ReadAsStringAsync());

        return false;
    }

    private async Task<HttpResult<T>> HandleHttpResponse<T>(HttpResponseMessage? response, string url, string method)
    {
        if (response == null)
        {
            _logger.LogInformation("[{Method}] {Url} - finished, No Response", method, url);
            return HttpResult<T>.Failure("No Response");
        }

        if (response.IsSuccessStatusCode)
        {
            _logger.LogInformation("[{Method}] {Url} - finished, StatusCode: {StatusCode}", method, url,
                response.StatusCode);

            var valueStr = await response.Content.ReadAsStringAsync();
            var value = JsonConvert.DeserializeObject<T>(valueStr, _jsonSettings);

            return HttpResult<T>.Success(value, response.StatusCode);
        }

        var payload = await response.Content.ReadAsStringAsync();
        _logger.LogInformation("[{Method}] {Url} - finished, StatusCode: {StatusCode} and Payload : {Payload}", method,
            url, response.StatusCode, payload);
        return HttpResult<T>.Failure(payload, response.StatusCode);
    }

}