using System; 
using MongoDB.Driver;
using MongoDB.Bson;  
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using WebApiCSharp.Models;
using System.Collections.Generic;

namespace WebApiCSharp.Services
{
    public class Globals
    { 
        public const string LOCAL_VARIABLES_COLLECTION_NAME = "LocalVariables";
        public const string PLPS_COLLECTION_NAME = "PLPs";
        public const string SOLVERS_COLLECTION_NAME = "Solvers";
        public const string ACTIONS_COLLECTION_NAME = "Actions";
        public const string GLOBAL_VARIABLES_ASSIGNMENTS_COLLECTION_NAME = "GlobalVariablesAssignments";
        public const string ACTIONS_FOR_EXECUTION_COLLECTION_NAME = "ActionsForExecution";
        public const string MODULE_RESPONSES_COLLECTION_NAME = "ModuleResponses";

        public const string BELIEF_STATES_COLLECTION_NAME = "BeliefStates";
        public const string SIMULATED_STATES_COLLECTION_NAME = "SimulatedStates";
        public const string LOGS_COLLECTION_NAME = "Logs";
        public const string MANUAL_ACTIONS_COLLECTION_NAME = "ManualActionsForSolver";
    }
}