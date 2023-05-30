using System;
using MongoDB.Bson;  
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;

namespace WebApiCSharp.Models
{ 
    [BsonIgnoreExtraElements]
    public class LogMessage
    {
        public string ID{ get; set; }
        public int LogLevel{ get; set; }
        public string LogLevelDesc{ get; set; }
        public string Event{ get; set; }
        public string Advanced { get; set; }
        public string Component{ get; set; }
      
        public string Time{ get; set; }
    }

    public class LogMessagePost
    {
        public ObjectId _id{ get; set; }
        public int LogLevel{ get; set; }
        public string LogLevelDesc{ get; set; }
        public string Event{ get; set; }
        public string Advanced { get; set; }
        public string Component{ get; set; }
      
        public DateTime Time{ get; set; }
    }
}