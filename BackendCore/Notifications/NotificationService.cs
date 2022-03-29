using System;
using Api.Api.Notifications;
using Api.Api.Notifications.Application;
using BackendCore.Helpers;
using BackendCore.Notifications.Services;

namespace BackendCore.Notifications
{
    public class NotificationService : INotificationService
    {
        private readonly Lazy<NotificationApplicationService> _applications;

        public NotificationService(ServiceSource serviceSource)
        {
            _applications = new Lazy<NotificationApplicationService>(() => new NotificationApplicationService(serviceSource));
        }

        public INotificationApplicationService Applications => _applications.Value;
    }
}