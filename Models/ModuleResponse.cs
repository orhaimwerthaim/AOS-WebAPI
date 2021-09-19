using MongoDB.Bson;  
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using System;
namespace WebApiCSharp.Models
{
    [BsonIgnoreExtraElements]
    public class ModuleResponse
    {
        [BsonElement("_id")]
        public BsonObjectId Id{get;set;}


        public int ActionSequenceId{get;set;}


        public string Module{get;set;}
        public string ModuleResponseText{get;set;}
        public DateTime? StartTime{get;set;}
        public BsonObjectId ActionForExecutionId{get;set;}
        public DateTime? EndTime{get;set;} 
    }
}