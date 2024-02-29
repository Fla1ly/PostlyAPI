using MongoDB.Bson;
using MongoDB.Driver;

namespace c_ApiLayout.Utilities
{
    public class Log
    {
        public static void LogEvent(IMongoCollection<BsonDocument> logCollection, string username)
        {
            var log = new BsonDocument
         {
             { "username", username },
         };
            logCollection.InsertOne(log);
        }
    }
}
