using System.Threading.Tasks;
using Api.Api.Tools;
using BackendCore.Helpers;

namespace BackendCore.Tools
{
    public class ToolsService : IToolsService
    {
        private readonly ServiceSource _serviceSource;
        public ToolsService(ServiceSource serviceSource)
        {
            _serviceSource = serviceSource;
        }
        public async Task InitDbData()
        {
            await BackendCore.SetupIfNeed(_serviceSource.СontextOptionsBuilder, _serviceSource.DataFolder);
        }

        public async Task<bool> IsServerAvailable()
        {
            return await Task.Run(() => true);
        }
    }
}