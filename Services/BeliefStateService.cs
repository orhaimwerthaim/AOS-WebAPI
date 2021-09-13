using System; 
using MongoDB.Driver;
using MongoDB.Bson;  
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using WebApiCSharp.Models;
using System.Collections.Generic;

namespace WebApiCSharp.Services
{
    public class BeliefStateService : ServiceBase
    {
        public static IMongoCollection<BsonDocument> BeliefStateCollection = dbAOS.GetCollection<BsonDocument>(Globals.BELIEF_STATES_COLLECTION_NAME);
        public static BsonDocument GetOne(int skip, int take)
        {
            try
            {
                var filter = Builders<BsonDocument>.Filter.Eq("ActionSequnceId" , 1);
                var project = Builders<BsonDocument>.Projection.Slice("BeliefeState", skip, take);
                var result = BeliefStateCollection.Find(filter).Project(project).FirstOrDefault();
                return result; 
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error:" + ex.Message);
                return null;
            }
        }

        public static List<BsonDocument> Get(int skip, int take)
        {
            try
            {
                var filter = Builders<BsonDocument>.Filter.Eq("ActionSequnceId" , 1);
                var project = Builders<BsonDocument>.Projection.Slice("BeliefeState", skip, take);
                var result = BeliefStateCollection.Find(filter).Project(project).ToList();
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