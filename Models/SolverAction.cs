using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WebApiCSharp.Models
{
    public class SolverAction
    { 
        public int ActionID { get; set; }

        public string ActionDescription { get; set; }
 
        public string ActionName { get; set; }

        public List<ActionConstantParameter> ActionConstantParameters { get; set; }

    }

    public class ActionConstantParameter
    {
        public string ParameterName{ get; set; }
        public string Value{ get; set; }
    }
}