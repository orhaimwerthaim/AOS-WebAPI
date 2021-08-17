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
        public DebugConfiguration DebugConfiguration{ get; set; } 
    }

    public class RosTargetProject
    {
        public string WorkspaceDirectortyPath { get; set; }

        public string TargetProjectLaunchFile { get; set; }
        public int TargetProjectInitializationTimeInSeconds { get; set; }
        public List<string> RosTargetProjectPackages { get; set; }

        
    }

    public class DebugConfiguration
    {
        public bool DebugOn { get; set; }
        public List<int> ActionsToSimulate { get; set; }
    }
}