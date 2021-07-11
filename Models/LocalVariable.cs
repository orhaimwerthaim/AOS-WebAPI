using MongoDB.Bson;  
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;

namespace WebApiCSharp.Models
{
    public class LocalVariable
    {
        [BsonElement("Id")]
        public int Id{get;set;}


        [BsonElement("username")]
        public string Name{get;set;}
 

    }
}