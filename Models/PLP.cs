using MongoDB.Bson;  
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using System.Collections.Generic;

namespace WebApiCSharp.Models
{
    public class PLP:ModuleDocumentationFile
    { 


          public List<GlobalVariableModuleParameter> GlobalVariableModuleParameters { get; set; }

          public List<LocalVariablesInitializationFromGlobalVariable> LocalVariablesInitializationFromGlobalVariables{ get; set; }

          public List<string> EnumResponse{ get; set; }

          public string ResponseType{ get; set; } 

          public List<ResponseRule> ResponseRules{ get; set; }

        public List<Assignment> Preconditions_GlobalVariablePreconditionAssignments{ get; set; }
          public List<Assignment> Preconditions_PlannerAssistancePreconditionsAssignments{ get; set; }

          public int Preconditions_ViolatingPreconditionPenalty{ get; set; }

          public List<Assignment> ModuleExecutionTimeDynamicModel{ get; set; }
          public List<Assignment> DynamicModel_VariableAssignments { get; set; }
        public PLP()
        {
            GlobalVariableModuleParameters = new List<GlobalVariableModuleParameter>();
            LocalVariablesInitializationFromGlobalVariables = new List<LocalVariablesInitializationFromGlobalVariable>();
            ModuleExecutionTimeDynamicModel = new List<Assignment>();
            DynamicModel_VariableAssignments = new List<Assignment>();
            Preconditions_GlobalVariablePreconditionAssignments = new List<Assignment>();
            Preconditions_PlannerAssistancePreconditionsAssignments = new List<Assignment>();
            ResponseRules = new List<ResponseRule>();
            EnumResponse = new List<string>(); 
        }

    }
  

    public class GlobalVariableModuleParameter
    {
        public string Name{get;set;}

          public string Type{ get; set; } 

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
        public List<ResponseAssignmentToGlobalVar> ResponseAssignmentsToGlobalVar;

        public ResponseRule()
        {
            ResponseAssignmentsToGlobalVar = new List<ResponseAssignmentToGlobalVar>();
        }
    }

    public class ResponseAssignmentToGlobalVar
    {
        public string GlobalVarName;
        public string Value;
    }

    public class Assignment
    {
        public string AssignmentName;
        public string AssignmentCode;

        public TempVar TempVariable;

        public Assignment()
        {
            TempVariable = new TempVar();
        }


    }

    public class TempVar
    {
        public string Type;
        public string VariableName;
        public string EnumName;
        public List<string> EnumValues;

        public TempVar()
        {
            EnumValues = new List<string>();
        }
    }
}