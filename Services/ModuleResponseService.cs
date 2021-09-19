using System; 
using MongoDB.Driver;
using MongoDB.Bson;  
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using WebApiCSharp.Models;
using System.Collections.Generic;

namespace WebApiCSharp.Services
{
    public class ModuleResponseService : ServiceBase
    {
        public static IMongoCollection<ModuleResponse> ModuleResponseCollection = dbAOS.GetCollection<ModuleResponse>(Globals.MODULE_RESPONSES_COLLECTION_NAME);
        public static List<ModuleResponse> Get()
        {
            try
            {
                var result = ModuleResponseCollection.Find(x=> true).ToList();
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