using System.Collections.Generic;
using System.Threading.Tasks;

namespace Api.Api.Notifications.Application
{
    public interface INotificationApplicationService
    {
        Task<ApplicationDto> AddNewApplication(ApplicationAddingDto inData);
        Task<ApplicationDto> GetApplicationById(int applicationId);
        Task<List<ApplicationDto>> GetApplicationsByUserId(int userId);
    }
}