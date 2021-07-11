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
    }
}