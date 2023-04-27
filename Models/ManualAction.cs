using System;
using MongoDB.Bson;  
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;

namespace WebApiCSharp.Models
{ 
    [BsonIgnoreExtraElements]
    public class ManualAction
    {
        public int ActionID{ get; set; }
    }
}