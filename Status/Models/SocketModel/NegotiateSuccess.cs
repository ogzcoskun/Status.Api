using Microsoft.AspNetCore.Http.Connections;

namespace Status.Models.SocketModel
{
    public class NegotiateSuccess
    {
        public int CVersion { get; set; }
        public string ConnectionId { get; set; }
        public List<AvailableTransport> AvailableTransports { get; set; }
    }
}
