using Api.Api;
using DbLayer.Context;
using Microsoft.EntityFrameworkCore;

namespace BackendCore.Helpers
{
    public class ServiceSource
    {
        private readonly DbContextOptionsBuilder<DatabaseContext> _сontextOptionsBuilder;

        public ServiceSource(
            DbContextOptionsBuilder<DatabaseContext> сontextOptionsBuilder,
            string dataFolder)
        {
            _сontextOptionsBuilder = сontextOptionsBuilder;
            DataFolder = dataFolder;
        }

        public IBackendApi BackendApi { get; set; }

        public int UserId { get; set; } = -1;

        public DbContextOptionsBuilder<DatabaseContext> СontextOptionsBuilder => _сontextOptionsBuilder;

        public string DataFolder { get; }

        public DatabaseContext CreateDbContext()
        {
            var dbContext = new DatabaseContext(_сontextOptionsBuilder.Options);
            return dbContext;
        }
    }
}