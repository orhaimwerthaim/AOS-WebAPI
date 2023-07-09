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

namespace WebApiCSharp.GenerateCodeFiles
{
    public class PLPsData
    {
        #region public PLP data

        public static string GetFatalErrorMsg(string PLPDesc, Exception e) { return (PLPDesc == null ? "" : PLPDesc) + " Fatal Error!!!" + Environment.NewLine + e.ToString(); }
        public double MaxReward;
        public double MinReward;

        public int Horizon { get; set; }
        public float Discount { get; set; }
  

        public Dictionary<string, DistributionSample> DistributionSamples = new Dictionary<string, DistributionSample>();
        public Dictionary<string, PLP> PLPs = new Dictionary<string, PLP>();
        public Dictionary<string, RosGlue> RosGlues = new Dictionary<string, RosGlue>();

        public bool OneStateModel = false;//no previos state or after Extrinsic changes state, only one state (changes override it)
        public bool HasExtrinsicChanges {
            get{return !(ExtrinsicChangesDynamicModel.Count() == 0);}
        }

        public bool HasPrintStateFunc{get{return !(PrintStateFunc.Count() == 0);}}
        public bool HasDynamicModelChanges {
            get
        {
            bool hasChanges = false;
            foreach(var plp in PLPs.Values)
            {
                hasChanges |= plp.DynamicModel_VariableAssignments.Count() > 0;
            }
            return hasChanges;
        }

        }
        public List<SpecialState> SpecialStates = new List<SpecialState>();
        public List<LocalVariableConstant> LocalVariableConstants = new List<LocalVariableConstant>();
        public List<LocalVariableTypePLP> LocalVariableTypes = new List<LocalVariableTypePLP>();
        public string ProjectName { get; set; }
      
        public string ProjectNameWithCapitalLetter { get; set; }
        public List<Assignment> InitialBeliefAssignments = new List<Assignment>();
        public List<Assignment> ExtrinsicChangesDynamicModel = new List<Assignment>();
        public List<Assignment> PrintStateFunc = new List<Assignment>();
        public List<EnumVarTypePLP> GlobalEnumTypes = new List<EnumVarTypePLP>();
        public List<BaseGlobalVarType> BaseGlobalVarTypes = new List<BaseGlobalVarType>();
        public List<CompoundVarTypePLP> GlobalCompoundTypes = new List<CompoundVarTypePLP>();

        public List<LocalVariableBase> LocalVariablesListings = new List<LocalVariableBase>();
    
        public List<GlobalVariableDeclaration> GlobalVariableDeclarations = new List<GlobalVariableDeclaration>();

        public List<string> AnyValueStateVariableNames = new List<string>();

        #endregion
        public const string GLOBAL_VARIABLE_STATE_REF = "state.";
        public const string PLP_TYPE_NAME_ENVIRONMENT = "Environment";
        public const string PLP_TYPE_NAME_ENVIRONMENT_ExtrinsicChanges = "Environment_ExtrinsicChanges";
        public const string PLP_TYPE_NAME_ENVIRONMENT_GLUE = "EnvironmentGlue";

        public const string PLP_TYPE_NAME_GLUE = "Glue";

        public const string PLP_TYPE_NAME_PLP = "PLP";

        public const string ANY_VALUE_TYPE_NAME = "anyValue";

        public const string ENUM_VARIABLE_TYPE_NAME = "enum";
        public const string INT_VARIABLE_TYPE_NAME = "int";
        public const string ARRAY_VARIABLE_TYPE_NAME = "[]";

        public const string FLOAT_VARIABLE_TYPE_NAME = "float";
        public const string BOOL_VARIABLE_TYPE_NAME = "bool";
        public const string STRING_VARIABLE_TYPE_NAME = "string";
        public const string DOUBLE_VARIABLE_TYPE_NAME = "double";
        public static readonly string[] PRIMITIVE_TYPES = { FLOAT_VARIABLE_TYPE_NAME, ENUM_VARIABLE_TYPE_NAME, INT_VARIABLE_TYPE_NAME, BOOL_VARIABLE_TYPE_NAME , DOUBLE_VARIABLE_TYPE_NAME, STRING_VARIABLE_TYPE_NAME};
        public const string MODULE_EXECUTION_TIME_VARIABLE_NAME = "__moduleExecutionTime";
        public const string MODULE_REWARD_VARIABLE_NAME = "__reward";

        public const string DISCRETE_DISTRIBUTION_FUNCTION_NAME = "AOS.SampleDiscrete"; //AOS.SampleDiscrete(enumRealCase,{0.8, 0.1,0,0.1})
        public const string NORMAL_DISTRIBUTION_FUNCTION_NAME = "AOS.SampleNormal"; //AOS.SampleNormal(40000,10000)
        public const string UNIFORM_DISTRIBUTION_FUNCTION_NAME = "AOS.SampleUniform"; //AOS.SampleUniform(40000,10000)


        public const string AOS_SET_NULL_FUNCTION_NAME = "AOS.SetNull"; //"AOS.SetNull(state__.cupAccurateLocation)"
        public const string AOS_IS_INITIALIZED_FUNCTION_NAME = "AOS.IsInitialized"; //"AOS.IsInitialized(state__.cupAccurateLocation)"
        public const string AOS_UN_INITIALIZED_FUNCTION_NAME = "AOS.Uninitialize";//AOS.Uninitialize(state__.cupAccurateLocation)
        public const string AOS_INITIALIZED_FUNCTION_NAME = "AOS.Initialize";//AOS.Initialize(state__.cupAccurateLocation)

        public const string AOS_Bernoulli_FUNCTION_NAME = "AOS.Bernoulli"; //AOS.Bernoulli(0.9)  :replaced by 'AOSUtils::Bernoulli(0.9)', which is implemented in cpp

        #region private variables 
        private string tempPlpName = "";
        private string tempPlpType = "";
        private BsonDocument environmentPLP = null;
        private string environmentPLP_Name = null;
        private BsonDocument environmentGlue = null;
        private Dictionary<string, BsonDocument> bsonPlps = new Dictionary<string, BsonDocument>();
        private Dictionary<string, BsonDocument> bsonGlues = new Dictionary<string, BsonDocument>();
        #endregion




private string GetLocalVariableTypeByGlobalVarName(string globalVarName, string skillName)
{
    PLP plp = PLPs[skillName];
    string[] bits = globalVarName.Split('.');
    string globalVarType = plp.GlobalVariableModuleParameters.Where(x=> x.Name == bits[0]).FirstOrDefault()?.Type;
    bits[0] = globalVarType;
    
    for(int i=0; i < bits.Length;i++)
    {
        if(GenerateFilesUtils.IsPrimitiveType(bits[i], true))
        {
            return bits[i] == ANY_VALUE_TYPE_NAME ? "bool" : bits[i];
        }    
        else
        {
            CompoundVarTypePLP gl1 = GlobalCompoundTypes.Where(x=> x.TypeName == bits[i]).FirstOrDefault();        
            if(gl1 == null)
            {
                throw new Exception("Skill '"+skillName+"' defines a local variable based on the global variable '"+globalVarName+"'. Could not find compound type named '"+bits[i]+"'!");
            }
            else
            {
                CompoundVarTypePLP_Variable v1 = gl1.Variables.Where(x=> x.Name == bits[i+1]).FirstOrDefault();
                if(gl1 == null)
                {
                    throw new Exception("Skill '"+skillName+"' defines a local variable based on the global variable '"+globalVarName+"'. Could not find the field '"+bits[i+1]+"'in compound type named '"+bits[i]+"'!");
                }
                bits[i+1]=v1.Type;
            }
                
        }
        
        
    } 
    return "";
}

public string GetModelHash()
{
    string res = "";
    res = environmentPLP.ToString();
    foreach (KeyValuePair<string, BsonDocument> plp in bsonPlps)
                {
                    res+=plp.ToString();
                }
                using (var sha = new System.Security.Cryptography.SHA256Managed())
    {
        // Convert the string to a byte array first, to be processed
        byte[] textBytes = System.Text.Encoding.UTF8.GetBytes(res);
        byte[] hashBytes = sha.ComputeHash(textBytes);
        
        // Convert back to a string, removing the '-' that BitConverter adds
        string hash = BitConverter
            .ToString(hashBytes)
            .Replace("-", String.Empty);

        return hash;
    } 

}

