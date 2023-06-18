using MongoDB.Bson;  
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using System.Collections.Generic;

namespace WebApiCSharp.Models
{
    public class PLP:ModuleDocumentationFile
    {  
          public List<GlobalVariableModuleParameter> GlobalVariableModuleParameters { get; set; } 
        public List<Assignment> Preconditions_GlobalVariablePreconditionAssignments{ get; set; }

        public List<Assignment> PossibleParametersValue{ get; set; }
          public List<Assignment> Preconditions_PlannerAssistancePreconditionsAssignments{ get; set; }

          public int Preconditions_ViolatingPreconditionPenalty{ get; set; }

          public List<Assignment> ModuleExecutionTimeDynamicModel{ get; set; }
          public List<Assignment> DynamicModel_VariableAssignments { get; set; }
          public List<Assignment> StateGivenObservationModel_VariableAssignments { get; set; }
        public PLP()
        { 
            PossibleParametersValue = new List<Assignment>();
            GlobalVariableModuleParameters = new List<GlobalVariableModuleParameter>(); 
            ModuleExecutionTimeDynamicModel = new List<Assignment>();
            DynamicModel_VariableAssignments = new List<Assignment>();
            StateGivenObservationModel_VariableAssignments = new List<Assignment>();
            Preconditions_GlobalVariablePreconditionAssignments = new List<Assignment>();
            Preconditions_PlannerAssistancePreconditionsAssignments = new List<Assignment>();
            
        } 
    }
  

    public class GlobalVariableModuleParameter
    {
        public string Name{get;set;}

          public string Type{ get; set; } 

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
        public EStateType LatestReachableState;
        public TempVar TempVariable;

        public List<IterateStateVars> IterateStateVariables;

        public Assignment()
        {
            TempVariable = new TempVar();
            IterateStateVariables = new List<IterateStateVars>();
            LatestReachableState = EStateType.eError;
        }


    }

    public enum EStateType
    {
        eError,
        ePreviousState,
        eAfterExtrinsicChangesState,
        eNextState
    }
    public class IterateStateVars
    {
        public bool ItemInMutableFunction;
        public string Type;
        public string ItemName;
        public string ConditionCode;
        public string WhenConditionTrueCode;

        public EStateType StateType;

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