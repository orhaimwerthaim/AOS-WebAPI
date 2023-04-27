using System;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using WebApiCSharp.Models;
using System.Collections.Generic;
using System.Text.Json;
using System.Text;
using System.IO;
namespace WebApiCSharp.Services
{
    public class AosGeneralService : ServiceBase
    {
        public static void DeleteCollectionsBeforeProjectInitialization()
        {
            try
            {
                dbAOS.GetCollection<BsonDocument>(Globals.PLPS_COLLECTION_NAME).DeleteMany(doc => true);
                dbAOS.GetCollection<BsonDocument>(Globals.ACTIONS_COLLECTION_NAME).DeleteMany(doc => true);
                dbAOS.GetCollection<BsonDocument>(Globals.ACTIONS_FOR_EXECUTION_COLLECTION_NAME).DeleteMany(doc => true);
                dbAOS.GetCollection<BsonDocument>(Globals.MODULE_RESPONSES_COLLECTION_NAME).DeleteMany(doc => true);
                dbAOS.GetCollection<BsonDocument>(Globals.LOCAL_VARIABLES_COLLECTION_NAME).DeleteMany(doc => true);
                dbAOS.GetCollection<BsonDocument>(Globals.GLOBAL_VARIABLES_ASSIGNMENTS_COLLECTION_NAME).DeleteMany(doc => true);
                dbAOS.GetCollection<BsonDocument>(Globals.LOGS_COLLECTION_NAME).DeleteMany(doc => true); 
                dbAOS.GetCollection<object>(Globals.BELIEF_STATES_COLLECTION_NAME).DeleteMany("{\"ActionSequnceId\": { $ne: -1 } }");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
 
    }
}