using System; 
using MongoDB.Driver;
using MongoDB.Bson;  
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using WebApiCSharp.Models;
using System.Collections.Generic;

namespace WebApiCSharp.Services
{
    public class ManualActionsService : ServiceBase
    {
        public static IMongoCollection<ManualAction> ManualActionsCollection = dbAOS.GetCollection<ManualAction>(Globals.MANUAL_ACTIONS_COLLECTION_NAME);
        public static List<ManualAction> Get()
        {
            try
            {
                var result = ManualActionsCollection.Find(x=> true).ToList();
                return result; 
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error:" + ex.Message);
                return null;
            }
        }

         public static ManualAction Add(ManualAction item)
        {
            try
            {  
                ManualActionsCollection.InsertOneAsync(item).GetAwaiter().GetResult();
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
            var result = ManualActionsCollection.DeleteMany(c=>true);
            return result.IsAcknowledged;
        }

    }
}