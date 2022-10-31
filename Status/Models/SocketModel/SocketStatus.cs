using MongoDB.Bson.Serialization.Attributes;
using System.Runtime.Serialization;

namespace Status.Models.SocketModel
{
    public class SocketStatus
    {
        //[DataMember]
        //[BsonElement("_id")]
        //public object? _id { get; set; }

        [DataMember]
        [BsonElement("endpointId")]
        public string EndpointId { get; set; }

        [DataMember]
        [BsonElement("socket")]
        public string Socket { get; set; }

        [DataMember]
        [BsonElement("lastUpdate")]
        public string LastUpdate { get; set; }
    }
}
