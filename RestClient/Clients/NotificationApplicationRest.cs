using System;
using Api.Api.Notifications.Application;
using RestClient.Helpers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RestClient
{
    internal class NotificationApplicationRest : INotificationApplicationService
    {
        public const string ServiceUrl = "NotificationDialog";

        private readonly RestHelper _helper;

        public NotificationApplicationRest(RestHelper helper)
        {
            _helper = helper;
        }

        public async Task<ApplicationDto> AddNewApplication(ApplicationAddingDto inData)
        {
            return await _helper.ExecutePost<ApplicationDto>(ServiceUrl, "AddNewApplication", inData, true);
        }

        public async Task<ApplicationDto> GetApplicationById(int applicationId)
        {
            return await _helper.ExecuteGet<ApplicationDto>(ServiceUrl, "GetApplicationById", new Tuple<string, string>("applicationId", applicationId.ToString()));
        }

        public async Task<List<ApplicationDto>> GetApplicationsByUserId(int userId)
        {
            return await _helper.ExecuteGet<List<ApplicationDto>>(ServiceUrl, "GetApplicationByUserId", new Tuple<string, string>("userId", userId.ToString()));
        }
    }
}