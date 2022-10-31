namespace Status.Models.SocketModel
{
    public class NegotiationModel
    {
        public int NegotiateVersion { get; set; }
        public string? Url { get; set; }
        public string? AccessToken { get; set; }
        public List<object>? AvailableTransports { get; set; }
    }
}
