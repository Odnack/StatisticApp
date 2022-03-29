using System.Collections.Generic;
using System.Threading.Tasks;
using Api.Api;
using Api.Api.Notifications.Application;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Statistic.Controllers
{
    [Route("Application")]
    [Authorize]
    public class NotificationApplicationController : BaseController, INotificationApplicationService
    {
        private INotificationApplicationService NotificationApplication => BackendApi.Notifications.Applications;
        public NotificationApplicationController(IBackendApi backendApi) : base(backendApi) { }

        [HttpGet]
        public IActionResult NewApplication()
        {
            return View();
        }
        [HttpPost]
        public IActionResult NewApplication(ApplicationAddingDto inData)
        {
            return View();
        }
        public IActionResult Statistic()
        {
            return View();
        }
        public async Task<ApplicationDto> AddNewApplication(ApplicationAddingDto inData)
        {
            return await NotificationApplication.AddNewApplication(inData);
        }

        public async Task<ApplicationDto> GetApplicationById(int applicationId)
        {
            return await NotificationApplication.GetApplicationById(applicationId);
        }

        public async Task<List<ApplicationDto>> GetApplicationsByUserId(int userId)
        {
            return await NotificationApplication.GetApplicationsByUserId(userId);
        }
    }
}