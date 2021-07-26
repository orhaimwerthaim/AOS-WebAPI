using MongoDB.Bson;  
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using System.Collections.Generic;

namespace WebApiCSharp.Models
{
    public class RosGlue:ModuleDocumentationFile
    {
        public RosServiceActivation RosServiceActivation;

        public RosGlue()
        {
            RosServiceActivation = null;
        }
    }

    public class RosServiceActivation
    {
        public string ServiceName;
        public string ServicePath;
        public List<RosImport> Imports;
        public List<GlueParameterAssignment> ParametersAssignments;
        public RosServiceActivation()
        {
            Imports = new List<RosImport>();
            ParametersAssignments = new List<GlueParameterAssignment>();
        }
    }

    public class GlueParameterAssignment
    {
        public string LocalVariableName;
        public string MsgFieldName;
    }

    public class GlueLocalVariablesInitialization
    {
        public string LocalVarName;
        public string RosTopicPath;
        public string AssignmentCode;
        public string TopicMessageType;
        public string VariableType;

        public List<RosImport> Imports;
    }
    public class RosImport
    {
        public string From;
        public List<string> Imports;
        public RosImport()
        {
            Imports = new List<string>();
        }

    }
}