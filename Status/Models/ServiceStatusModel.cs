namespace Status.Models
{
    public class ServiceStatusModel
    {
        
        public string? ServiceId { get; set; }
        public string? ServiceName { get; set; }
        public bool? IsRunning { get; set; }
        public DateTime? Date { get; set; }
        public string? StatusMessage { get; set; }
    }
}