        public PLPsData(out List<string> errors)
        { 
            errors = new List<string>();
            try
            {
                List<BsonDocument> bPLPs = PLPsService.GetAll();
                foreach (BsonDocument plp in bPLPs)
                {
                    errors.AddRange(AddPLPSetProjectName(plp));
                    if (errors.Count > 0) return;
                }

                errors.AddRange(ProcessEnvironmentFile());

                ProcessEnvironmentGlueFile();

                List<string> tempErrors = new List<string>();
                foreach (KeyValuePair<string, BsonDocument> plp in bsonPlps)
                {

                    PLPs.Add(plp.Key, (PLP)ProcessModuleDocumentationFile(plp.Value, out tempErrors));
                    errors.AddRange(tempErrors);
                }
                foreach (KeyValuePair<string, BsonDocument> plp in bsonGlues)
                {
                    tempErrors.Clear();
                    RosGlues.Add(plp.Key, (RosGlue)ProcessModuleDocumentationFile(plp.Value, out tempErrors));
                    errors.AddRange(tempErrors);
                }

                List<KeyValuePair<string, string>> allCodeSections = GetC_CodeSections();
                foreach (DistributionType distType in Enum.GetValues(typeof(DistributionType)))
                {
                    GetSampleDistributionFunctions(allCodeSections, distType);
                }

                GetMaxMinReward();
            }
            catch (Exception e)
            {
                errors.Add(GetFatalErrorMsg(null, e));
            }
        }
        //AOS.SampleDiscrete(enumRealCase,{0.8, 0.1,0,0.1})//AOS.SampleNormal(40000,10000)//AOS.SampleUniform(40000,10000)
        private void GetSampleDistributionFunctions(List<KeyValuePair<string, string>> allCodeSections, DistributionType type)
        {
            foreach (KeyValuePair<string, string> code in allCodeSections)
            {
                //original line
                //string c = code.Key.Replace(" ", "");
                string c = code.Key;//Or Wertheim removed the replace(" ","")
                bool found = false;
                string functionName = type == DistributionType.Normal ? NORMAL_DISTRIBUTION_FUNCTION_NAME :
                        type == DistributionType.Discrete ? DISCRETE_DISTRIBUTION_FUNCTION_NAME :
                        type == DistributionType.Uniform ? UNIFORM_DISTRIBUTION_FUNCTION_NAME : null;
                do
                {

                    int index = c.IndexOf(functionName);

                    found = index > -1;
                    if (found)
                    {
                        DistributionSample dist = new DistributionSample();
                        dist.FromFile = code.Value;
                        c = c.Substring(index);
                        dist.FunctionDescription = c.Substring(0, c.IndexOf(")") + 1);
                        c = c.Substring(c.IndexOf(")") + 1);
                        dist.Type = type;

                        if (type == DistributionType.Normal || type == DistributionType.Uniform)
                        {
                            dist.Parameters.AddRange(dist.FunctionDescription.Substring(functionName.Length).Replace("(", "").Replace(")", "").Split(","));
                        }
                        if (type == DistributionType.Discrete)
                        {
                            int startEnumIndex = dist.FunctionDescription.IndexOf("(") + 1;
                            int enumWordLength = dist.FunctionDescription.IndexOf(",") - startEnumIndex;
                            dist.Parameters.Add(dist.FunctionDescription.Substring(startEnumIndex, enumWordLength));
                            dist.Parameters.AddRange(dist.FunctionDescription.Substring(dist.FunctionDescription.IndexOf("{")).Replace("{", "").Replace("}", "").Replace(")", "").Split(","));
                        }

                        string key = dist.FromFile + "_" + dist.FunctionDescription;
                        if (!DistributionSamples.ContainsKey(key))
                        {
                            foreach (string par in dist.Parameters)
                            {  
                                if(GlobalCompoundTypes.Where(x => x.TypeName.Equals(par)).FirstOrDefault() != null)
                                {
                                    dist.HasParameterAsGlobalType = true;
                                    break;
                                }
                                if(GlobalEnumTypes.Where(x => x.TypeName.Equals(par)).FirstOrDefault() != null)
                                {
                                    dist.HasParameterAsGlobalType = true;
                                    break;
                                }
                            }
                            DistributionSamples.Add(key, dist);
                        }
                    }
                } while (found);
            }

            string nameBase = type == DistributionType.Discrete ? "discrete_dist" :
                type == DistributionType.Uniform ? "uniform_dist" :
                type == DistributionType.Normal ? "normal_dist" : null;
            int i = 1;
            foreach (string key in DistributionSamples.Keys)
            {
                if (DistributionSamples[key].Type == type)
                {
                    DistributionSamples[key].C_VariableName = DistributionSamples[key].FromFile + "_" + nameBase + i.ToString();
                    i++;
                }
            }
        }

        private List<KeyValuePair<string, string>> GetC_CodeSections()
        {
            List<KeyValuePair<string, string>> codeSections = new List<KeyValuePair<string, string>>();

            List<Assignment> assignments = new List<Assignment>(); 

            foreach (PLP plp in PLPs.Values)
            {
                foreach (Assignment assign in plp.DynamicModel_VariableAssignments)
                {
                    codeSections.Add(new KeyValuePair<string, string>(assign.AssignmentCode, plp.Name));
                }

                foreach (Assignment assign in plp.ModuleExecutionTimeDynamicModel)
                {
                    codeSections.Add(new KeyValuePair<string, string>(assign.AssignmentCode, plp.Name));
                }

                foreach (Assignment assign in plp.StateGivenObservationModel_VariableAssignments)
                {
                    codeSections.Add(new KeyValuePair<string, string>(assign.AssignmentCode, plp.Name));
                }
            }
 

            foreach (Assignment assign in InitialBeliefAssignments)
            {
                codeSections.Add(new KeyValuePair<string, string>(assign.AssignmentCode, PLP_TYPE_NAME_ENVIRONMENT));
            }

            foreach (Assignment assign in ExtrinsicChangesDynamicModel)
            {
                codeSections.Add(new KeyValuePair<string, string>(assign.AssignmentCode, PLP_TYPE_NAME_ENVIRONMENT));
            }

            foreach (Assignment assign in PrintStateFunc)
            {
                codeSections.Add(new KeyValuePair<string, string>(assign.AssignmentCode, PLP_TYPE_NAME_ENVIRONMENT));
            }
            

            foreach (GlobalVariableDeclaration globalVarDec in GlobalVariableDeclarations)
            {
                if (globalVarDec.DefaultCode != null)
                {
                    codeSections.Add(new KeyValuePair<string, string>(globalVarDec.DefaultCode, PLP_TYPE_NAME_ENVIRONMENT));
                }
            }

            return codeSections;
        }

        private void GetMaxMinReward()
        {
            MaxReward = double.MinValue;
            MinReward = double.MaxValue;

            List<double> rewards = new List<double>();
            foreach (SpecialState ss in SpecialStates)
            {
                rewards.Add(ss.Reward);
            }

            foreach (PLP plp in PLPs.Values)
            {
                rewards.Add(plp.Preconditions_ViolatingPreconditionPenalty);
                foreach (Assignment assign in plp.DynamicModel_VariableAssignments)
                {
                    rewards.AddRange(GetVariableAssignmentsFrom_C_Code(assign.AssignmentCode, MODULE_REWARD_VARIABLE_NAME));
                }
            }

            foreach (double reward in rewards)
            {
                MaxReward = MaxReward < reward ? reward : MaxReward;
                MinReward = MinReward > reward ? reward : MinReward;
            }

            if (rewards.Count == 0)
            {
                MinReward = 0;
                MaxReward = 0;
            }
        }

