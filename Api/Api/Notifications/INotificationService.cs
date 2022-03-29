using Api.Api.Notifications.Application;

namespace Api.Api.Notifications
{
    public interface INotificationService
    {
        INotificationApplicationService Applications { get; }
    }
}