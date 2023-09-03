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
        public string RosDistribution{get;set;}
        public string WorkspaceDirectortyPath { get; set; }

        public string TargetProjectLaunchFile { get; set; }
        public double? TargetProjectInitializationTimeInSeconds { get; set; }
        public List<string> RosTargetProjectPackages { get; set; }

        
    }

    public class MiddlewareConfiguration
    {
        public bool DebugOn { get; set; } 
        public bool KillRosCoreBeforeStarting {get;set;}
        public MiddlewareConfiguration()
        { 
            DebugOn = false;
        }
    }

    public class SolverConfiguration
    {
        public int PolicyGraphDepth{get;set;}
        public int limitClosedModelHorizon_stepsAfterGoalDetection{get;set;}
        public bool UseSarsop { get; set; }
        public bool UseML { get; set; } 
        public bool UseSavedSarsopPolicy { get; set; } 
        public int NumOfSamplesPerStateActionToLearnModel { get; set; } 

        public bool LoadBeliefFromDB { get; set; } 
        public int NumOfParticles { get; set; }

        public int NumOfBeliefStateParticlesToSaveInDB { get; set; }
        public List<int> ActionsToSimulate { get; set; } 
        public bool IsInternalSimulation { get; set; }
        public float SimulateByMdpRate {get;set;}
        public bool ManualControl { get; set; }
        
        public float PlanningTimePerMoveInSeconds{ get; set; }
        public int RolloutsCount{get;set;}

        public int Verbosity{ get; set; }
        public SolverConfiguration()
        {
            SimulateByMdpRate=0.0f;
            RolloutsCount = 100;
            limitClosedModelHorizon_stepsAfterGoalDetection=-1;
            NumOfSamplesPerStateActionToLearnModel = 20;
            UseSavedSarsopPolicy = false;
            UseSarsop = false;
            Verbosity = 4;
            LoadBeliefFromDB = false; 
            UseML=false;
            NumOfBeliefStateParticlesToSaveInDB = 1;
            NumOfParticles = 5000;
            PlanningTimePerMoveInSeconds = 2;
            ActionsToSimulate = new List<int>();
            IsInternalSimulation = false;
            ManualControl = false;
            PolicyGraphDepth=0;
        }
    }
}