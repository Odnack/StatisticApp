using DbLayer.Tables.Core;
using Infrastructure.Compression;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace RestClient.Helpers
{
    public class RestHelper
    {
        #region - Private fields -
        private readonly HttpClient _httpClient;
        private readonly string _url;

        private readonly Dictionary<int, string> _tokenByUserId = new();

        private const int _maxTryCount = 5;
        #endregion

        #region - Constructor -
        public RestHelper(HttpClient httpClient, string url)
        {
            httpClient.Timeout = TimeSpan.FromHours(24);
            httpClient.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
            _httpClient = httpClient;
            _url = url;
        }
        #endregion

        #region - Public fields -
        public int CurrUserId { get; set; }

        public double CurrUserTimeZone { get; set; }

        public string CurrToken
        {
            get => _tokenByUserId.ContainsKey(CurrUserId) ? _tokenByUserId[CurrUserId] : "";
            set => _tokenByUserId[CurrUserId] = value;
        }

        public enum SerializationType
        {
            Json,
            Binary
        }
        #endregion

        #region - Rest methods -
        #region - Get -
        public async Task<TOutData> ExecuteGetAnonymous<TOutData>(string serviceUrl, string address)
            where TOutData : new()
        {
            return await ExecuteGetAnonymous<TOutData>(serviceUrl, address, new Tuple<string, string>[] { });
        }

        public async Task<TOutData> ExecuteGetAnonymous<TOutData>(string serviceUrl, string address, params Tuple<string, string>[] parameters)
            where TOutData : new()
        {
            return await ExecuteGet<TOutData>(serviceUrl, address, parameters, false);
        }

        public async Task<TOutData> ExecuteGet<TOutData>(string serviceUrl, string address)
            where TOutData : new()
        {
            return await ExecuteGet<TOutData>(serviceUrl, address, new Tuple<string, string>[] { });
        }

        public async Task<TOutData> ExecuteGet<TOutData>(string serviceUrl, string address, params Tuple<string, string>[] parameters)
            where TOutData : new()
        {
            return await ExecuteGet<TOutData>(serviceUrl, address, parameters, true);
        }

        public async Task<TOutData> ExecuteGet<TOutData>(string serviceUrl, string address, IEnumerable<Tuple<string, string>> parameters,
            bool needAuthentication)
            where TOutData : new()
        {
            address = CreateFullAddress(serviceUrl, address);
            address = AddUrlParameters(address, parameters);

            return await SendAndRetryStringDecompress<TOutData>(HttpMethod.Get, null, address, needAuthentication, true);
        }

        public async Task<Stream> ExecuteGetFileStream(string serviceUrl, string address, IEnumerable<Tuple<string, string>> parameters)
        {
            address = CreateFullAddress(serviceUrl, address);
            address = AddUrlParameters(address, parameters);

            return await SendAndRetryStreamDecompress(HttpMethod.Get, null, address);
        }
        #endregion

        #region - Put -
        public async Task<TOutData> ExecutePutAnonymous<TOutData>(string serviceUrl, string address, object inData, bool needReturn)
            where TOutData : new()
        {
            return await ExecutePut<TOutData>(serviceUrl, address, inData, false, needReturn);
        }

        public async Task<TOutData> ExecutePut<TOutData>(string serviceUrl, string address, object inData, bool needReturn)
            where TOutData : new()
        {
            return await ExecutePut<TOutData>(serviceUrl, address, inData, true, needReturn);
        }

        public async Task<TOutData> ExecutePut<TOutData>(string serviceUrl, string address, object inData,
            bool needAuthentication,
            bool needReturn)
            where TOutData : new()
        {
            address = CreateFullAddress(serviceUrl, address);

            string jsonToSend = SerializeToJson(inData);
            using (var content = new StringContent(jsonToSend, Encoding.UTF8, "application/json"))
            {
                return await SendAndRetryStringDecompress<TOutData>(HttpMethod.Put, content, address, needAuthentication, needReturn);
            }
        }
        #endregion

        #region - Post -
        public async Task<TOutData> ExecutePostAnonymous<TOutData>(string serviceUrl, string address, bool needReturn)
            where TOutData : new()
        {
            return await ExecutePostAnonymous<TOutData>(serviceUrl, address, new object(), needReturn);
        }

        public async Task<TOutData> ExecutePostAnonymous<TOutData>(string serviceUrl, string address, object inData, bool needReturn)
            where TOutData : new()
        {
            return await ExecutePost<TOutData>(serviceUrl, address, inData, SerializationType.Json, false, needReturn);
        }

        public async Task<TOutData> ExecutePost<TOutData>(string serviceUrl, string address, bool needReturn)
            where TOutData : new()
        {
            return await ExecutePost<TOutData>(serviceUrl, address, new object(), needReturn);
        }

        public async Task<TOutData> ExecutePost<TOutData>(string serviceUrl, string address, object inData, bool needReturn)
            where TOutData : new()
        {
            return await ExecutePost<TOutData>(serviceUrl, address, inData, SerializationType.Json, true, needReturn);
        }

        public async Task<TOutData> ExecutePost<TOutData>(string serviceUrl, string address, object inData, SerializationType serializationType, bool needReturn)
            where TOutData : new()
        {
            return await ExecutePost<TOutData>(serviceUrl, address, inData, serializationType, true, needReturn);
        }

        private async Task<TOutData> ExecutePost<TOutData>(string serviceUrl, string address, object inData,
            SerializationType serializationType,
            bool needAuthentication,
            bool needReturn)
            where TOutData : new()
        {
            address = CreateFullAddress(serviceUrl, address);

            switch (serializationType)
            {
                case SerializationType.Json:
                    string jsonToSend = SerializeToJson(inData);
                    using (var content = new StringContent(jsonToSend, Encoding.UTF8, "application/json"))
                    {
                        return await SendAndRetryStringDecompress<TOutData>(HttpMethod.Post, content, address, needAuthentication, needReturn);
                    }
                case SerializationType.Binary:
                    var memoryStream = new MemoryStream();
                    var binaryFormatter = new BinaryFormatter();
                    binaryFormatter.Serialize(memoryStream, inData);
                    using (var content = new ByteArrayContent(memoryStream.ToArray()))
                    {
                        return await SendAndRetryStringDecompress<TOutData>(HttpMethod.Post, content, address, needAuthentication, needReturn);
                    }
                default:
                    throw new ArgumentOutOfRangeException(nameof(serializationType), serializationType, null);
            }
        }

        public async Task<Stream> ExecutePostFileStream(string serviceUrl, string address, object inData)
        {
            address = CreateFullAddress(serviceUrl, address);

            string jsonToSend = SerializeToJson(inData);
            using (var content = new StringContent(jsonToSend, Encoding.UTF8, "application/json"))
            {
                return await SendAndRetryStreamDecompress(HttpMethod.Post, content, address);
            }
        }
        #endregion      

        #region - Delete -
        public async Task ExecuteDelete(string serviceUrl, string address, params Tuple<string, string>[] parameters)
        {
            await ExecuteDelete(serviceUrl, address, parameters, true);
        }

        public async Task ExecuteDelete(string serviceUrl, string address, IEnumerable<Tuple<string, string>> parameters,
            bool needAuthentication)
        {
            address = CreateFullAddress(serviceUrl, address);
            address = AddUrlParameters(address, parameters);

            await SendAndRetryStringDecompress<object>(HttpMethod.Delete, null, address, needAuthentication, false);
        }
        #endregion  
        #endregion

        #region - Rest private methods -
        private HttpRequestMessage CreateMessage(HttpMethod httpMethod, HttpContent httpContent, string address, bool needAuthentication)
        {
            var requestMessage = new HttpRequestMessage(httpMethod, address);
            if (httpContent != null)
                requestMessage.Content = httpContent;

            if (needAuthentication)
            {
                AddBearer(requestMessage, CurrUserId);
                AddTimezone(requestMessage);
            }

            return requestMessage;
        }

        private HttpRequestMessage CopyMessage(HttpRequestMessage requestMessage)
        {
            var newMessage = new HttpRequestMessage()
            {
                Content = requestMessage.Content,
                Method = requestMessage.Method,
                RequestUri = requestMessage.RequestUri,
                Version = requestMessage.Version
            };

            foreach (var requestMessageProperty in requestMessage.Properties)
                newMessage.Properties.Add(requestMessageProperty.Key, requestMessageProperty.Value);

            foreach (var header in requestMessage.Headers)
                newMessage.Headers.Add(header.Key, header.Value);

            return newMessage;
        }

        private async Task<TOutData> SendAndRetryStringDecompress<TOutData>(HttpMethod httpMethod, HttpContent httpContent, string address, bool needAuthentication, bool needReturn)
            where TOutData : new()
        {
            try
            {
                var requestMessage = CreateMessage(httpMethod, httpContent, address, needAuthentication);

                using (var resultContent = await SendRequestAndProcessErrors(requestMessage))
                {
                    if (needReturn)
                    {
                        var resultString = await GetContentAsStringAsync(resultContent);
                        return JsonConvert.DeserializeObject<TOutData>(resultString);
                    }
                    else
                        return new TOutData();
                }
            }
            catch (System.Exception exception)
            {
                Log.Error($"Request exception: {exception.Message} stack:{exception.StackTrace} " +
                          $"inner:{exception.InnerException?.Message} stack:{exception.InnerException?.StackTrace}");
                throw new Exception();
            }
        }

        private async Task<Stream> SendAndRetryStreamDecompress(HttpMethod httpMethod, HttpContent httpContent, string address)
        {
            try
            {
                var requestMessage = CreateMessage(httpMethod, httpContent, address, true);

                var resultContent = await SendRequestAndProcessErrors(requestMessage);
                return await GetContentAsStreamAsync(resultContent);
            }
            catch (System.Exception exception)
            {
                Log.Error($"Request exception: {exception.Message} stack:{exception.StackTrace} " +
                          $"inner:{exception.InnerException?.Message} stack:{exception.InnerException?.StackTrace}");
                throw new Exception();
            }
        }

        private async Task<HttpResponseMessage> SendRequestAndProcessErrors(HttpRequestMessage requestMessage, HttpCompletionOption httpCompletionOption = HttpCompletionOption.ResponseHeadersRead, int tryCount = 0)
        {
            HttpResponseMessage resultContent;
            try
            {
                Log.Information($"Request started, url={requestMessage.RequestUri} type={requestMessage.Method}");
                resultContent = await _httpClient.SendAsync(requestMessage, httpCompletionOption);
            }
            catch (HttpRequestException httpException)
            {
                if (tryCount < _maxTryCount)
                {
                    await Task.Delay(3000 * tryCount);
                    return await SendRequestAndProcessErrors(CopyMessage(requestMessage), httpCompletionOption, tryCount + 1);
                }
                else
                {
                    Log.Error($"Request error, url={requestMessage.RequestUri} type={requestMessage.Method}");
                    Log.Error($"Request exception: {httpException.Message} stack:{httpException.StackTrace} " +
                              $"inner:{httpException.InnerException?.Message} stack:{httpException.InnerException?.StackTrace}");
                    throw new Exception();
                }
            }
            catch (System.Exception exception)
            {
                Log.Error($"Request error, url={requestMessage.RequestUri} type={requestMessage.Method}");
                Log.Error($"Request exception: {exception.Message} stack:{exception.StackTrace} " +
                          $"inner:{exception.InnerException?.Message} stack:{exception.InnerException?.StackTrace}");
                throw exception;
            }

            if (await ProcessErrors(resultContent, tryCount))
            {
                await Task.Delay(3000 * tryCount);
                return await SendRequestAndProcessErrors(CopyMessage(requestMessage), httpCompletionOption, tryCount + 1);
            }

            Log.Information($"Request ended, url={requestMessage.RequestUri} type={requestMessage.Method}");
            return resultContent;
        }

        private async Task<bool> ProcessErrors(HttpResponseMessage resultContent, int tryCount = 0)
        {
            if (resultContent.IsSuccessStatusCode)
                return false;

            // сервер может посылать обработанные ошибки только с кодом.
            // всё остальное - ошибки клиента или подключения
            switch (resultContent.StatusCode)
            {
                case HttpStatusCode.InternalServerError:
                    string backendErrorMessageString = await GetContentAsStringAsync(resultContent);
                    if (!string.IsNullOrWhiteSpace(backendErrorMessageString))
                        throw new Exception(backendErrorMessageString);
                    else
                        throw new Exception();
                //case HttpStatusCode.Unauthorized:
                //    throw new Exception(Resources.UnauthorizedExceptionMessage);
                //case HttpStatusCode.NotFound:
                //    throw new Exception(Resources.NotFound404ExceptionMessage);
                //case HttpStatusCode.ProxyAuthenticationRequired:
                //    throw new Exception(Resources.ProxyAuthenticationRequiredExceptionMessage);
                //case HttpStatusCode.ServiceUnavailable:
                //    if (tryCount < _maxTryCount)
                //        return true;
                //    else
                //        throw new MessageForUserException(Resources.ServiceUnavailableExceptionMessage);
                default:
                    Log.Error(resultContent.ReasonPhrase);
                    string unknownErrorMessageString = await GetContentAsStringAsync(resultContent);
                    if (!string.IsNullOrWhiteSpace(unknownErrorMessageString))
                        Log.Error(unknownErrorMessageString);
                    if (tryCount < _maxTryCount)
                        return true;
                    else
                        throw new Exception();
            }
        }
        #endregion

        #region - Additional public methods -
        public void SetUserToken(int userId, string token)
        {
            _tokenByUserId[userId] = token;
        }
        #endregion

        #region - Additional private methods -
        private string SerializeToJson(object inData)
        {
            return JsonConvert.SerializeObject(inData, new JsonSerializerSettings
            {
                DateTimeZoneHandling = DateTimeZoneHandling.Unspecified
            });
        }

        private string CreateFullAddress(string serviceUrl, string address)
        {
            if (string.IsNullOrEmpty(_url))
                throw new Exception();

            var result = _url;
            if (result.Last() != '/')
                result += "/";

            return result + serviceUrl + "/" + address;
        }

        private string AddUrlParameters(string address, IEnumerable<Tuple<string, string>> parameters)
        {
            bool isFirstParameter = true;
            foreach (var parameter in parameters)
            {
                address += isFirstParameter ? "?" : "&";
                address += parameter.Item1 + "=" + parameter.Item2;
                isFirstParameter = false;
            }
            return address;
        }

        private void AddBearer(HttpRequestMessage request, int clientUserId)
        {
            if (!_tokenByUserId.ContainsKey(clientUserId))
                throw new Exception();

            var token = _tokenByUserId[clientUserId];
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        private void AddTimezone(HttpRequestMessage request)
        {
            var offset = TimeZoneInfo.Local.BaseUtcOffset;
            request.Headers.Add("Timezone", offset.TotalHours.ToString(CultureInfo.InvariantCulture));
        }

        private async Task<string> GetContentAsStringAsync(HttpResponseMessage httpResponseMessage)
        {
            using (var content = httpResponseMessage.Content)
            {
                if (content.Headers.ContentEncoding.Contains(CompressionType.GZIP.ToString().ToLower()))
                {
                    using (var stream = await DecompressContentAsync(content, CompressionType.GZIP))
                    {
                        StreamReader streamReader = new StreamReader(stream);
                        return streamReader.ReadToEnd();
                    }
                }

                if (content.Headers.ContentEncoding.Contains(CompressionType.ZStandard.ToString().ToLower()))
                {
                    using (var stream = await DecompressContentAsync(content, CompressionType.GZIP))
                    {
                        StreamReader streamReader = new StreamReader(stream);
                        return streamReader.ReadToEnd();
                    }
                }

                if (content.Headers.ContentEncoding.Contains(CompressionType.Unknown.ToString().ToLower()))
                    throw new Exception();

                return await content.ReadAsStringAsync();
            }
        }

        private async Task<Stream> GetContentAsStreamAsync(HttpResponseMessage httpResponsemessage)
        {
            if (httpResponsemessage.Content.Headers.ContentEncoding.Contains(CompressionType.GZIP.ToString().ToLower()))
                return await DecompressContentAsync(httpResponsemessage.Content, CompressionType.GZIP);

            if (httpResponsemessage.Content.Headers.ContentEncoding.Contains(CompressionType.ZStandard.ToString().ToLower()))
                return await DecompressContentAsync(httpResponsemessage.Content, CompressionType.ZStandard);

            if (httpResponsemessage.Content.Headers.ContentEncoding.Contains(CompressionType.Unknown.ToString().ToLower()))
                throw new Exception();

            return await httpResponsemessage.Content.ReadAsStreamAsync();
        }

        private async Task<Stream> DecompressContentAsync(HttpContent httpContent, CompressionType compressionType)
        {
            Stream stream = await httpContent.ReadAsStreamAsync();
            switch (compressionType)
            {
                case CompressionType.GZIP:
                    return Compressor.GZIP.Decompress(stream);
                case CompressionType.ZStandard:
                    return Compressor.ZStandart.Decompress(stream);
                default:
                    throw new ArgumentOutOfRangeException(nameof(compressionType), compressionType, null);
            }
        }
        #endregion       
    }
}
