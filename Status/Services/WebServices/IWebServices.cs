using Status.Models;
using Status.Models.SocketModel;

namespace Status.Services.WebServices
{
    public interface IWebServices
    {
        Task<ServiceResponse> PingServices(List<EndpointModel> endpoint);
        Task<List<FailedServicesResponse>> GetAllFailedServices();
        Task<List<FailedServicesResponse>> GetFailedServicesByDate(string start, string end);
        Task<ServiceResponse> AddEndpoint(EndpointModel endpoint);
        Task<ServiceResponse> Login();
        Task<ServiceResponse> ClearStatusHistory();
        Task<ServiceResponse> PingSocket(SocketInfoModel info);
        Task<List<EndpointDbModel>> GetAllEndpoints();
        Task<List<SocketStatus>> GetAllSockets();
    }
}
