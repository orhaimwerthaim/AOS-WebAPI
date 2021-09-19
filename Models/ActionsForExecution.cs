using MongoDB.Bson;  
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using System;
namespace WebApiCSharp.Models
{
    [BsonIgnoreExtraElements]
    public class ActionsForExecution
    {
        [BsonElement("_id")]
        public BsonObjectId Id{get;set;}


        public int ActionSequenceId{get;set;}


        public string ActionName{get;set;} 
        public DateTime? RequestCreateTime{get;set;}
        public string ParametersJson{get;set;}
    }
}