using System; 
using MongoDB.Driver;
using MongoDB.Bson;  
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using WebApiCSharp.Models;
using System.Collections.Generic;

namespace WebApiCSharp.Services
{
    public class SimulatedStateService : ServiceBase
    {
        public static IMongoCollection<BsonDocument> SimulatedStateCollection = dbAOS.GetCollection<BsonDocument>(Globals.SIMULATED_STATES_COLLECTION_NAME);


        public static List<BsonDocument> Get()
        {
            try
            { 
                var result = SimulatedStateCollection.Find(Builders<BsonDocument>.Filter.Empty).ToList();
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