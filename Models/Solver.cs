using System;
using MongoDB.Bson;  
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;

namespace WebApiCSharp.Models
{
    [BsonIgnoreExtraElements]
    public class Solver
    {
        public int SolverId{ get; set; }
        public string ProjectName{ get; set; }
        public DateTime? ServerGeneratedSolverDateTime{ get; set; }
        public DateTime? FirstSolverIsAliveDateTime{ get; set; }
        public DateTime? SolverIsAliveDateTime{ get; set; }
        public DateTime? ServerShutDownRequestDateTime{ get; set; }
    }
}