using MongoDB.Bson;  
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using System.Collections.Generic;

namespace WebApiCSharp.Models
{
    public class RosGlue:ModuleDocumentationFile
    {
        public List<LocalVariablesInitializationFromGlobalVariable> LocalVariablesInitializationFromGlobalVariables{ get; set; }
        public RosServiceActivation RosServiceActivation;
        public List<GlueLocalVariablesInitialization> GlueLocalVariablesInitializations;
        public List<ResponseRule> ResponseRules{ get; set; }
        public string ResponseFromStringLocalVariable{get;set;}
        public List<string> EnumResponse{ get; set; } 
        public RosGlue()
        {
            ResponseRules = new List<ResponseRule>();
            EnumResponse = new List<string>(); 
            ResponseFromStringLocalVariable=null;
            LocalVariablesInitializationFromGlobalVariables = new List<LocalVariablesInitializationFromGlobalVariable>();
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

    public class LocalVariableBase
    {
        public string VariableName;
        public string VariableType;
        public string SkillName;
        public bool IsHeavyVariable;
    }

    public class LocalVariablesInitializationFromGlobalVariable:LocalVariableBase
    {
        public string InputLocalVariable
        {
            get => VariableName;
            set
            {
                VariableName = value;
            }
        }
        public string FromGlobalVariable;
    }

    public class GlueLocalVariablesInitialization:LocalVariableBase
    {
        public string LocalVarName
        {
            get => VariableName;
            set
            {
                VariableName = value;
            }
        }
        public string RosTopicPath;
        public string AssignmentCode;
        public string TopicMessageType; 
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