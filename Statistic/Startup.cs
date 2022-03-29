using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Api.Api;
using BackendCore;
using DbLayer.Context;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;
using Statistic.Helpers;

namespace Statistic
{
    public class Startup
    {
        public Startup(IWebHostEnvironment environment)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(environment.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environment.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
            Environment = environment;
        }
        private AppSettings AppSettings { get; set; }
        private IWebHostEnvironment Environment { get; }
        public IConfiguration Configuration { get; }


        public void ConfigureServices(IServiceCollection services)
        {
            try
            {
                AppSettings = services.GetAppsettings(Configuration);
                services.AddDatabase(AppSettings);
                services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                    .AddCookie(options =>
                    {
                        options.LoginPath = new PathString("/Authentication/LogIn");
                        options.LogoutPath = new PathString("/Authentication/LogOut");
                    });
                services.AddControllersWithViews();
                ServicesExtension.AddLog(AppSettings);
                Log.Information("Web service started.");
                Log.Information(Environment.EnvironmentName);
                
                Log.Information("db.");
                
                services.AddScoped<IBackendApi, BackendCore.BackendCore>();
                services.AddSingleton<IContentTypeProvider>(new FileExtensionContentTypeProvider());

                services.AddCors();
            }
            catch (Exception ex)
            {
                Log.Error($"{ex.Message}\n{ex.StackTrace}");
            }
        }
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            var cookiePolicyOptions = new CookiePolicyOptions
            {
                MinimumSameSitePolicy = SameSiteMode.Strict,
            };
            app.UseCookiePolicy(cookiePolicyOptions);
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();
            app.UseAuthentication();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
