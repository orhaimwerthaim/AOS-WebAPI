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
        public static IMongoCollection<LogMessagePost> LogsCollectionPostObj = dbAOS.GetCollection<LogMessagePost>(Globals.LOGS_COLLECTION_NAME);

        

        public static List<LogMessage> Get()
        {
            try
            {
                List<LogMessage> olResult = new List<LogMessage>();
                List<BsonDocument> results = LogsCollection.Find<BsonDocument>(c => true).ToList();

                foreach (var doc in results)
                {
                    LogMessage item = new LogMessage();

                    item.ID =  GetElemenetObjectIDStr(doc);
                    item.LogLevel = GetElemenetInt(doc,"LogLevel");
                    item.LogLevelDesc = GetElemenetStr(doc,"LogLevelDesc");
                    item.Event = GetElemenetStr(doc,"Event");
                    item.Component = GetElemenetStr(doc,"Component");
                    item.Time = DateTimeToString(GetElemenetDateTime(doc,"Time"));//.ToLocalTime();
                    item.Advanced = GetElemenetStr(doc,"Advanced");//.ToLocalTime();
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
  
        public static void Add(LogMessagePost item)
        {
            try
            {  
                item._id = new ObjectId();
                item.Time = DateTime.Now;
                item.Advanced = item.Advanced == null ? "" : item.Advanced;
                item.Component = "Web-API";
                item.Event = item.Event == null ? "" : item.Event;
                LogsCollectionPostObj.InsertOneAsync(item).GetAwaiter().GetResult();
            }
            catch (MongoWriteException mwx)
            {
                if (mwx.WriteError.Category == ServerErrorCategory.DuplicateKey)
                {
                    // mwx.WriteError.Message contains the duplicate key error message
                }
            }
        }


        public static bool DeleteAll()
        { 
            var result = LogsCollection.DeleteMany(c=>true);
            return result.IsAcknowledged;
        }
    }
}