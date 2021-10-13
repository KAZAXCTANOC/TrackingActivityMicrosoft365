using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PulseCosts.Models
{
    public class MongoDbElement
    {
        [BsonId]
        [BsonElement("_id")]
        public ObjectId Id { get; set; }
        [BsonElement("Data")]
        public string Data { get; set; }
    }
}
