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
    public class PLPsService : ServiceBase
    {
        public static IMongoCollection<BsonDocument> PLPsCollection = dbAOS.GetCollection<BsonDocument>(Globals.PLPS_COLLECTION_NAME);
        public static List<BsonDocument> GetAll()
        {
            try
            {

                var c = PLPsCollection.Find<BsonDocument>(c => true).ToList();
                return c;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error:" + ex.Message);
                return null;
            }
        }

        public static BsonDocument Get(int id)
        {
            try
            {
                var c = PLPsCollection.Find<BsonDocument>(c => true).FirstOrDefault();
                return c;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error:" + ex.Message);
                return null;
            }
        }

        public static BsonDocument Add(JsonDocument item)
        {
            try
            {
                //var document = BsonSerializer.Deserialize<BsonDocument>(text);
                //var collection = db.GetCollection<BsonDocument>(collectionName);
                string jsonText = null;
                using (var stream = new MemoryStream())
                {
                    Utf8JsonWriter writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true });
                    item.WriteTo(writer);
                    writer.Flush();
                    jsonText = Encoding.UTF8.GetString(stream.ToArray());
                }
                if (jsonText == null)
                {
                    throw new Exception("Cannot convert json to string");
                }
                BsonDocument document = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonDocument>(jsonText);

                string a = document["PlpMain"]["Project"].ToString();
                string b = document["PlpMain"]["Type"].ToString();
                string c = document["PlpMain"]["Name"].ToString();
                PLPsCollection.ReplaceOne(doc => doc["PlpMain"]["Project"].Equals(document["PlpMain"]["Project"]) &&
                 doc["PlpMain"]["Name"].Equals(document["PlpMain"]["Name"]) &&
                 doc["PlpMain"]["Type"].Equals(document["PlpMain"]["Type"]), document, new ReplaceOptions { IsUpsert = true });
                return document;

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

        public static JsonDocument Update(JsonDocument item)
        {/*
            try
            { 
                var replaceResult = PLPsCollection.ReplaceOne(doc => true, item);
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
            }*/
            return null;
        }

        public static bool DeleteAll()
        {
            var result = PLPsCollection.DeleteMany(doc => true);
            return result.IsAcknowledged;
        }
    }
}