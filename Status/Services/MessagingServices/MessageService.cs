using MongoDB.Bson.IO;
using Newtonsoft.Json;
using Status.Models;
using Telegram.Bot;
using Telegram.Bot.Types;
using JsonConvert = Newtonsoft.Json.JsonConvert;

namespace Status.Services.MessagingServices
{
    public class MessageService : IMessageService
    {
        private readonly IConfiguration _config;

        public MessageService(IConfiguration config)
        {
            _config = config;
        }
        public async Task SendTelegramMessage(ServiceStatusModel status)
        {
            TelegramBotClient bot = new TelegramBotClient(_config["Telegram:Key"]);

            var errorMessage = $"ServiceId: {status.ServiceId} \n\nServiceName: {status.ServiceName}\n\nIsRunning: {status.IsRunning}\n\nData: {status.Date}\n\nMessage: {status.StatusMessage}"; 

            await bot.SendTextMessageAsync(_config["Telegram:ChatId"], errorMessage);

        }
    }
}
