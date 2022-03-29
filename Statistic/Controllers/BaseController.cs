using System.Security.Claims;
using Api.Api;
using Microsoft.AspNetCore.Mvc;
using Statistic.Helpers;

namespace Statistic.Controllers
{
    public class BaseController : Controller
    {
        private IBackendApi _backendApi;
        protected IBackendApi BackendApi
        {
            get
            {
                _backendApi.UserId = ClientId;
                return _backendApi;
            }
        }
        protected int ClientId
        {
            get
            {
                var clientId = HttpContext.User.GetId();
                return clientId;
            }
        }
        protected IBackendApi BackendApiAnonymous => _backendApi;

        public BaseController(IBackendApi backendApi)
        {
            _backendApi = backendApi;
        }
    }
}