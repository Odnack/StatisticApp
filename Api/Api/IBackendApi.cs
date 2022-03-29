using Api.Api.Authentication;
using Api.Api.Notifications;
using Api.Api.Tools;

namespace Api.Api
{
    public interface IBackendApi
    {
        IAuthenticationService Authentication { get; }
        IToolsService Tools { get; }
        INotificationService Notifications { get; }
        int UserId { get; set; }
    }
}