        private List<string> AddPLPSetProjectName(BsonDocument plp)
        {
            List<string> errors = new List<string>();
            if (ProjectName == null)
            {
                tempPlpName = plp["PlpMain"]["Name"].ToString();
                tempPlpType = plp["PlpMain"]["Type"].ToString();
                ProjectName = plp["PlpMain"]["Project"].ToString();
                ProjectNameWithCapitalLetter = GenerateFilesUtils.ToUpperFirstLetter(ProjectName);
            }

            if (!plp["PlpMain"]["Project"].ToString().Equals(ProjectName))
            {
                errors.Add("The PLPs directory contains PLPs of different project ('" + ProjectName + "' [plp name='" + tempPlpName + "', type='" + tempPlpType + "'],'" + plp["PlpMain"]["Project"].ToString() +
                "' [plp name='" + plp["PlpMain"]["Name"].ToString() + "', type='" + plp["PlpMain"]["Type"].ToString() + "'])!");
            }

            switch (plp["PlpMain"]["Type"].ToString())
            {
                case PLP_TYPE_NAME_ENVIRONMENT:
                    if (environmentPLP != null)
                    {
                        errors.Add("There are two PLPs of type '" + PLP_TYPE_NAME_ENVIRONMENT + "', only one is allowed!");
                    }
                    else
                    {
                        environmentPLP = plp;
                        environmentPLP_Name = environmentPLP["PlpMain"]["Name"].ToString();
                        if (!environmentPLP.Contains("EnvironmentGeneral") || !environmentPLP["EnvironmentGeneral"].AsBsonDocument.Contains("Horizon") || !environmentPLP["EnvironmentGeneral"]["Horizon"].IsInt32)
                        {
                            errors.Add(GetPLPDescriptionForError(environmentPLP_Name, PLP_TYPE_NAME_ENVIRONMENT) + ", \"EnvironmentGeneral.Horizon\" must be defined with integer value!");
                        }
                        Horizon = environmentPLP["EnvironmentGeneral"]["Horizon"].AsInt32;

                        if (!environmentPLP.Contains("EnvironmentGeneral") || !environmentPLP["EnvironmentGeneral"].AsBsonDocument.Contains("Discount") || !environmentPLP["EnvironmentGeneral"]["Discount"].IsDouble)
                        {
                            errors.Add(GetPLPDescriptionForError(environmentPLP_Name, PLP_TYPE_NAME_ENVIRONMENT) + ", \"EnvironmentGeneral.Discount\" must be defined with decimal value!");
                        }
                        Discount = (float)environmentPLP["EnvironmentGeneral"]["Discount"].AsDouble;

                        OneStateModel = environmentPLP.Contains("EnvironmentGeneral") && environmentPLP["EnvironmentGeneral"].AsBsonDocument.Contains("OneStateModel") ? environmentPLP["EnvironmentGeneral"]["OneStateModel"].AsBoolean : false;
                    }
                    break;
                case PLP_TYPE_NAME_ENVIRONMENT_GLUE:
                    if (environmentGlue != null)
                    {
                        errors.Add("There are two PLPs of type '" + PLP_TYPE_NAME_ENVIRONMENT_GLUE + "', only one is allowed!");
                    }
                    else
                    {
                        environmentGlue = plp;
                    }
                    break;
                case PLP_TYPE_NAME_PLP:
                    if (bsonPlps.ContainsKey(plp["PlpMain"]["Name"].ToString()))
                    {
                        errors.Add("There are two PLPs of name '" + plp["PlpMain"]["Name"] + "' and type '" + PLP_TYPE_NAME_PLP + "', only one is allowed!");
                    }
                    else
                    {
                        bsonPlps.Add(plp["PlpMain"]["Name"].ToString(), plp);
                    }
                    break;
                case PLP_TYPE_NAME_GLUE:
                    if (bsonGlues.ContainsKey(plp["PlpMain"]["Name"].ToString()))
                    {
                        errors.Add("There are two PLPs of name '" + plp["PlpMain"]["Name"] + "' and type '" + PLP_TYPE_NAME_GLUE + "', only one is allowed!");
                    }
                    else
                    {
                        bsonGlues.Add(plp["PlpMain"]["Name"].ToString(), plp);
                    }
                    break;
            }

            return errors;
        }



        private List<double> GetVariableAssignmentsFrom_C_Code(string code, string variableName)
        {
            List<double> values = new List<double>();
            code = code.Replace(" ", "");
            bool found = false;
            do
            {
                found = code.IndexOf(MODULE_REWARD_VARIABLE_NAME) > -1;
                code = found ? code.Substring(code.IndexOf(MODULE_REWARD_VARIABLE_NAME) + MODULE_REWARD_VARIABLE_NAME.Length) : "";
                if (code.StartsWith("=") && code.Contains(";"))
                {
                    string temp = code.Substring(1, code.IndexOf(";") - 1);
                    double value = double.MinValue;
                    double.TryParse(temp, out value);
                    if (value > double.MinValue)
                    {
                        values.Add(value);
                    }
                }
            } while (found);
            return values;
        }

        private void ProcessEnvironmentGlueFile()
        {
            if (environmentGlue != null)
            {
                foreach (BsonValue bType in environmentGlue["LocalVariableTypes"].AsBsonArray)
                {
                    LocalVariableTypePLP localVar = new LocalVariableTypePLP();
                    BsonDocument docType = bType.AsBsonDocument;

                    localVar.TypeName = docType["TypeName"].ToString();

                    foreach (BsonValue bVar in docType["Variables"].AsBsonArray)
                    {
                        BsonDocument docVar = bVar.AsBsonDocument;
                        localVar.SubFields.Add(new LocalVariableCompoundTypeField() { FieldName = docVar["Name"].ToString(), FieldType = docVar["Type"].ToString() });
                    }
                    LocalVariableTypes.Add(localVar);
                }


                foreach (BsonValue bConst in environmentGlue["Constants"].AsBsonArray)
                {
                    LocalVariableConstant localVar = new LocalVariableConstant();
                    BsonDocument docConst = bConst.AsBsonDocument;

                    localVar.Name = docConst["Name"].ToString();
                    localVar.Type = docConst["Type"].ToString();
                    localVar.InitCode = docConst["InitCode"].ToString();

                    LocalVariableConstants.Add(localVar);
                }
            }
        }
        private List<string> ProcessEnvironmentFile()
        {
            List<string> errors = new List<string>();
            List<string> tempErrors;
            GetEnvironmentTypes(out tempErrors);
            errors.AddRange(tempErrors);
            errors.AddRange(GetGlobalVariablesDeclaration());
            errors.AddRange(GetBeliefStateAssignments_ExtrinsicChangesDynamicModel_AndSpecialStates());
            return errors;
        }

        

        private List<string> GetBeliefStateAssignments_ExtrinsicChangesDynamicModel_AndSpecialStates()
        {
            List<string> errors = new List<string>();
            List<string> tempErrors = new List<string>();

            tempErrors.Clear();
            List<Assignment> extrinsicChangesDynamicModel = !environmentPLP.Contains("ExtrinsicChangesDynamicModel") ? new List<Assignment>() : LoadAssignment(environmentPLP["ExtrinsicChangesDynamicModel"].AsBsonArray, environmentPLP_Name,
                PLP_TYPE_NAME_ENVIRONMENT, out tempErrors, EStateType.eAfterExtrinsicChangesState);
            errors.AddRange(tempErrors);
            ExtrinsicChangesDynamicModel.AddRange(extrinsicChangesDynamicModel);


            tempErrors.Clear();
            List<Assignment> initialBeliefStateAssignments = !environmentPLP.Contains("InitialBeliefStateAssignments") ? new List<Assignment>() : LoadAssignment(environmentPLP["InitialBeliefStateAssignments"].AsBsonArray, environmentPLP_Name,
                PLP_TYPE_NAME_ENVIRONMENT, out tempErrors, EStateType.ePreviousState);
            errors.AddRange(tempErrors);
            InitialBeliefAssignments.AddRange(initialBeliefStateAssignments);

            
            tempErrors.Clear();
            List<Assignment> printStateFunc = !environmentPLP.Contains("PrintStateFunc") ? new List<Assignment>() : LoadAssignment(environmentPLP["PrintStateFunc"].AsBsonArray, environmentPLP_Name,
                PLP_TYPE_NAME_ENVIRONMENT, out tempErrors, EStateType.ePreviousState);
            errors.AddRange(tempErrors);
            PrintStateFunc.AddRange(printStateFunc);



            foreach (BsonValue bState in environmentPLP["SpecialStates"].AsBsonArray)
            {
                try
                {
                    BsonDocument docState = bState.AsBsonDocument;
                    SpecialState spState = new SpecialState();
                    
                    if(docState.Contains("StateFunctionCode"))
                    {
                        if(docState.Contains("IsGoalState") || docState.Contains("IsGoalState") || docState.Contains("IsGoalState"))
                        {
                            throw new Exception("SpecialState.StateFunctionCode cannot be defined alongside IsGoalState, StateConditionCode, or IsOneTimeReward");
                        }
                    tempErrors.Clear();
                    spState.StateFunctionCode = LoadAssignment(docState["StateFunctionCode"].AsBsonArray, environmentPLP_Name,
                                    PLP_TYPE_NAME_ENVIRONMENT, out tempErrors, EStateType.ePreviousState);
                    errors.AddRange(tempErrors);
                    }
                    else
                    {
                        spState.IsGoalState = !docState.Contains("IsGoalState") ? false : docState["IsGoalState"].AsBoolean;
                        spState.StateConditionCode = docState["StateConditionCode"].ToString();
                        spState.IsOneTimeReward = !docState.Contains("IsOneTimeReward") ? false : docState["IsOneTimeReward"].AsBoolean;
                        spState.Reward = docState["Reward"].AsDouble;
                    }

                    SpecialStates.Add(spState);
                }

                catch (Exception e2)
                {
                    errors.Add(GetPLPDescriptionForError(environmentPLP_Name, PLP_TYPE_NAME_ENVIRONMENT) + ", if \"SpecialStates.StateFunctionCode\" is defined, no other fields are allowed o.w \"SpecialStates.IsGoalState\" (default is 'false') and \"IsOneTimeReward\"(default is 'false') must be boolean,  \"SpecialStates.Reward\" must be decimal, \"SpecialStates.StateConditionCode\" must be defined!");
                }
            }
            return errors;
        }




