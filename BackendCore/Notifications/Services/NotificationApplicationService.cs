using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Api.Api.Notifications.Application;
using BackendCore.Helpers;
using DbLayer.Context;
using DbLayer.Tables.Public;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendCore.Notifications.Services
{
    public class NotificationApplicationService : INotificationApplicationService
    {
        private readonly ServiceSource _serviceSource;
        public NotificationApplicationService(ServiceSource serviceSource)
        {
            _serviceSource = serviceSource;
        }

        private ApplicationDto DtoFromApplication(DatabaseContext dbContext, Application inData)
            => new ApplicationDto()
            {
                Id = inData.Id,
                CreationDate = inData.CreationDate,
                UserId = inData.UserId,
                Name = inData.Name,
                Description = inData.Description,
                Views = inData.Views
            };
        public async Task<IActionResult> AddNewApplication(ApplicationAddingDto inData)
        {
            var dbContext = _serviceSource.CreateDbContext();

            var newApplication = dbContext.Applications.Add(new Application()
            {
                Name = inData.Name,
                CreationDate = DateTime.UtcNow,
                UserId = inData.UserId,
                Description = inData.Description,
                Views = 0
            }).Entity;

            await dbContext.SaveChangesAsync();

            var result = DtoFromApplication(dbContext, newApplication);

            return new OkObjectResult(result);
        }
        
        public async Task<int> GetApplicationCountByUserId(int userId)
        {
            var dbContext = _serviceSource.CreateDbContext();
            var count = await dbContext.Applications
                .Where(utd => utd.UserId == userId)
                .CountAsync();
            return count;
        }
        public async Task<IActionResult> GetApplicationsByUserId(int userId)
        {
            var dbContext = _serviceSource.CreateDbContext();
            var applicationList = await dbContext.Applications
                .Where(utd => utd.UserId == userId)
                .ToListAsync();

            var result = new List<ApplicationDto>();

            foreach (var application in applicationList)
                result.Add(DtoFromApplication(dbContext, application));

            return new OkObjectResult(result);
        }
    }
}