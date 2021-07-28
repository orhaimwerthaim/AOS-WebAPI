using MongoDB.Bson;  
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using System.Collections.Generic;

namespace WebApiCSharp.Models
{
    public class RosGlue:ModuleDocumentationFile
    {
        public RosServiceActivation RosServiceActivation;
        public List<GlueLocalVariablesInitialization> GlueLocalVariablesInitializations;

        public RosGlue()
        {
            RosServiceActivation = null;
            GlueLocalVariablesInitializations = new List<GlueLocalVariablesInitialization>();
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
        public string AssignServiceFieldCode;

        public string ConstantToAssign;
        public string MsgFieldName;
    }

    public class GlueLocalVariablesInitialization
    {
        public string LocalVarName;
        public string RosTopicPath;
        public string AssignmentCode;
        public string TopicMessageType;
        public string VariableType;

        public string RosParameterPath;

        public string InitialValue;
        public bool? FromROSServiceResponse;

        public List<RosImport> Imports;

        public GlueLocalVariablesInitialization()
        {
            Imports = new List<RosImport>();
        }
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