        private List<string> GetEnumValues(BsonDocument doc, string enumValuesFieldName, out List<string> errors)
        {
            List<string> enumValues = new List<string>();
            errors = new List<string>();
            foreach (BsonValue bEnumVal in doc[enumValuesFieldName].AsBsonArray)
            {
                enumValues.Add(bEnumVal.AsString.ToString());
            }
            return enumValues;
        }
        private void GetEnvironmentTypes(out List<string> errors)
        {
            errors = new List<string>();
            if(environmentPLP.Contains("GlobalVariableTypes"))
            {
            foreach (BsonValue bVal in environmentPLP["GlobalVariableTypes"].AsBsonArray)
            {
                BsonDocument doc = bVal.AsBsonDocument;
                switch (doc["Type"].ToString())
                {
                    case ENUM_VARIABLE_TYPE_NAME:
                        EnumVarTypePLP enu = new EnumVarTypePLP();

                        enu.TypeName = doc["TypeName"].ToString();
                        enu.Values.AddRange(GetEnumValues(doc, "EnumValues", out errors));

                        GlobalEnumTypes.Add(enu);
                        BaseGlobalVarTypes.Add(enu);
                        break;
                    case "compound":
                        CompoundVarTypePLP comp = new CompoundVarTypePLP();
                        comp.TypeName = doc["TypeName"].ToString();
                        foreach (BsonValue bVariables in doc["Variables"].AsBsonArray)
                        {
                            CompoundVarTypePLP_Variable oVar = new CompoundVarTypePLP_Variable();
                            BsonDocument docVar = bVariables.AsBsonDocument;
                            oVar.Name = docVar["Name"].ToString();
                            oVar.Type = docVar["Type"].ToString();
                            oVar.Default = docVar.Contains("Default") ? (docVar["Default"].ToString().Equals("") ? null : docVar["Default"].ToString()) : null;
                            if(docVar.Contains("ML_MaxPossibleValue"))
                            {
                                float f;
                                string s = docVar["ML_MaxPossibleValue"].ToString();
                                if(float.TryParse(s, out f))
                                {
                                    oVar.ML_MaxPossibleValue=f;
                                }
                                else
                                {
                                    errors.Add(GetPLPDescriptionForError("", PLP_TYPE_NAME_ENVIRONMENT) + ", 'ML_MaxPossibleValue' must be a decimal number!");
                                }
                            }

                            oVar.ML_IgnoreVariable = docVar.Contains("ML_IgnoreVariable") ? docVar["ML_IgnoreVariable"].AsBoolean : false;
                                
                            oVar.UnderlineLocalVariableType = GetBsonStringField(docVar, "UnderlineLocalVariableType");

                            if (oVar.UnderlineLocalVariableType != null && oVar.Type != ANY_VALUE_TYPE_NAME)
                            {
                                errors.Add(GetPLPDescriptionForError("", PLP_TYPE_NAME_ENVIRONMENT) + ", in 'GlobalVariableTypes',  'UnderlineLocalVariableType' is defined while type '" + oVar.Type + "' is not '" + ANY_VALUE_TYPE_NAME + "'!");
                            }
                            comp.Variables.Add(oVar);
                        }
                        GlobalCompoundTypes.Add(comp);
                        BaseGlobalVarTypes.Add(comp);
                        break;
                }
            }
            }
        }


        private List<string> GetParameterizedGlobalVarData(GlobalVariableDeclaration oVarDec, BsonDocument docVar)
        {
            List<string> errors = new List<string>();

            oVarDec.ParametersData.IncludeParametersCode = !docVar.Contains("IncludeParametersCode") ? "true" : 
            (docVar["IncludeParametersCode"].ToString().Replace(" ","").Equals("") ? "true" : docVar["IncludeParametersCode"].ToString());
            if(docVar.Contains("Parameters"))
            {
                string[] pars = docVar["Parameters"].ToString().Replace(" ","").Split(',');
                foreach (BsonValue bVal in docVar["Parameters"].AsBsonArray)
                {
                    string parStr = bVal.ToString();
                    GlobalVariableDeclarationParameter p = new GlobalVariableDeclarationParameter();
                    EnumVarTypePLP enumP = GlobalEnumTypes.Where(x=> x.TypeName == parStr).FirstOrDefault();
                    if(enumP != null)
                    {
                        p.EnumParameterType = enumP;
                        oVarDec.ParametersData.Parameters.Add(p);
                        continue;
                    }
                    else if(parStr.Contains("(") && parStr.Contains(")"))
                    {
                        string[] bits = parStr.Replace("(","").Replace(")","").Split(',');
                        bool hasError = false;
                        if(bits.Length != 3)
                        {
                            hasError = true;
                            
                        }
                        else
                        {
                            int temp;
                            hasError |= !int.TryParse(bits[0], out temp);
                            p.Start = temp;
                            
                            hasError |= !int.TryParse(bits[1], out temp);
                            p.Stop = temp;

                            hasError |= !int.TryParse(bits[2], out temp);
                            p.Step = temp;
                        }
                        if(hasError)
                        {
                            errors.Add(GetPLPDescriptionForError("", PLP_TYPE_NAME_ENVIRONMENT) + ", in 'GlobalVariablesDeclaration', variable '"+oVarDec.Name+
                            "' has an invalid parameter. Valid parameters are either enum types or integer range with the following form '(Start,End,Step)' e.g. '(1,10,1)'");
                            continue;
                        }
                        else
                        {
                            oVarDec.ParametersData.Parameters.Add(p);
                            continue;
                        }
                    }
                }
            }
            return errors;
        }

        private List<string> GetGlobalVariablesDeclaration()
        {
            List<string> errors = new List<string>();
            //globalVariablesDeclaration
            foreach (BsonValue bVal in environmentPLP["GlobalVariablesDeclaration"].AsBsonArray)
            {
                BsonDocument docVar = bVal.AsBsonDocument;
                GlobalVariableDeclaration oVarDec = new GlobalVariableDeclaration();

                oVarDec.Name = docVar["Name"].ToString();
                oVarDec.Type = docVar["Type"].ToString();
                oVarDec.Default = docVar.Contains("Default") ? (docVar["Default"].ToString().Equals("") ? null : docVar["Default"].ToString()) : null;
                oVarDec.DefaultCode = docVar.Contains("DefaultCode") ? (docVar["DefaultCode"].ToString().Equals("") ? null : docVar["DefaultCode"].ToString()) : null;
                oVarDec.IsArray = docVar.Contains("IsArray") ? docVar["IsArray"].AsBoolean : false;
                if(docVar.Contains("ML_MaxPossibleValue"))
                {
                    float f;
                    string s = docVar["ML_MaxPossibleValue"].ToString();
                    if(float.TryParse(s, out f))
                    {
                        oVarDec.ML_MaxPossibleValue=f;
                    }
                    else
                    {
                        errors.Add(GetPLPDescriptionForError("", PLP_TYPE_NAME_ENVIRONMENT) + ", 'ML_MaxPossibleValue' must be a decimal number!");
                    }
                    oVarDec.ML_IgnoreVariable = docVar.Contains("ML_IgnoreVariable") ? docVar["ML_IgnoreVariable"].AsBoolean : false;
                }
                oVarDec.IsActionParameterValue = docVar.Contains("IsActionParameterValue") ? docVar["IsActionParameterValue"].AsBoolean : false;
                oVarDec.ML_IgnoreVariable = oVarDec.IsActionParameterValue ? true : oVarDec.ML_IgnoreVariable;

                oVarDec.UnderlineLocalVariableType = GetBsonStringField(docVar, "UnderlineLocalVariableType");
                errors.AddRange(GetParameterizedGlobalVarData(oVarDec, docVar));
                if (oVarDec.UnderlineLocalVariableType != null && oVarDec.Type != ANY_VALUE_TYPE_NAME)
                {
                    errors.Add(GetPLPDescriptionForError("", PLP_TYPE_NAME_ENVIRONMENT) + ", in 'GlobalVariablesDeclaration',  'UnderlineLocalVariableType' is defined while type '" + oVarDec.Type + "' is not '" + ANY_VALUE_TYPE_NAME + "'!");
                }

                GlobalVariableDeclarations.Add(oVarDec);
                if (oVarDec.DefaultCode != null && oVarDec.Default != null)
                {
                    errors.Add("PLP type='" + PLP_TYPE_NAME_ENVIRONMENT + "', Section='GlobalVariablesDeclaration', variable Name='" + oVarDec.Name +
                    "', 'DefaultCode' and for 'Default' are defined, only one of them can be defined!");
                }
            }
            FillGlobalVariableDeclarationsSubFields();
            return errors;
        }
  
