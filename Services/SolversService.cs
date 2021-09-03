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
    public class SolversService : ServiceBase
    {
        private static Solver GetSolverFromBson(BsonDocument doc)
        {
            Solver item = new Solver();

            item.SolverId = doc["SolverId"].AsInt32;
            item.ProjectName = doc["ProjectName"].ToString();
            item.ServerGeneratedSolverDateTime = doc["ServerGeneratedSolverDateTime"].IsBsonNull ? null : (DateTime?)doc["ServerGeneratedSolverDateTime"].ToUniversalTime();
            item.FirstSolverIsAliveDateTime = doc["FirstSolverIsAliveDateTime"].IsBsonNull ? null : (DateTime?)doc["FirstSolverIsAliveDateTime"].ToUniversalTime();
            item.SolverIsAliveDateTime = doc["SolverIsAliveDateTime"].IsBsonNull ? null : (DateTime?)doc["SolverIsAliveDateTime"].ToUniversalTime();
            item.ServerShutDownRequestDateTime = doc["ServerShutDownRequestDateTime"].IsBsonNull ? null : (DateTime?)doc["ServerShutDownRequestDateTime"].ToUniversalTime();
            return item;
        }
        public static IMongoCollection<BsonDocument> SolversCollectionBson = dbAOS.GetCollection<BsonDocument>(Globals.SOLVERS_COLLECTION_NAME);
        public static IMongoCollection<Solver> SolversCollection = dbAOS.GetCollection<Solver>(Globals.SOLVERS_COLLECTION_NAME);


        public static int GetNextNewSolverId()
        {
            int max = 0;
            foreach (Solver solver in Get())
            {
                max = max > solver.SolverId ? max : solver.SolverId;
            }
            return max + 1;
        }
        
        //return false if the solver is not in the DB
        public static bool StopOrStartSolver(int solverId, bool IsStopSolver, float planTimePerAction)
        {
            Solver solver = SolversCollection.Find<Solver>(
                Builders<Solver>.Filter.Eq("SolverId", solverId)).FirstOrDefault();
            bool solverExist = solver != null;
            if(solverExist)
            {
                FilterDefinition<BsonDocument> filter = Builders<BsonDocument>.Filter.Eq("SolverId", solverId);
                UpdateDefinition<BsonDocument> update = IsStopSolver ?
                     Builders<BsonDocument>.Update.Set("ServerShutDownRequestDateTime", DateTime.UtcNow) :
                     Builders<BsonDocument>.Update.Set("ServerShutDownRequestDateTime", (DateTime?)null)
                        .Set("FirstSolverIsAliveDateTime",(DateTime?)null)
                        .Set("SolverIsAliveDateTime",(DateTime?)null);
                SolversCollectionBson.UpdateOne(filter, update);

                if (IsStopSolver)
                {
                    int secondsSinceLastSolverIsAlive = solver.SolverIsAliveDateTime == null ? 10000 : Convert.ToInt32((DateTime.UtcNow - solver.SolverIsAliveDateTime.Value).TotalSeconds);
                    if (secondsSinceLastSolverIsAlive - 1 < planTimePerAction)
                    {
                        Thread.Sleep(Convert.ToInt32(planTimePerAction + 1) * 1000);//sleep until server should stop.
                    }
                }
            }
            
            return solverExist;
        }
        public static List<Solver> Get()
        {
             
            try
            {
                List<Solver> olResult = new List<Solver>();
                List<BsonDocument> results = SolversCollectionBson.Find<BsonDocument>(c => true).ToList();

                foreach (var doc in results)
                {
                    Solver item = GetSolverFromBson(doc);

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

        public static int RemoveAllSolversAndGetNextSolverID()
        {
            int nextId = GetNextNewSolverId();
            SolversCollectionBson.DeleteMany<BsonDocument>(c => true);

            return nextId;
        }
        public static Solver Get(int id)
        {
            /*try
            { 
                var c = SolversCollection.Find<BsonDocument>(c => c.SolverId == id).FirstOrDefault();
                return c == null ? null : GetSolverFromBson(c); 
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error:" + ex.Message);
                return null;
            }*/
            return null;
        }

        public static Solver Add(Solver item)
        {
            try
            {  
                SolversCollection.InsertOneAsync(item).GetAwaiter().GetResult();
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

        public static Solver Update(Solver item)
        {
            try
            { 
                var replaceResult = SolversCollection.ReplaceOne(doc => doc.SolverId == item.SolverId, item);
                if (replaceResult.IsAcknowledged)
                {
                    return item;
                }
                else
                {
                    return null;
                }
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

        public static bool Delete(Solver item)
        { 
            var result = SolversCollection.DeleteOne(doc => doc.SolverId == item.SolverId);
            return result.IsAcknowledged;
        }
    }
}