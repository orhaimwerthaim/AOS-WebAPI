using System; 
using MongoDB.Driver;
using MongoDB.Bson;  
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using WebApiCSharp.Models;
using System.Collections.Generic;

namespace WebApiCSharp.Services
{
    public class LocalVariableService : ServiceBase
    {
        public static IMongoCollection<LocalVariable> LocalVarCollection = dbAOS.GetCollection<LocalVariable>(Globals.LOCAL_VARIABLES_COLLECTION_NAME);
        public static List<LocalVariable> Get()
        {
            try
            {
                // var connString = "mongodb://127.0.0.1:27017";
                // MongoClient client = new MongoClient(connString);
                // IMongoDatabase db = client.GetDatabase("AOS"); 
                //var cars2 = db.GetCollection<BsonDocument>("localVariables");

                var c = LocalVarCollection.Find<LocalVariable>(c=> true).ToList();
                return c;
                //   foreach(LocalVariable a in c.ToList())
                //   {
                //         string s =a.Name;
                //   }
                //    // cars2.InsertOne((new Car().ToBsonDocument()));
                //     // List all the MongoDB databases
                //     var allDatabases = client.ListDatabases().ToList();

                //     Console.WriteLine("MongoDB db array type: " + allDatabases.GetType());
                //     Console.WriteLine("MongoDB databases:");
                //     Console.WriteLine(string.Join(", ", allDatabases));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error:" + ex.Message);
                return null;
            }
        }

        public static LocalVariable Get(int id)
        {
            try
            {
                // var connString = "mongodb://127.0.0.1:27017";
                // MongoClient client = new MongoClient(connString);
                // IMongoDatabase db = client.GetDatabase("AOS"); 
                //var cars2 = db.GetCollection<BsonDocument>("localVariables");

                // var c = LocalVarCollection.Find<LocalVariable>(c => c.Id == id).FirstOrDefault();
                // return c;
                return null;
                //   foreach(LocalVariable a in c.ToList())
                //   {
                //         string s =a.Name;
                //   }
                //    // cars2.InsertOne((new Car().ToBsonDocument()));
                //     // List all the MongoDB databases
                //     var allDatabases = client.ListDatabases().ToList();

                //     Console.WriteLine("MongoDB db array type: " + allDatabases.GetType());
                //     Console.WriteLine("MongoDB databases:");
                //     Console.WriteLine(string.Join(", ", allDatabases));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error:" + ex.Message);
                return null;
            }
        }

        public static LocalVariable Add(LocalVariable item)
        {
            try
            { 
                //var cars2 = db.GetCollection<BsonDocument>("localVariables");
                LocalVarCollection.InsertOneAsync(item).GetAwaiter().GetResult();
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

        public static LocalVariable Update(LocalVariable item)
        {
            try
            { 
                // var replaceResult = LocalVarCollection.ReplaceOne(doc => doc.Id == item.Id, item);
                // if (replaceResult.IsAcknowledged)
                // {
                //     return item;
                // }
                // else
                // {
                //     return null;
                // }
                return null;
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

        public static bool Delete(LocalVariable item)
        { 
            // var result = LocalVarCollection.DeleteOne(doc => doc.Id == item.Id);
            // return result.IsAcknowledged;
            return true;
        }
    }
}