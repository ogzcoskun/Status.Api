using System.Globalization;

namespace Status.Models.SocketModel
{
    public class SocketInfoModel
    {
        //(string url, string hubGroup, string id

        public string Url { get; set; }
        public string HubGroup { get; set; }
        public List<string> IdList { get; set; }
        public string SocketName { get; set; }
        public int ReturnNum { get; set; }

    }
}
