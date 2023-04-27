using System; 
using System.Threading; 
using MongoDB.Driver;
using MongoDB.Bson;  
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using WebApiCSharp.Models;
using System.Collections.Generic;

namespace WebApiCSharp.Services
{
    public class LogMessageService : ServiceBase
    { 
        public static IMongoCollection<BsonDocument> LogsCollection = dbAOS.GetCollection<BsonDocument>(Globals.LOGS_COLLECTION_NAME);
        public static IMongoCollection<LogMessage> LogsCollectionObj = dbAOS.GetCollection<LogMessage>(Globals.LOGS_COLLECTION_NAME);

        public static List<LogMessage> Get()
        {
            try
            {
                List<LogMessage> olResult = new List<LogMessage>();
                List<BsonDocument> results = LogsCollection.Find<BsonDocument>(c => true).ToList();

                foreach (var doc in results)
                {
                    LogMessage item = new LogMessage();

                    item.ID = doc["_id"].AsObjectId.ToString();
                    item.LogLevel = doc["LogLevel"].AsInt32;
                    item.LogLevelDesc = doc["LogLevelDesc"].ToString();
                    item.Event = doc["Event"].ToString();
                    item.Component = doc["Component"].ToString();
                    item.Time = doc["Time"].ToUniversalTime();//.ToLocalTime();
                    olResult.Add(item);
                }
                return olResult;

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error:" + ex.Message);
                return null;
            }
        }
  
        public static LogMessage Add(LogMessage item)
        {
            try
            {  
                LogsCollectionObj.InsertOneAsync(item).GetAwaiter().GetResult();
                return item;
            }
            catch (MongoWriteException mwx)
            {
                if (mwx.WriteError.Category == ServerErrorCategory.DuplicateKey)
                {
                    // mwx.WriteError.Message contains the duplicate key error message
                }
                return null;
            }
        }


        public static bool DeleteAll()
        { 
            var result = LogsCollection.DeleteMany(c=>true);
            return result.IsAcknowledged;
        }
    }
}