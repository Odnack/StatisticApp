using System.Threading.Tasks;

namespace Api.Api.Tools
{
    public interface IToolsService
    {
        Task InitDbData();
        Task<bool> IsServerAvailable();
    }
}