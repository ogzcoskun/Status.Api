using Status.Models;

namespace Status.Services.MessagingServices
{
    public interface IMessageService
    {
        Task SendTelegramMessage(ServiceStatusModel status); 
    }
}
