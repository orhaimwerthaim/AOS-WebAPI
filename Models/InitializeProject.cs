using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using System.Collections.Generic;
namespace WebApiCSharp.Models
{
    public class InitializeProject
    { 
        public string PLPsDirectoryPath { get; set; }

        public RosTargetProject RosTarget { get; set; } 
        public SolverConfiguration SolverConfiguration{ get; set; }
        public MiddlewareConfiguration MiddlewareConfiguration{ get; set; } 
    }

    public class RosTargetProject
    {
        public string WorkspaceDirectortyPath { get; set; }

        public string TargetProjectLaunchFile { get; set; }
        public int TargetProjectInitializationTimeInSeconds { get; set; }
        public List<string> RosTargetProjectPackages { get; set; }

        
    }

    public class MiddlewareConfiguration
    {
        public bool DebugOn { get; set; } 
        
        public MiddlewareConfiguration()
        { 
            DebugOn = false;
        }
    }

    public class SolverConfiguration
    {
        public List<int> ActionsToSimulate { get; set; }
        public int SearchDepth { get; set; }
        public float DiscountFactor { get; set; }
        public bool IsInternalSimulation { get; set; }
        public float PlanningTimePerMoveInSeconds{ get; set; }
        public SolverConfiguration()
        {
            DiscountFactor = 0.9999F;
            SearchDepth = 42;
            PlanningTimePerMoveInSeconds = 2;
            ActionsToSimulate = new List<int>();
            IsInternalSimulation = false;
        }
    }
}