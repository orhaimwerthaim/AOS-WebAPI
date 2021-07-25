using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using System.Collections.Generic;
namespace WebApiCSharp.Models
{
    public class InitializeProject
    {
        public string PLPsDirectoryPath { get; set; }

        public RosTergetProject RosTarget { get; set; }


    }

    public class RosTergetProject
    {
        public string WorkspaceDirectortyPath { get; set; }

        public string TargetProjectLaunchFile { get; set; }
        public int TargetProjectInitializationTimeInSeconds { get; set; }
        public List<string> RosTargetProjectPackages { get; set; }
    }
}