        private void FillGlobalVariableDeclarationsSubFields(GlobalVariableDeclaration parentVar = null)
        {
            if (parentVar == null)
            {
                foreach (var gVar in GlobalVariableDeclarations)
                {
                    gVar.StateVariableName = "state." + gVar.Name;
                    if (gVar.Type.Equals(ANY_VALUE_TYPE_NAME))
                    {
                        AnyValueStateVariableNames.Add(gVar.StateVariableName);
                    }
                    else if (!GenerateFilesUtils.IsPrimitiveType(gVar.Type))
                    { 
                        FillGlobalVariableDeclarationsSubFields(gVar);
                    }
                }
            }
            else
            {
                var compType = GlobalCompoundTypes.Where(x => x.TypeName.Equals(parentVar.Type)).FirstOrDefault();
                if(compType != null)
                {
                    foreach(var subField in compType.Variables)
                    {
                        
                        GlobalVariableDeclaration subVar = new GlobalVariableDeclaration();
                        subVar.Name = subField.Name;
                        subVar.Default = subField.Default;
                        subVar.StateVariableName = parentVar.StateVariableName + "." + subField.Name;
                        subVar.Type= compType.Variables.Where(x=> x.Name.Equals(subVar.Name)).FirstOrDefault()?.Type;
                        parentVar.SubCompoundFeilds.Add(subVar);
                        if(subField.Type.Equals(ANY_VALUE_TYPE_NAME))
                        {
                            AnyValueStateVariableNames.Add(subVar.StateVariableName);
                        }
                        else if (!GenerateFilesUtils.IsPrimitiveType(subField.Type))
                        {
                            FillGlobalVariableDeclarationsSubFields(subVar);
                        }
                    }
                }
                else
                {
                
                }
            }
        }

        private string GetBsonStringField(BsonDocument doc, string field, string internalField = null)
        {
            if (internalField == null)
            {
                return doc.Contains(field) ? (doc[field].ToString().Replace(" ", "").Equals("") ? null
                    : doc[field].ToString()) : null;
            }
            else
            {
                return doc.Contains(field) && doc[field].AsBsonDocument.Contains(internalField) ?
                    (doc[field][internalField].ToString().Replace(" ", "").Equals("") ? null
                        : doc[field][internalField].ToString()) : null;
            }
        }

        private string GetBsonStringOrStringArrayField(BsonDocument oDoc, string field, string innerField = null)
        {
            string result="";
            if(!oDoc.Contains(field)) return "";
            if(innerField != null && !oDoc[field].AsBsonDocument.Contains(innerField)) return "";
            
            BsonDocument doc = null;
            BsonArray bArr= null;

            if(innerField != null)
            {
                result = oDoc[field][innerField].IsBsonArray ? "" : oDoc[field][innerField].ToString();
                bArr = oDoc[field][innerField].IsBsonArray ? oDoc[field][innerField].AsBsonArray : null;
            }
            else
            {
                result = oDoc[field].IsBsonArray ? "" : oDoc[field].ToString();
                bArr = oDoc[field].IsBsonArray ? oDoc[field].AsBsonArray : null;
            } 
            
            if(bArr != null)
            {
                for(int i=0; i < bArr.Count; i++)
                {
                    BsonValue bVal = bArr[i];
                    result += i > 0 ? Environment.NewLine : "";
                    result += bVal.ToString();
                }
            } 
            return result;
        }

        private string GetPLPDescriptionForError(ModuleDocumentationFile docFile)
        {
            return "PLP name='" + docFile.Name + "',type = '" + docFile.Type + "'";
        }
        private string GetPLPDescriptionForError(string plpName, string plpType)
        {
            return "PLP name='" + plpName + "',type = '" + plpType + "'";
        }

