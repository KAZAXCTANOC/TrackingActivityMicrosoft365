using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace TrackingActivityMicrosoft365.Models
{
    public class DataElementInfo
    {
        [BsonId]
        [BsonElement("_id")]
        public ObjectId Id { get; set; }
        [BsonElement("Data")]
        public string Data { get; set; }
        [BsonElement("LastView")]
        public string LastView { get; set; }
        [BsonElement("Changed")]
        public List<ChangedElement> Changed { get; set; }
    }
}
