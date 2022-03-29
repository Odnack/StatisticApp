using System;
using System.Linq;
using System.Threading.Tasks;
using Api.Api;
using Api.Api.Authentication;
using Api.Api.Notifications;
using Api.Api.Tools;
using BackendCore.Authentication;
using BackendCore.Helpers;
using BackendCore.Notifications;
using BackendCore.Tools;
using DbLayer.Context;
using Microsoft.EntityFrameworkCore;

namespace BackendCore
{
    public class BackendCore : IBackendApi
    {
        public class DataFolderProvider
        {
            public string DataFolder { get; set; }
        }
        private readonly ServiceSource _serviceSource;
        private readonly Lazy<AuthenticationService> _internalAuthentication;
        private readonly Lazy<NotificationService> _internalNotification;
        private readonly Lazy<ToolsService> _tools;
        public IAuthenticationService Authentication => _internalAuthentication.Value;
        public IToolsService Tools => _tools.Value;
        public AuthenticationService InternalAuthentication => _internalAuthentication.Value;
        public INotificationService Notifications => _internalNotification.Value;

        public int UserId { get => _serviceSource.UserId; set => _serviceSource.UserId = value; }
        public BackendCore(
            DbContextOptionsBuilder<DatabaseContext> сontextOptionsBuilder,
            DataFolderProvider dataFolderProvider)
            : this(сontextOptionsBuilder, dataFolderProvider.DataFolder)
        {
        }
        public BackendCore(
            DbContextOptionsBuilder<DatabaseContext> сontextOptionsBuilder,
            string dataFolder)
        {
            if (dataFolder.Length != 0)
            {
                var lastChar = dataFolder.Last();
                if (lastChar != '\\' && lastChar != '/')
                    dataFolder += "/";
            }

            _serviceSource = new ServiceSource(сontextOptionsBuilder, dataFolder)
            {
                BackendApi = this
            };

            _internalAuthentication = new Lazy<AuthenticationService>(() => new AuthenticationService(_serviceSource));
            _internalNotification = new Lazy<NotificationService>(() => new NotificationService(_serviceSource));
            _tools = new Lazy<ToolsService>(() => new ToolsService(_serviceSource));
        }
        public static async Task SetupIfNeed(
            DbContextOptionsBuilder<DatabaseContext> сontextOptionsBuilder,
            string dataFolder)
        {
            //ProjectTransactionHelper.Get().SetTransactionTimeoutByDefault();

            var thisObj = new BackendCore(сontextOptionsBuilder, dataFolder);
            await DefaultDbCreator.Setup(thisObj);
        }
    }
}