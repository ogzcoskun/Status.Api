using MongoDB.Bson.Serialization.Attributes;
using System.Net;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;

namespace Status.Models
{
    public class EndpointModel
    {
        [DataMember]
        [BsonElement("endpointId")]
        public string? EndpointId { get; set; }

        [DataMember]
        [BsonElement("url")]
        public string Url { get; set; }

        [DataMember]
        [BsonElement("method")]
        public string Method { get; set; }

        [DataMember]
        [BsonElement("parameters")]
        public Dictionary<string, string>? Parameters { get; set; }

        [DataMember]
        [BsonElement("expectedResponse")]
        public HttpStatusCode ExpectedResponse { get; set; }

        [DataMember]
        [BsonElement("frequency")]
        public int Frequency { get; set; }
    }
}
