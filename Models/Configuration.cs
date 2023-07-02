using MongoDB.Bson;  
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;

namespace WebApiCSharp.Models
{
    public class Configuration
    {
        public string AOS_BasePath{ get; set; }
        public string SolverPath{ get; set; }
        public string SolverGraphPDF_DirectoryPath{ get; set; } 

        public string ML_ServerPath { get; set; }
        public string OpenAiGymEnvPath { get; set; }
    }
}