using MongoDB.Bson;  
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using System.Collections.Generic;

namespace WebApiCSharp.Models
{
    public class PLP
    { 
          public string Name{get;set;}

          public string Type{ get; set; }

          public string Project{ get; set; }

          public List<GlobalVariableModuleParameter> GlobalVariableModuleParameters { get; set; }

          public List<LocalVariablesInitializationFromGlobalVariable> LocalVariablesInitializationFromGlobalVariables{ get; set; }

          public List<string> EnumResponse{ get; set; }

          public string ResponseType{ get; set; } 

          public List<ResponseRule> ResponseRules{ get; set; }

        public string Preconditions_GlobalVariableConditionCode{ get; set; }
          public string Preconditions_PlannerAssistancePreconditions{ get; set; }

          public int Preconditions_ViolatingPreconditionPenalty{ get; set; }

          public List<Assignment> ModuleExecutionTimeDynamicModel{ get; set; }
          public List<Assignment> DynamicModel_VariableAssignments { get; set; }
        public PLP()
        {
            GlobalVariableModuleParameters = new List<GlobalVariableModuleParameter>();
            LocalVariablesInitializationFromGlobalVariables = new List<LocalVariablesInitializationFromGlobalVariable>();
            ModuleExecutionTimeDynamicModel = new List<Assignment>();
            DynamicModel_VariableAssignments = new List<Assignment>();
            ResponseRules = new List<ResponseRule>();
            EnumResponse = new List<string>(); 
        }

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

    public class GlobalVariableModuleParameter
    {
        public string Name{get;set;}

          public string Type{ get; set; } 

          public string UnderlineLocalVariableType{ get; set; }
    }

    public class LocalVariablesInitializationFromGlobalVariable
    {
        public string InputLocalVariable;
        public string FromGlobalVariable;
    }

    public class ResponseRule
    {
        public string Condition;
        public string Response;
        public string Comment;
    }

    public class Assignment
    {
        public string AssignmentName;
        public string AssignmentCode;

        public TempVarType TempVariable;

        public Assignment()
        {
            TempVariable = new TempVarType();
        }


    }

    public class TempVarType
    {
        public string Type;
        public string EnumName;
        public List<string> EnumValues;

        public TempVarType()
        {
            EnumValues = new List<string>();
        }
    }
}