        private RosGlue ProcessRosGlue(BsonDocument bRosGlue, out List<string> errors, RosGlue rosGlue)
        {
            errors = new List<string>();
            List<string> tempErrors = new List<string>();

            string plpDescription = GetPLPDescriptionForError(rosGlue);
            
            if(bRosGlue["ModuleResponse"].AsBsonDocument.Contains("FromStringLocalVariable"))
            {
                rosGlue.ResponseFromStringLocalVariable = bRosGlue["ModuleResponse"]["FromStringLocalVariable"].ToString();
            }
            
            if (bRosGlue["ModuleResponse"].AsBsonDocument.Contains("ResponseRules"))
            {
                foreach (BsonValue bVal in bRosGlue["ModuleResponse"]["ResponseRules"].AsBsonArray)
                {
                    BsonDocument docResponseRule = bVal.AsBsonDocument;
                    ResponseRule oResponseRule = new ResponseRule();

                    oResponseRule.Condition = docResponseRule["ConditionCodeWithLocalVariables"].ToString();
                    oResponseRule.Response = docResponseRule["Response"].ToString();
                    oResponseRule.Comment = GetBsonStringField(docResponseRule, "Comment");


                    if (docResponseRule.Contains("AssignGlobalVariables"))
                    {
                        foreach (BsonValue bAssign in docResponseRule["AssignGlobalVariables"].AsBsonArray)
                        {
                            BsonDocument boAssign = bAssign.AsBsonDocument;
                            ResponseAssignmentToGlobalVar assignment = new ResponseAssignmentToGlobalVar();
                            assignment.GlobalVarName = GetBsonStringField(boAssign, "VarName");
                            assignment.Value = GetBsonStringField(boAssign, "Value");
                            if (assignment.GlobalVarName == null)
                            {
                                errors.Add(plpDescription + ", 'AssignGlobalVariables' contains an item without 'VarName', it is a mandatory field!");
                            }
                            if (assignment.Value == null)
                            {
                                errors.Add(plpDescription + ", 'AssignGlobalVariables' contains an item without 'Value', it is a mandatory field!");
                            }
                            oResponseRule.ResponseAssignmentsToGlobalVar.Add(assignment);
                        }
                    }

                    rosGlue.ResponseRules.Add(oResponseRule);
                    if(!rosGlue.EnumResponse.Contains(oResponseRule.Response))
                    {
                        rosGlue.EnumResponse.Add(oResponseRule.Response);
                        if (oResponseRule.Response.Contains(" ") || oResponseRule.Response.Contains("*"))
                        {
                            errors.Add(plpDescription + ", in 'EnumResponse', response value cannot contain spaces or special characters");
                        }
                    }
                    // else
                    // {
                    //     errors.Add(plpDescription + ", 'EnumResponse' contains duplicate values ('" + oResponseRule.Response + "')");
                    // }
                }
            }


            if (bRosGlue.Contains("LocalVariablesInitialization"))
            {
                foreach (BsonValue bVal in bRosGlue["LocalVariablesInitialization"].AsBsonArray)
                {
                    BsonDocument docVar = bVal.AsBsonDocument;
                    if(docVar.Contains("FromGlobalVariable"))
                    { 
                        LocalVariablesInitializationFromGlobalVariable oVar = new LocalVariablesInitializationFromGlobalVariable();

                        oVar.FromGlobalVariable = docVar["FromGlobalVariable"].ToString();
                        oVar.InputLocalVariable = docVar["InputLocalVariable"].ToString();
                        oVar.SkillName = rosGlue.Name;
                        oVar.VariableType = GetLocalVariableTypeByGlobalVarName(oVar.FromGlobalVariable, oVar.SkillName);
                        
                        rosGlue.LocalVariablesInitializationFromGlobalVariables.Add(oVar);
                        LocalVariablesListings.Add(oVar);
                    }
                    else
                    {
                        GlueLocalVariablesInitialization oVar = new GlueLocalVariablesInitialization();
                        oVar.LocalVarName = GetBsonStringField(docVar, "LocalVariableName");
                        oVar.RosTopicPath = GetBsonStringField(docVar, "RosTopicPath");
                        oVar.TopicMessageType = GetBsonStringField(docVar, "TopicMessageType");
                        oVar.IsHeavyVariable = docVar.Contains("IsHeavyVariable") ? docVar["IsHeavyVariable"].AsBoolean : false;
                        //oVar.AssignmentCode = GetBsonStringField(docVar, "AssignmentCode");
                         oVar.AssignmentCode = GetBsonStringOrStringArrayField(docVar, "AssignmentCode");//Or Wertheim changed to allow string arrays as code lines
                        oVar.VariableType = GetBsonStringField(docVar, "VariableType");
                        oVar.RosParameterPath = GetBsonStringField(docVar, "RosParameter");
                        oVar.SkillName = rosGlue.Name;
                        tempErrors.Clear();
                        oVar.FromROSServiceResponse = GetBoolFieldFromBsonNullIfNotThere(rosGlue.Name, PLPsData.PLP_TYPE_NAME_GLUE, docVar, "FromROSServiceResponse", null, out tempErrors);
                        errors.AddRange(tempErrors);
                        tempErrors.Clear();
                        oVar.Imports.AddRange(LoadImports(docVar, out tempErrors, plpDescription, "LocalVariablesInitialization"));
                        errors.AddRange(tempErrors);

                        oVar.InitialValue = GetBsonStringField(docVar, "InitialValue");

                        rosGlue.GlueLocalVariablesInitializations.Add(oVar);
                        LocalVariablesListings.Add(oVar);

                        if (oVar.LocalVarName == null)
                        {
                            errors.Add(plpDescription + ", 'LocalVariablesInitialization', contains an element without a definition for 'LocalVariableName', which is a mandatory field!");
                        }

                        if (oVar.AssignmentCode == null)
                        {
                            errors.Add(plpDescription + ", 'LocalVariablesInitialization', contains an element without a definition for 'AssignmentCode', which is a mandatory field!");
                        }

                        if ((oVar.TopicMessageType == null) != (oVar.RosTopicPath == null))
                        {
                            errors.Add(plpDescription + ", 'LocalVariablesInitialization', contains an element in which 'RosTopicPath' and 'TopicMessageType' are not both defined (or both not defined), it must be conssitent!");
                        }
                    }
                }
            }

            Dictionary<string,string> localVarsNameType = new Dictionary<string,string>();
            foreach(LocalVariableBase lv in LocalVariablesListings)
            {
                if(localVarsNameType.ContainsKey(lv.VariableName) && lv.VariableType != localVarsNameType[lv.VariableName])
                {
                    errors.Add(GetPLPDescriptionForError(lv.SkillName, "Glue") + ", local variable named '"+lv.VariableName+"' of type '"+lv.VariableType+"' is defined. There is a local variable with the same name and a different type defined in anoter glue file. It is not possible to have two local variables with the same name and different types!");
                }
                localVarsNameType[lv.VariableName]= lv.VariableType;
            }

            if (!bRosGlue.Contains("ModuleActivation"))
            {
                errors.Add(plpDescription + ", 'ModuleActivation' is not defined, but it is a mandatory field!");
            }
            if (bRosGlue["ModuleActivation"].AsBsonDocument.Contains("RosService"))
            {
                BsonDocument docAct = bRosGlue["ModuleActivation"]["RosService"].AsBsonDocument;
                rosGlue.RosServiceActivation = new RosServiceActivation();
                rosGlue.RosServiceActivation.ServiceName = GetBsonStringField(docAct, "ServiceName");

                if (rosGlue.RosServiceActivation.ServiceName == null)
                {
                    errors.Add(plpDescription + ", 'ModuleActivation.RosService' does not contain a definition for 'ServiceName', which is a mandatory field!");
                }

                rosGlue.RosServiceActivation.ServicePath = GetBsonStringField(docAct, "ServicePath");

                if (rosGlue.RosServiceActivation.ServicePath == null)
                {
                    errors.Add(plpDescription + ", 'ModuleActivation.RosService' does not contain a definition for 'ServicePath', which is a mandatory field!");
                }

                if (docAct.Contains("ServiceParameters"))
                {
                    foreach (BsonValue bVal in docAct["ServiceParameters"].AsBsonArray)
                    {
                        BsonDocument docPar = bVal.AsBsonDocument;
                        GlueParameterAssignment oPar = new GlueParameterAssignment();
                        oPar.MsgFieldName = GetBsonStringField(docPar, "ServiceFieldName");
                        oPar.AssignServiceFieldCode = GetBsonStringField(docPar, "AssignServiceFieldCode");


                        rosGlue.RosServiceActivation.ParametersAssignments.Add(oPar);
                        if (oPar.MsgFieldName == null)
                        {
                            errors.Add(plpDescription + ", 'ModuleActivation.RosService.ServiceParameters', contains an element without a definition for 'ServiceFieldName', which is a mandatory field!");
                        }

                        if (oPar.MsgFieldName == null)
                        {
                            errors.Add(plpDescription + ", 'ModuleActivation.RosService.ServiceParameters', contains an element without a definition for 'AssignServiceFieldCode', which is a mandatory field!");
                        }
                    }
                }

                tempErrors.Clear();
                rosGlue.RosServiceActivation.Imports.AddRange(LoadImports(docAct, out tempErrors, plpDescription, "ModuleActivation.RosService"));
                errors.AddRange(tempErrors);
                /*if (docAct.Contains("ImportCode"))
                {
                    foreach (BsonValue bVal in docAct["ImportCode"].AsBsonArray)
                    {
                        BsonDocument docImp = bVal.AsBsonDocument;
                        RosImport oImp = new RosImport();

                        oImp.From = GetBsonStringField(docImp, "From");


                        if (docImp.Contains("Import"))
                        {
                            foreach (BsonValue bVal2 in docImp["Import"].AsBsonArray)
                            {
                                oImp.Imports.Add(bVal2.ToString());
                            }
                        }

                        if (oImp.From == null && oImp.Imports.Count == 0)
                        {
                            errors.Add(plpDescription + ", in 'ModuleActivation.RosService.ImportCode',  contains an element without a definition for either 'From' or 'Import', one of them must be deinfed!");
                        }
                        rosGlue.RosServiceActivation.Imports.Add(oImp);
                    }
                }*/
            }

            return rosGlue;
        }

