using MongoDB.Bson;  
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;

namespace WebApiCSharp.Models
{
    public class PLP
    {
        public PLPMain plpMain{get;set;}

        public PLPMain a{get;set;}

        public PLPMain v{get;set;}


        [BsonElement("username")]
        public string Name{get;set;}
 

    }

    public class PLPMain
    {
        public string project{get;set;}
        public string name{get;set;}
        public string type{get;set;}
        public int version{get;set;}

    }

    public class EnvironmentGeneral
    {
        public string project{get;set;}
        public string name{get;set;}
        public string type{get;set;}
        public int version{get;set;}

    }
}