using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackingActivityMicrosoft365.Models;

namespace TrackingActivityMicrosoft365.MongoDB
{
    public class MongoDBController
    {
        private readonly IMongoClient _mongoClient;

        private const string UserName = "dataGetter";
        private const string DBName = "Office365";
        private const string Password = "6BnPudyx7KVLmiZ";

        public MongoDBController()
        {
            _mongoClient = new MongoClient(MongoUri);
        }

        public static string MongoUri => $"mongodb+srv://{UserName}:{Password}@cluster0.xuex9.mongodb.net/{DBName}?retryWrites=true&w=majority";

        public List<DataElementInfo> GetCollection(string CollectionName)
        {
            var database = _mongoClient.GetDatabase(DBName);
            var Collection = database.GetCollection<DataElementInfo>(CollectionName);
            var result = Collection.Find(_ => true);

            return result.ToList();
        }

        public void CreateElemetInfo(DataElementInfo elementInfo, string CollectionName)
        {
            var database = _mongoClient.GetDatabase(DBName);
            var collection = database.GetCollection<DataElementInfo>(CollectionName);
            collection.InsertOne(elementInfo);
        }
    }
}
