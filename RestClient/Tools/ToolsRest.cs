using System.Threading.Tasks;
using Api.Api.Tools;
using RestClient.Helpers;

namespace RestClient.Tools
{
    public class ToolsRest : IToolsService
    {
        public const string ServiceUrl = "Tools";

        private readonly RestHelper _helper;

        public ToolsRest(RestHelper helper)
        {
            _helper = helper;
        }
        public async Task InitDbData()
        {
            await _helper.ExecutePostAnonymous<object>(ServiceUrl, "InitDbData", false);
        }

        public Task<bool> IsServerAvailable()
        {
            throw new System.NotImplementedException();
        }
    }
}