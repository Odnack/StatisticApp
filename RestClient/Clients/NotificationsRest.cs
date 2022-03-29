using Api.Api.Notifications;
using Api.Api.Notifications.Application;
using RestClient.Helpers;

namespace RestClient
{
    public class NotificationsRest : INotificationService
    {
        public INotificationApplicationService Applications { get; }
        public NotificationsRest(RestHelper restHelper)
        {
            Applications = new NotificationApplicationRest(restHelper);
        }
    }
}