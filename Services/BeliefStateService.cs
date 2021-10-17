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

public static int GetNumOfStatesSavedInCurrentBelief()
{
            var count = BeliefStateCollection.Aggregate(new AggregateOptions())
            .Match(Builders<BsonDocument>.Filter.Eq("ActionSequnceId", -1))
            .Project(new BsonDocument("count", new BsonDocument("$size", "$BeliefeState"))).FirstOrDefault();

            if(count == null)return 0;
            else
            {
                return count["count"].AsInt32;
            }
            /*
                        var filter = Builders<BsonDocument>.Filter.Eq("ActionSequnceId",-1);
                        var result = BeliefStateCollection.Find(filter).ToList();
                        if(result.Count == 0)return 0;
                        return 0;*/
        }

        public static List<BsonDocument> GetBeliefForExecution(int skip, int take, int? actionSequnceId = null)
        {
            try
            {
                //var filter = Builders<BsonDocument>.Filter.Eq("ActionSequnceId" , 1);
                var filter = actionSequnceId == null ? Builders<BsonDocument>.Filter.Empty : Builders<BsonDocument>.Filter.Eq("ActionSequnceId" , actionSequnceId.Value);
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

        public static List<BsonDocument> Get(int skip, int take, int? actionSequnceId = null)
        {
            try
            {
                var filter = actionSequnceId == null ? Builders<BsonDocument>.Filter.Empty : Builders<BsonDocument>.Filter.Eq("ActionSequnceId" , actionSequnceId.Value);
                //var filter = Builders<BsonDocument>.Filter.Eq("ActionSequnceId" , 1);
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