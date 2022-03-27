using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using System.Collections.Generic;
namespace WebApiCSharp.Models
{
    public class InitializeProject
    { 
        public string PLPsDirectoryPath { get; set; }

        public bool? RunWithoutRebuild { get; set; }

        public bool? OnlyGenerateCode { get; set; }

        public RosTargetProject RosTarget { get; set; } 
        public SolverConfiguration SolverConfiguration{ get; set; }
        public MiddlewareConfiguration MiddlewareConfiguration{ get; set; } 
    }

    public class RosTargetProject
    {
        public string WorkspaceDirectortyPath { get; set; }

        public string TargetProjectLaunchFile { get; set; }
        public double? TargetProjectInitializationTimeInSeconds { get; set; }
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
        public bool UseSarsop { get; set; } 
        public float OfflineSolverTimeLimitInSeconds { get; set; } 
        public int NumOfSamplesPerStateActionToLearnModel { get; set; } 

        public bool LoadBeliefFromDB { get; set; } 
        public int NumOfParticles { get; set; }

        public int NumOfBeliefStateParticlesToSaveInDB { get; set; }
        public List<int> ActionsToSimulate { get; set; } 
        public bool IsInternalSimulation { get; set; }
        public float PlanningTimePerMoveInSeconds{ get; set; }

        public bool DebugOn{ get; set; }
        public SolverConfiguration()
        {
            NumOfSamplesPerStateActionToLearnModel = 200;
            OfflineSolverTimeLimitInSeconds = 0;
            UseSarsop = true;
            DebugOn = false;
            LoadBeliefFromDB = false; 
            NumOfBeliefStateParticlesToSaveInDB = 1;
            NumOfParticles = 5000;
            PlanningTimePerMoveInSeconds = 2;
            ActionsToSimulate = new List<int>();
            IsInternalSimulation = false;
        }
    }
}