        private List<RosImport> LoadImports(BsonDocument doc, out List<string> errors, string plpDescription, string baseJsonField)
        {
            errors = new List<string>();
            List<RosImport> result = new List<RosImport>();
            if (doc.Contains("ImportCode"))
            {
                foreach (BsonValue bVal in doc["ImportCode"].AsBsonArray)
                {
                    BsonDocument docImp = bVal.AsBsonDocument;
                    RosImport oImp = new RosImport();

                    oImp.From = GetBsonStringField(docImp, "From");


                    if (docImp.Contains("Import"))
                    {
                        foreach (BsonValue bVal2 in docImp["Import"].AsBsonArray)
                        {
                            oImp.Imports.Add(bVal2.ToString());
                        }
                    }

                    if (oImp.From == null && oImp.Imports.Count == 0)
                    {
                        errors.Add(plpDescription + ", in '" + baseJsonField + ".ImportCode',  contains an element without a definition for either 'From' or 'Import', one of them must be deinfed!");
                    }
                    result.Add(oImp);
                }
            }


            return result;
        }
        private PLP ProcessPLP(BsonDocument bPlp, out List<string> errors, PLP plp)
        {
            errors = new List<string>();
            List<string> tempErrors = new List<string>();

            string plpDescription = GetPLPDescriptionForError(plp);
            if (bPlp.Contains("GlobalVariableModuleParameters"))
            {
                foreach (BsonValue bVal in bPlp["GlobalVariableModuleParameters"].AsBsonArray)
                {
                    BsonDocument docPar = bVal.AsBsonDocument;
                    GlobalVariableModuleParameter oPar = new GlobalVariableModuleParameter();

                    oPar.Name = docPar["Name"].ToString();
                    oPar.Type = docPar["Type"].ToString();

                    plp.GlobalVariableModuleParameters.Add(oPar);
                }
            }

            if(plp.GlobalVariableModuleParameters.Count() > 0 && bPlp.Contains("PossibleParametersValue"))
            {
 
            tempErrors.Clear();
            plp.PossibleParametersValue = LoadAssignment(bPlp["PossibleParametersValue"].AsBsonArray, plp.Name, plp.Type, out tempErrors, EStateType.ePreviousState);
            errors.AddRange(tempErrors);               
            }

            tempErrors.Clear();
            plp.Preconditions_GlobalVariablePreconditionAssignments = !bPlp.Contains("Preconditions") || !bPlp["Preconditions"].AsBsonDocument.Contains("GlobalVariablePreconditionAssignments") ? new List<Assignment>() : LoadAssignment(bPlp["Preconditions"]["GlobalVariablePreconditionAssignments"].AsBsonArray, plp.Name, plp.Type, out tempErrors, EStateType.ePreviousState, true);
            errors.AddRange(tempErrors);

            tempErrors.Clear();
            plp.Preconditions_PlannerAssistancePreconditionsAssignments = !bPlp.Contains("Preconditions") || !bPlp["Preconditions"].AsBsonDocument.Contains("PlannerAssistancePreconditionsAssignments") ? new List<Assignment>() : LoadAssignment(bPlp["Preconditions"]["PlannerAssistancePreconditionsAssignments"].AsBsonArray, plp.Name, plp.Type, out tempErrors, EStateType.ePreviousState, true);
            errors.AddRange(tempErrors);

            List<Assignment> preconditions = new List<Assignment>(); 
            preconditions.AddRange(plp.Preconditions_PlannerAssistancePreconditionsAssignments);
            preconditions.AddRange(plp.Preconditions_GlobalVariablePreconditionAssignments);
            
            foreach (Assignment oAssignment in preconditions)
            {
                if (oAssignment.AssignmentCode.Contains("state_.") || oAssignment.AssignmentCode.Contains("state__."))
                {
                    errors.Add(plpDescription + ", 'GlobalVariablePreconditionAssignments' and 'PlannerAssistancePreconditionsAssignments' cannot be dependent on 'state_' or 'stat__'" +
                    "(the state after extrinsic environment changes) or 'state__' (the next state, also after module effects), " +
                    "see AssignmentCode='" + oAssignment.AssignmentCode + "'!");
                }
            }

            tempErrors.Clear();
            plp.Preconditions_ViolatingPreconditionPenalty = bPlp.Contains("Preconditions") &&
                    bPlp["Preconditions"].AsBsonDocument.Contains("ViolatingPreconditionPenalty") ?
                    GetIntFieldFromBson(plp, bPlp, "Preconditions", "ViolatingPreconditionPenalty", out tempErrors) : 0;
            errors.AddRange(tempErrors);

            tempErrors.Clear();
            List<Assignment> moduleExecutionTimeAssignments = !bPlp.Contains("ModuleExecutionTimeDynamicModel") ? new List<Assignment>() : LoadAssignment(bPlp["ModuleExecutionTimeDynamicModel"].AsBsonArray, plp.Name, plp.Type, out tempErrors, EStateType.ePreviousState);
            errors.AddRange(tempErrors);
            plp.ModuleExecutionTimeDynamicModel.AddRange(moduleExecutionTimeAssignments);
            foreach (Assignment oAssignment in plp.ModuleExecutionTimeDynamicModel)
            {
                if (oAssignment.AssignmentCode.Contains("state_.") || oAssignment.AssignmentCode.Contains("state__."))
                {
                    errors.Add(plpDescription + ", 'ModuleExecutionTimeDynamicModel' cannot be dependent on 'state_' " +
                    "(the state after extrinsic environment changes) or 'state__' (the next state, also after module effects), " +
                    "see AssignmentCode='" + oAssignment.AssignmentCode + "'!");
                }
            }


            if (plp.ModuleExecutionTimeDynamicModel.Count > 0)
            {
                if (plp.ModuleExecutionTimeDynamicModel.Where(x => x.AssignmentCode.Contains(MODULE_EXECUTION_TIME_VARIABLE_NAME + "=")).ToList().Count == 0)
                {
                    errors.Add(plpDescription + ", 'ModuleExecutionTimeDynamicModel' is defined but there is no assignment for '" + MODULE_EXECUTION_TIME_VARIABLE_NAME + "'!");
                }
            }

            tempErrors.Clear();
            List<Assignment> nextStateAssignments = LoadAssignment(bPlp["DynamicModel"]["NextStateAssignments"].AsBsonArray, plp.Name, plp.Type, out tempErrors, EStateType.eNextState);
            errors.AddRange(tempErrors);
            plp.DynamicModel_VariableAssignments.AddRange(nextStateAssignments);

            
            if (bPlp.Contains("StateGivenObservationModel") && bPlp["StateGivenObservationModel"].AsBsonDocument.Contains("Assignments"))
            {
                tempErrors.Clear();
                List<Assignment> stateGivenObservationAssignments = LoadAssignment(bPlp["StateGivenObservationModel"]["Assignments"].AsBsonArray, plp.Name, plp.Type, out tempErrors, EStateType.eNextState);
                errors.AddRange(tempErrors);
                plp.StateGivenObservationModel_VariableAssignments.AddRange(stateGivenObservationAssignments);
            }
            return plp;
        }
        private ModuleDocumentationFile ProcessModuleDocumentationFile(BsonDocument bPlp, out List<string> errors)
        {
            errors = new List<string>();
            List<string> tempErrors = new List<string>();
            ModuleDocumentationFile docFile = null;
            try
            {
                docFile = bPlp["PlpMain"]["Type"].ToString().Equals(PLP_TYPE_NAME_PLP) ? new PLP() : bPlp["PlpMain"]["Type"].ToString().Equals(PLP_TYPE_NAME_GLUE) ? new RosGlue() : new ModuleDocumentationFile();

                docFile.Name = bPlp["PlpMain"]["Name"].ToString();
                docFile.Type = bPlp["PlpMain"]["Type"].ToString();
                docFile.Project = bPlp["PlpMain"]["Project"].ToString();

                switch (docFile.Type)
                {
                    case PLP_TYPE_NAME_PLP:
                        return ProcessPLP(bPlp, out errors, (PLP)docFile);
                    case PLP_TYPE_NAME_GLUE:
                        return ProcessRosGlue(bPlp, out errors, (RosGlue)docFile);
                }
                return docFile;
            }
            catch (Exception e)
            {
                string plpDescription = docFile == null ? "" : GetPLPDescriptionForError(docFile) + ", ";
                errors.Add(PLPsData.GetFatalErrorMsg(plpDescription, e));
                return null;
                
            }
        }

        private int GetIntFieldFromBson(string fromFile, string fileType, BsonDocument bson, string firstField, string secondField, out List<string> errors)
        {
            errors = new List<string>();
            int result = int.MinValue;
            string fileDesc = GetPLPDescriptionForError(fromFile, fileType);

            if (!bson.Contains(firstField))
            {
                string errorMsg = "field '" + firstField + "' was expected but missing!";
                errors.Add(fileDesc + ", " + errorMsg);
                return result;
            }
            if (secondField != null && !bson[firstField].AsBsonDocument.Contains(secondField))
            {
                string errorMsg = "field '" + firstField + "." + secondField + "' was expected but missing !";
                errors.Add(fileDesc + ", " + errorMsg);
                return result;
            }
            BsonValue bVal = secondField == null ? bson[firstField] : bson[firstField][secondField];

            if (!bVal.IsInt32)
            {
                string errorMsg = "field '" + firstField + (secondField != null ? "." + secondField : "") + "' should contain " + PLPsData.INT_VARIABLE_TYPE_NAME + " (but contains \"" + bVal.ToString() + "\")!";
                errors.Add(fileDesc + ", " + errorMsg);
                return result;
            }
            return bson[firstField][secondField].AsInt32;
        }


