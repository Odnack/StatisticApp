using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Api.Api;
using Api.Api.Authentication;
using Api.Api.Notifications;
using Api.Api.Tools;
using RestClient.Clients;
using RestClient.Helpers;
using RestClient.Tools;

namespace RestClient
{
    public struct ClientAndProxy
    {
        public HttpClient HttpClient { get; set; }
        public WebProxy Proxy { get; set; }
    }
    public class BackendRest : IBackendApi
    {
        private readonly string _url;
        private readonly RestHelper _restHelper;
        private readonly WebProxy _proxy;
        public BackendRest(string url, ProxySettings proxySettings, bool initEvents = true)
            : this(CreateClient(url, proxySettings), url, initEvents)
        {
        }
        public BackendRest(HttpClient httpClient, string url, bool initEvents = true)
            : this(new ClientAndProxy { HttpClient = httpClient }, url, initEvents)
        {
        }

        public BackendRest(ClientAndProxy сlientAndProxy, string url, bool initEvents = true)
        {
            var httpClient = сlientAndProxy.HttpClient;
            _proxy = сlientAndProxy.Proxy;

            _url = url;
            Authentication = new AuthenticationRest(_restHelper);
            Notifications = new NotificationsRest(_restHelper);
            var tools = new ToolsRest(_restHelper);
            Tools = tools;
            
        }
        public struct ProxySettings
        {
            public bool UseProxy { get; set; }
            public string UrlWithPort { get; set; }
        }
        private static ClientAndProxy CreateClient(string url, ProxySettings proxySettings)
        {
            var result = new ClientAndProxy();

            if (proxySettings.UseProxy)
            {
                var proxy = new WebProxy(proxySettings.UrlWithPort, BypassOnLocal: false)
                {
                    Credentials = CredentialCache.DefaultCredentials
                };

                var httpClientHandler = new HttpClientHandler()
                {
                    Proxy = proxy,
                    UseProxy = true,
                };
                result.Proxy = proxy;
                result.HttpClient = new HttpClient(httpClientHandler);
            }
            else
            {
                var httpClientHandler = new HttpClientHandler
                {
                    UseProxy = false
                };
                result.HttpClient = new HttpClient(httpClientHandler);
            }
            result.HttpClient.DefaultRequestHeaders.Connection.Add("keep-alive");
            result.HttpClient.BaseAddress = new Uri(url);
            return result;
        }
        public IAuthenticationService Authentication { get; }
        public IToolsService Tools { get; }
        public INotificationService Notifications { get; }
        public int UserId
        {
            get => _restHelper.CurrUserId;
            set => _restHelper.CurrUserId = value;
        }
    }
}
