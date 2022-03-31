using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
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
    public class NotificationApplicationController : BaseController, INotificationApplicationService
    {
        private INotificationApplicationService NotificationApplication => BackendApi.Notifications.Applications;
        public NotificationApplicationController(IBackendApi backendApi) : base(backendApi) { }

        [HttpGet]
        public IActionResult AddNewApplication()
        {
            if (User.Identity.IsAuthenticated)
                return View();
            return RedirectToAction("LogIn", "Authentication");
        }
        [HttpPost]
        public async Task<IActionResult> AddNewApplication(ApplicationAddingDto inData)
        {
            if (!User.Identity.IsAuthenticated)
                return View();
            inData.UserId = ClientId;
            var result = await NotificationApplication.AddNewApplication(inData);
            if (result is OkObjectResult okObject)
            {
                var application = okObject.Value as ApplicationDto;
                return View("Success", application);
            }
            return View("Error");
        }
        [HttpGet]
        [Route("Statistic")]
        public async Task<IActionResult> GetApplicationsByUserId(int userId)
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("LogIn", "Authentication");
            userId = ClientId;
            var result = await NotificationApplication.GetApplicationsByUserId(userId);

            if (result is OkObjectResult okObject)
            {
                var applications = okObject.Value as List<ApplicationDto>;
                var count = 0;
                foreach (var application in applications)
                {
                    count += application.Views;
                }

                ViewBag.ViewCount = count;
                return View("Statistic", applications);
            }
            return View("Error");
        }

        public async Task<int> GetApplicationCountByUserId(int userId)
        {
            return await NotificationApplication.GetApplicationCountByUserId(userId);
        }

        [HttpGet]
        [Route("Home")]
        public async Task<IActionResult> HomePage()
        {
            if (!User.Identity.IsAuthenticated) return View("HomePage");
            ViewBag.ApplicationCount = await GetApplicationCountByUserId(ClientId);
            return View("HomePage");
        }
    }
}