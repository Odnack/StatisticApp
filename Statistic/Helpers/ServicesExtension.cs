using System;
using System.Text;
using System.Threading.Tasks;
using BackendCore;
using DbLayer.Context;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;


namespace Statistic.Helpers
{
    public static class ServicesExtension
    {
        private static DbContextOptionsBuilder SetupDbOptions(
            string connectionString,
            DatabaseType databaseType,
            DbContextOptionsBuilder optionsBuilder)
        {
            switch (databaseType)
            {
                case DatabaseType.Postgres:
                    return optionsBuilder.UseNpgsql(connectionString);
                default:
                    return optionsBuilder;
            }
        }
        private static async Task SetupDatabase(DbContextOptionsBuilder<DatabaseContext> optionsBuilder, string dataFolder)
        {
            await BackendCore.BackendCore.SetupIfNeed(optionsBuilder, dataFolder);
        }
        public static void AddDatabase(this IServiceCollection services, AppSettings appSettings)
        {
            var connectionString = appSettings.Database.ConnectionString;
            Log.Information("Db connection string: " + connectionString);
            var databaseType = appSettings.Database.Type;
            Log.Information("databaseType: " + databaseType);
            Log.Information("dataFolder: " + appSettings.Database.DataFolder);
            services.Configure<FormOptions>(options =>
            {
                options.ValueLengthLimit = int.MaxValue;
                options.MultipartHeadersLengthLimit = int.MaxValue;
                options.MultipartBodyLengthLimit = int.MaxValue;
            });

            services.AddSingleton(s =>
            {
                var builder = new DbContextOptionsBuilder<DatabaseContext>();
                SetupDbOptions(connectionString, databaseType, builder);
                return builder;
            });

            services.AddSingleton(s => new BackendCore.BackendCore.DataFolderProvider { DataFolder = appSettings.Database.DataFolder });

            services.AddScoped<DatabaseContext>();
            switch (databaseType)
            {
                case DatabaseType.Postgres:
                    services.AddDbContext<DatabaseContext>(options => SetupDbOptions(
                        connectionString,
                        databaseType,
                        options));
                    break;
            }
            var serviceProvider = services.BuildServiceProvider();
            var context = serviceProvider.GetService<DatabaseContext>();
            context.Database.Migrate();

            var optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>();
            SetupDbOptions(connectionString, databaseType, optionsBuilder);
            SetupDatabase(optionsBuilder, appSettings.Database.DataFolder).Wait();
        }
        public static AppSettings GetAppsettings(this IServiceCollection services, IConfiguration Configuration)
        {
            services.Configure<AppSettings>(Configuration);
            var serviceProvider = services.BuildServiceProvider();
            return serviceProvider.GetService<IOptions<AppSettings>>()?.Value;
        }

        public static void AddLog(AppSettings appSettings)
        {
            var logFileName = appSettings.Logging.FileName;
            var loggerConfiguration = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File(logFileName, rollingInterval: RollingInterval.Day);

            var minLevel = appSettings.Logging.MinLevel;
            switch (minLevel)
            {
                case LogLevel.Information:
                    loggerConfiguration = loggerConfiguration.MinimumLevel.Information();
                    break;
                case LogLevel.Debug:
                    loggerConfiguration = loggerConfiguration.MinimumLevel.Debug();
                    break;
                case LogLevel.Warning:
                    loggerConfiguration = loggerConfiguration.MinimumLevel.Warning();
                    break;
                case LogLevel.Error:
                    loggerConfiguration = loggerConfiguration.MinimumLevel.Error();
                    break;
                case LogLevel.Fatal:
                    loggerConfiguration = loggerConfiguration.MinimumLevel.Fatal();
                    break;
            }

            Log.Logger = loggerConfiguration.CreateLogger();
        }
    }
}