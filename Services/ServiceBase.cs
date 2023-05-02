using System;
using MongoDB.Driver;
using MongoDB.Bson;  
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using WebApiCSharp.Models;
using System.Collections.Generic;

namespace WebApiCSharp.Services
{
    public class ServiceBase
    {
        public static IMongoDatabase dbAOS = null;
        static ServiceBase()
        {
            var connString = "mongodb://127.0.0.1:27017";
            MongoClient client = new MongoClient(connString);
            dbAOS = client.GetDatabase("AOS");
        }

        public static string GetElemenetStr(BsonDocument doc, string field)
        {
            if(doc.Contains(field)) 
                    return doc[field].ToString();
            else return "";
        }

        public static string GetElemenetObjectIDStr(BsonDocument doc, string field="_id")
        {
            if(doc.Contains(field)) 
                    return doc[field].AsObjectId.ToString();
            else return "";
        }

        

        public static DateTime GetElemenetDateTime(BsonDocument doc, string field)
        {
            if(doc.Contains(field)) 
                    return doc[field].ToLocalTime();
            else return new DateTime();
        }

        public static int GetElemenetInt(BsonDocument doc, string field)
        {
            if(doc.Contains(field)) 
                    return doc[field].AsInt32;
            else return -1;
        }
    }
}