using MongoDB.Bson;
using System;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using WebApiCSharp.Services;
using WebApiCSharp.Models;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;

namespace WebApiCSharp.JsonTextModel
{
    public class PlpMain
    {
        public string Project{get;set;}
        public string Name{get;set;}
        public string Type{get;set;}
        float Version{get;set;}
        public PlpMain()
        {
            Version=1.0f;
        }

    }
    public class ModuleResponse 
    {
        public string FromStringLocalVariable {get;set;}
        public ResponseRule[] ResponseRules{get;set;}
    }

    public class ResponseRule
    {
        public string Response {get;set;}
        public string ConditionCodeWithLocalVariables {get;set;}
    }

    public class ModuleActivation
    {
        public RosService RosService {get;set;}
    }

    public class RosService
    {
        public ImportCode[] ImportCode{get;set;}
        public string ServiceName {get;set;}
        public string ServicePath {get;set;}
        public List<ServiceParameter> ServiceParameters {get;set;}

        public RosService()
        {
            ServiceParameters = new List<ServiceParameter>();
        }
    }

    public class ServiceParameter
    {
        public string ServiceFieldName{get;set;}
        public string AssignServiceFieldCode{get;set;}
    }

    public class ImportCode
    {
        public string From{get;set;}
        public string[] Import{get;set;}
    }

    public class LocalVariableInitialization
    {
        public bool? FromROSServiceResponse{get;set;}
        public string LocalVariableName{get;set;}
        public string VariableType{get;set;}
        public string RosTopicPath{get;set;}
        public string TopicMessageType{get;set;}

        public string InputLocalVariable{get;set;}

        public string FromGlobalVariable{get;set;}
        
        public string[] AssignmentCode{get;set;}
        public ImportCode[] ImportCode{get;set;}

        public string InitialValue{get;set;}
    }
 


 

    public class AmFile
    {
        public PlpMain PlpMain {get;set;}
        public string GlueFramework{get;set;}

        public ModuleResponse ModuleResponse{get;set;}
        public ModuleActivation ModuleActivation {get;set;}
        public List<LocalVariableInitialization> LocalVariablesInitialization{get;set;}
        public AmFile()
        {
            this.PlpMain = new PlpMain();
            this.ModuleResponse = new ModuleResponse();
        }
    }


public class EnvironmentGeneral
    {
        public int Horizon{get;set;}
        public float Discount{get;set;}  
    }

    public class GlobalVariableType
    {
        public string TypeName {get;set;}
        public string Type {get;set;}
        public GlobalCompoundTypeVariable[] Variables{get;set;}
        public string[] EnumValues{get;set;}
    }

    public class GlobalCompoundTypeVariable
    {
        public string Name {get;set;}
        public string Type {get;set;}
        public string Default {get;set;}

        public float? ML_MaxPossibleValue{get;set;} 

        public bool ML_IgnoreVariable{get;set;}

        public GlobalCompoundTypeVariable()
        {
            ML_IgnoreVariable=false;
        }
    }
    public class GlobalVariableDeclaration
    {
        public string Name {get;set;}
        public string Type {get;set;}
        public string DefaultCode {get;set;} 
        public bool? IsArray{get;set;}
        public bool IsActionParameterValue{get;set;}

        public float? ML_MaxPossibleValue{get;set;}

        public bool ML_IgnoreVariable{get;set;}

        public GlobalVariableDeclaration()
        {
            ML_IgnoreVariable=false;
        }
    }
    public class CodeAssignment
    {
        public string[] AssignmentCode{get;set;}
    }

    public class SpecialStateCode
    {
        public CodeAssignment[] StateFunctionCode{get;set;}
    }
    public class EfFile
    {
        public PlpMain PlpMain {get;set;}
        public EnvironmentGeneral EnvironmentGeneral{get;set;}

        public GlobalVariableType[] GlobalVariableTypes{get;set;}
        
        
        public GlobalVariableDeclaration[] GlobalVariablesDeclaration {get;set;}
        public CodeAssignment[] InitialBeliefStateAssignments{get;set;}
        public SpecialStateCode[] SpecialStates{get;set;}
        public CodeAssignment[] ExtrinsicChangesDynamicModel{get;set;}
        public EfFile()
        {
            this.PlpMain = new PlpMain();
            this.EnvironmentGeneral = new EnvironmentGeneral();
            EnvironmentGeneral.Discount = 0.9999F;
        }
    }

public class GlobalVariableModuleParameter
{
    public string Name{get;set;}
    public string Type{get;set;}
}

public class Preconditions
{
    public CodeAssignment[] GlobalVariablePreconditionAssignments{get;set;}
    public CodeAssignment[] PlannerAssistancePreconditionsAssignments {get;set;}

    public int? ViolatingPreconditionPenalty{get;set;}
}

public class DynamicModel
{
    public CodeAssignment[] NextStateAssignments{get;set;}
}
    public class SdFile
    {
        public PlpMain PlpMain {get;set;}
        public CodeAssignment[] PossibleParametersValue{get;set;}
        public GlobalVariableModuleParameter[] GlobalVariableModuleParameters{get;set;}
        public Preconditions Preconditions{get;set;}
        public DynamicModel DynamicModel{get;set;}

        
        public SdFile()
        {
            this.PlpMain = new PlpMain();
        }
    }
}