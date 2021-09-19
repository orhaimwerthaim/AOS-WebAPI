using System; 
using MongoDB.Driver;
using MongoDB.Bson;  
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using WebApiCSharp.Models;
using System.Collections.Generic;

namespace WebApiCSharp.Services
{
    public class ActionsForExecutionService : ServiceBase
    {
        public static IMongoCollection<BsonDocument> ActionsForExecutionCollection = dbAOS.GetCollection<BsonDocument>(Globals.ACTIONS_FOR_EXECUTION_COLLECTION_NAME);
        public static List<BsonDocument> Get()
        {
            try
            {
                var filter = Builders<BsonDocument>.Filter.Empty; 
                var result = ActionsForExecutionCollection.Find(filter).ToList();
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error:" + ex.Message);
                return null;
            }
        } 
    }
}