        private bool? GetBoolFieldFromBsonNullIfNotThere(string fromFile, string fileType, BsonDocument bson, string firstField, string secondField, out List<string> errors)
        {
            errors = new List<string>();
            bool? result = null;
            string fileDesc = GetPLPDescriptionForError(fromFile, fileType);

            if (!bson.Contains(firstField))
            {
                return null;
            }
            if (secondField != null && !bson[firstField].AsBsonDocument.Contains(secondField))
            {
                return null;
            }

            BsonValue bVal = secondField == null ? bson[firstField] : bson[firstField][secondField];

            if (!bVal.IsBoolean)
            {
                string errorMsg = "field '" + firstField + (secondField != null ? "." + secondField : "") + "' should contain " + PLPsData.BOOL_VARIABLE_TYPE_NAME + " (but contains \"" + bVal.ToString() + "\")!";
                errors.Add(fileDesc + ", " + errorMsg);
                return result;
            }
            return secondField == null ? bson[firstField].AsBoolean : bson[firstField][secondField].AsBoolean;
        }
        private int GetIntFieldFromBson(PLP plp, BsonDocument bson, string firstField, string secondField, out List<string> errors)
        {
            return GetIntFieldFromBson(plp.Name, plp.Type, bson, firstField, secondField, out errors);
        }
        private List<Assignment> LoadAssignment(BsonArray bAssignmentsArray, string plpName, string plpType, out List<string> errors, EStateType assignmentLatestPointInTime, bool isCheckPrecondition = false)
        {
            List<Assignment> assignments = new List<Assignment>();
            string plpDescription = GetPLPDescriptionForError(plpName, plpType);
            errors = new List<string>();
            foreach (BsonValue bVal in bAssignmentsArray)
            {
                BsonDocument docAssignment = bVal.AsBsonDocument;
                Assignment oAssignment = new Assignment();
                oAssignment.LatestReachableState = assignmentLatestPointInTime;
                oAssignment.AssignmentName = docAssignment.Contains("AssignmentName") ? docAssignment["AssignmentName"].ToString() : "";

                //original line
                //oAssignment.AssignmentCode = docAssignment.Contains("AssignmentCode") ? docAssignment["AssignmentCode"].ToString().Replace(" ", "") : "";
                //oAssignment.AssignmentCode = docAssignment.Contains("AssignmentCode") ? docAssignment["AssignmentCode"].ToString() : "";//OR Wertheim removed the replace(" ","")
                oAssignment.AssignmentCode = GetBsonStringOrStringArrayField(docAssignment, "AssignmentCode");//Or Wertheim changed to allow string arrays as code lines
                string iterateVar = docAssignment.Contains("IteratePreviousStateVars") ? "IteratePreviousStateVars" :
                    docAssignment.Contains("IterateNextStateVars") ? "IterateNextStateVars" : null;

                if(iterateVar != null)
                {
                    
                    BsonArray bIterationsArray = docAssignment[iterateVar].AsBsonArray;
                    foreach (BsonValue bIter in bIterationsArray)
                    {
                        IterateStateVars oIter = new IterateStateVars();
                        oIter.ItemInMutableFunction = !isCheckPrecondition;
                        BsonDocument docIter = bIter.AsBsonDocument;
                        oIter.Type = GetBsonStringField(docIter, "Type");
                        oIter.ItemName = GetBsonStringField(docIter, "ItemName");
                        oIter.ConditionCode = GetBsonStringField(docIter, "ConditionCode");
                        oIter.WhenConditionTrueCode = GetBsonStringField(docIter, "WhenConditionTrueCode");
                        oIter.StateType = iterateVar.Equals("IteratePreviousStateVars") ? EStateType.ePreviousState
                                :iterateVar.Equals("IterateNextStateVars") ? EStateType.eNextState : 
                                iterateVar.Equals("IterateAfterExtrinsicChangesStateVars") ? EStateType.eAfterExtrinsicChangesState : EStateType.eError;

                        if(string.IsNullOrEmpty(oIter.Type) || string.IsNullOrEmpty(oIter.ItemName))
                        {
                            errors.Add(plpDescription + ", '"+iterateVar+"', fields 'Type' and 'ItemName' are mandatory!");
                        }

                        if(assignmentLatestPointInTime.Equals(EStateType.ePreviousState) && oIter.StateType != EStateType.ePreviousState)
                        {
                            errors.Add(plpDescription + ", '"+iterateVar+"', in this area you can only use 'IteratePreviousStateVars'!");
                        }

                         if(assignmentLatestPointInTime.Equals(EStateType.eAfterExtrinsicChangesState) && oIter.StateType == EStateType.eNextState)
                        {
                            errors.Add(plpDescription + ", '"+iterateVar+"', in this area you cannot iterate the next state with 'IterateNextStateVars'!");
                        }

                        oAssignment.IterateStateVariables.Add(oIter);
                    }
                }
                if (docAssignment.Contains("TempVar"))
                {


                    BsonDocument docTemp = docAssignment["TempVar"].AsBsonDocument;

                    oAssignment.TempVariable.VariableName = GetBsonStringField(docTemp, "VarName");
                    if (oAssignment.TempVariable.VariableName == null)
                    {
                        errors.Add(plpDescription + ", 'TempVar', field 'VarName' is mandatory (when 'TempVar' is defined)!");
                    }

                    oAssignment.TempVariable.Type = GetBsonStringField(docTemp, "Type");
                    if (oAssignment.TempVariable.Type == null)
                    {
                        errors.Add(plpDescription + ", 'TempVar', field 'Type' is mandatory (when 'TempVar' is defined)!");
                    }

                    if (oAssignment.TempVariable.Type != ENUM_VARIABLE_TYPE_NAME && oAssignment.TempVariable.Type != "bool" && 
                        oAssignment.TempVariable.Type != "int" && 
                         oAssignment.TempVariable.Type != "float" &&
                          oAssignment.TempVariable.Type != "double" &&
                          GlobalEnumTypes.Where(x => x.TypeName.Equals(oAssignment.TempVariable.Type)).FirstOrDefault() == null)
                    {
                        errors.Add(plpDescription + ", 'TempVar', valid values for field 'Type' are: 'enum','int', 'float', 'double' or 'bool'!");
                    }
                    if (oAssignment.TempVariable.Type == ENUM_VARIABLE_TYPE_NAME)
                    {
                        oAssignment.TempVariable.EnumName = GetBsonStringField(docTemp, "EnumName");
                        if (oAssignment.TempVariable.EnumName == null)
                        {
                            errors.Add(plpDescription + ", 'TempVar', field 'EnumName' is mandatory (when 'Type' is 'enum')!");
                        }

                        List<string> enumErrors;
                        oAssignment.TempVariable.EnumValues.AddRange(GetEnumValues(docTemp, "EnumValues", out enumErrors));
                        errors.AddRange(enumErrors);
                    }

                }
                assignments.Add(oAssignment);
            }
            return assignments;
        }
    }

    public class EnumVarTypePLP:BaseGlobalVarType
    {
        public bool IsCompundType
        {
            get => false;
        }
        public List<string> Values;

        public EnumVarTypePLP()
        {
            Values = new List<string>();
        }
    }
    public class BaseGlobalVarType
    {
        public string TypeName;
        public bool IsCompundType;
    }
    public class CompoundVarTypePLP:BaseGlobalVarType
    {
        public bool IsCompundType
        {
            get => true;
        }
        public List<CompoundVarTypePLP_Variable> Variables;

        public CompoundVarTypePLP()
        {
            Variables = new List<CompoundVarTypePLP_Variable>();
        }
    }

    public class LocalVariableTypePLP
    {
        public string TypeName;
        public List<LocalVariableCompoundTypeField> SubFields;

        public LocalVariableTypePLP()
        {
            SubFields = new List<LocalVariableCompoundTypeField>();
        }
    }

    public class LocalVariableCompoundTypeField
    {
        public string FieldName;
        public string FieldType;
    }
    public class CompoundVarTypePLP_Variable
    {
        public string Name;
        public string Type;
        public string Default;
        public float? ML_MaxPossibleValue;

        public bool ML_IgnoreVariable;


        public string UnderlineLocalVariableType;

        public CompoundVarTypePLP_Variable()
        {
            ML_IgnoreVariable=false;
        }
    }

    public class GlobalVariableDeclaration
    {
        public string Name;
        public string Type;
        public string Default;
        public string DefaultCode;
        public bool IsActionParameterValue;
        public bool IsArray;
        public float? ML_MaxPossibleValue;

        public bool ML_IgnoreVariable;

        public List<GlobalVariableDeclaration> SubCompoundFeilds = new List<GlobalVariableDeclaration>();
        public string StateVariableName;
        public string UnderlineLocalVariableType;
        public ParameterizedGlobalVariableData ParametersData = new ParameterizedGlobalVariableData(); 
        public GlobalVariableDeclaration()
        {
            ML_IgnoreVariable=false;
        }
        public string GetVariableDefinition()
        {
            if(IsArray)
            {

            }
            if(this.ParametersData.Parameters.Count() > 0)
            {
                return "std::map<tuple<" + string.Join(",", this.ParametersData.Parameters.Select(x=> x.EnumParameterType == null ? "int" : x.EnumParameterType.TypeName).ToList()) + ">,"+this.Type+"> " + this.Name + ";";
            }
            else
            {
                return (IsArray ? "vector<" : "") + this.Type +(IsArray?">":"")+ " " + this.Name + ";";
            } 
        }
    }

    public class ParameterizedGlobalVariableData
    {
        public string IncludeParametersCode;
        public List<GlobalVariableDeclarationParameter> Parameters = new List<GlobalVariableDeclarationParameter>(); 

        
    }

    public class GlobalVariableDeclarationParameter
    {
        public int Start=0;
        public int Stop=0;
        public int Step=0;
        public EnumVarTypePLP EnumParameterType = null;
    }

    public class LocalVariableConstant
    {
        public string Name;
        public string Type;
        public string InitCode;

    }

    public class SpecialState
    {
        public List<Assignment> StateFunctionCode = new List<Assignment>();
        public string StateConditionCode;
        public double Reward;
        public bool IsGoalState;

        public bool IsOneTimeReward;
    }

    public enum DistributionType { Normal, Discrete, Uniform };
    public class DistributionSample
    {
        public string C_VariableName;
        public string FunctionDescription;
        public DistributionType Type;
        public List<string> Parameters;
        public bool HasParameterAsGlobalType = false;
        public string FromFile;

        public DistributionSample()
        {
            Parameters = new List<string>();
        }
    }
}
