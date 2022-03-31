using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Api.Api.Notifications.Application
{
    public interface INotificationApplicationService
    {
        Task<IActionResult> AddNewApplication(ApplicationAddingDto inData);
        Task<IActionResult> GetApplicationsByUserId(int userId);
        Task<int> GetApplicationCountByUserId(int userId);
    }
}