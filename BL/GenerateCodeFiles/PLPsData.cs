using MongoDB.Bson;
using System;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using WebApiCSharp.Services;
using WebApiCSharp.Models;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace WebApiCSharp.GenerateCodeFiles
{
    public class PLPsData
    {
        #region public PLP data
        public double MaxReward;
        public double MinReward;

        public int NumberOfActions = -1;

        public Dictionary<string, DistributionSample> DistributionSamples = new Dictionary<string, DistributionSample>();
        public Dictionary<string, PLP> PLPs = new Dictionary<string, PLP>();
        public List<SpecialState> SpecialStates = new List<SpecialState>();
        public List<LocalVariableConstant> LocalVariableConstants = new List<LocalVariableConstant>();
        public List<LocalVariableTypePLP> LocalVariableTypes = new List<LocalVariableTypePLP>();
        public string ProjectName { get; set; }
        public string ProjectNameWithCapitalLetter { get; set; }
        public List<Assignment> InitialBeliefAssignments = new List<Assignment>();
        public List<EnumVarTypePLP> GlobalEnumTypes = new List<EnumVarTypePLP>();
        public List<CompoundVarTypePLP> GlobalCompoundTypes = new List<CompoundVarTypePLP>();

        public List<GlobalVariableDeclaration> GlobalVariableDeclarations = new List<GlobalVariableDeclaration>();

        #endregion
        public const string PLP_TYPE_NAME_ENVIRONMENT = "Environment";
        public const string PLP_TYPE_NAME_ENVIRONMENT_GLUE = "EnvironmentGlue";

        public const string PLP_TYPE_NAME_GLUE = "Glue";

        public const string PLP_TYPE_NAME_PLP = "PLP";

        public const string ANY_VALUE_TYPE_NAME = "AnyValue";
        public const string MODULE_EXECUTION_TIME_VARIABLE_NAME = "__moduleExecutionTime";
        public const string MODULE_REWARD_VARIABLE_NAME = "__reward";

        public const string DISCRETE_DISTRIBUTION_FUNCTION_NAME = "AOS.SampleDiscrete"; //AOS.SampleDiscrete(enumRealCase,{0.8, 0.1,0,0.1})
        public const string NORMAL_DISTRIBUTION_FUNCTION_NAME = "AOS.SampleNormal"; //AOS.SampleNormal(40000,10000)
        public const string UNIFORM_DISTRIBUTION_FUNCTION_NAME = "AOS.SampleUniform"; //AOS.SampleUniform(40000,10000)
        #region private variables 
        private string tempPlpName = "";
        private string tempPlpType = "";
        private BsonDocument environmentPLP = null;
        private BsonDocument environmentGlue = null;
        private Dictionary<string, BsonDocument> bsonPlps = new Dictionary<string, BsonDocument>();
        private Dictionary<string, BsonDocument> bsonGlues = new Dictionary<string, BsonDocument>();
        #endregion

        public PLPsData(out List<string> errors)
        {
            errors = new List<string>();
            List<BsonDocument> bPLPs = PLPsService.GetAll();
            foreach (BsonDocument plp in bPLPs)
            {
                errors.AddRange(AddPLPSetProjectName(plp));
                if (errors.Count > 0) return;
            }

            errors.AddRange(ProcessEnvironmentFile());

            ProcessEnvironmentGlueFile();

            foreach (KeyValuePair<string, BsonDocument> plp in bsonPlps)
            {
                List<string> tempErrors = new List<string>();
                PLPs.Add(plp.Key, ProcessPLP(plp.Value, out tempErrors));
                errors.AddRange(tempErrors);
            }

            List<string> allCodeSections = GetC_CodeSections();
            foreach (DistributionType distType in Enum.GetValues(typeof(DistributionType)))
            {
                GetSampleDistributionFunctions(allCodeSections, distType);
            }

            GetMaxMinReward();
        }
        //AOS.SampleDiscrete(enumRealCase,{0.8, 0.1,0,0.1})//AOS.SampleNormal(40000,10000)//AOS.SampleUniform(40000,10000)
        private void GetSampleDistributionFunctions(List<string> allCodeSections, DistributionType type)
        {
            foreach (string code in allCodeSections)
            {
                string c = code.Replace(" ", "");
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
                            dist.Parameters.AddRange(dist.FunctionDescription.Substring(dist.FunctionDescription.IndexOf("{")).Replace("{", "").Replace("}", "").Split(","));
                        }

                        if (!DistributionSamples.ContainsKey(dist.FunctionDescription))
                        {
                            DistributionSamples.Add(dist.FunctionDescription, dist);
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
                    DistributionSamples[key].C_VariableName = nameBase + i.ToString();
                    i++;
                }
            }
        }

        private List<string> GetC_CodeSections()
        {
            List<string> codeSections = new List<string>();
            foreach (PLP plp in PLPs.Values)
            {
                foreach (Assignment assign in plp.DynamicModel_VariableAssignments)
                {
                    codeSections.Add(assign.AssignmentCode);
                }

                foreach (Assignment assign in plp.ModuleExecutionTimeDynamicModel)
                {
                    codeSections.Add(assign.AssignmentCode);
                }
            }

            foreach (Assignment assign in InitialBeliefAssignments)
            {
                codeSections.Add(assign.AssignmentCode);
            }

            foreach (GlobalVariableDeclaration globalVarDec in GlobalVariableDeclarations)
            {
                if (globalVarDec.DefaultCode != null)
                {
                    codeSections.Add(globalVarDec.DefaultCode);
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
            foreach (BsonValue bType in environmentGlue["LocalVariableTypes"].AsBsonArray)
            {
                LocalVariableTypePLP localVar = new LocalVariableTypePLP();
                BsonDocument docType = bType.AsBsonDocument;

                localVar.TypeName = docType["TypeName"].ToString();

                foreach (BsonValue bVar in docType["Variables"].AsBsonArray)
                {
                    BsonDocument docVar = bVar.AsBsonDocument;
                    localVar.VariablesNameAndType.Add(new KeyValuePair<string, string>(docVar["Name"].ToString(), docVar["Type"].ToString()));
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
        private List<string> ProcessEnvironmentFile()
        {
            List<string> errors = new List<string>();
            List<string> tempErrors;
            GetEnvironmentTypes(out tempErrors);
            errors.AddRange(tempErrors);
            errors.AddRange(GetGlobalVariablesDeclaration());
            GetBeliefStateAssignmentsAndSpecialStates();
            return errors;
        }

        private void GetBeliefStateAssignmentsAndSpecialStates()
        {
            foreach (BsonValue bVal in environmentPLP["InitialBeliefStateAssignments"].AsBsonArray)
            {
                BsonDocument docAssignment = bVal.AsBsonDocument;
                Assignment assignment = new Assignment() { AssignmentName = docAssignment["AssignmentName"].ToString(), AssignmentCode = docAssignment["AssignmentCode"].ToString().Replace(" ", "") };
                InitialBeliefAssignments.Add(assignment);
            }


            foreach (BsonValue bState in environmentPLP["SpecialStates"].AsBsonArray)
            {
                BsonDocument docState = bState.AsBsonDocument;
                SpecialState spState = new SpecialState();
                spState.IsGoalState = docState["IsGoalState"].AsBoolean;
                spState.StateConditionCode = docState["StateConditionCode"].ToString();
                spState.Reward = docState["Reward"].AsDouble;
                SpecialStates.Add(spState);
            }
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
            foreach (BsonValue bVal in environmentPLP["GlobalVariableTypes"].AsBsonArray)
            {
                BsonDocument doc = bVal.AsBsonDocument;
                switch (doc["Type"].ToString())
                {
                    case "enum":
                        EnumVarTypePLP enu = new EnumVarTypePLP();

                        enu.TypeName = doc["TypeName"].ToString();
                        enu.Values.AddRange(GetEnumValues(doc, "EnumValues", out errors));

                        GlobalEnumTypes.Add(enu);
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
                            comp.Variables.Add(oVar);
                        }
                        GlobalCompoundTypes.Add(comp);
                        break;
                }
            }
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
                oVarDec.IsActionParameter = docVar.Contains("IsActionParameter") ? docVar["IsActionParameter"].AsBoolean : false;

                GlobalVariableDeclarations.Add(oVarDec);
                if (oVarDec.DefaultCode != null && oVarDec.Default != null)
                {
                    errors.Add("PLP type='" + PLP_TYPE_NAME_ENVIRONMENT + "', Section='GlobalVariablesDeclaration', variable Name='" + oVarDec.Name +
                    "', 'DefaultCode' and for 'Default' are defined, only one of them can be defined!");
                }
            }

            return errors;
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
        private PLP ProcessPLP(BsonDocument bPlp, out List<string> errors)
        {
            errors = new List<string>();
            PLP plp = new PLP();
            plp.Name = bPlp["PlpMain"]["Name"].ToString();
            plp.Type = bPlp["PlpMain"]["Type"].ToString();
            plp.Project = bPlp["PlpMain"]["Project"].ToString();

            string plpDescription = "PLP name='" + plp.Name + "',type = '" + plp.Type + "'";
            foreach (BsonValue bVal in bPlp["GlobalVariableModuleParameters"].AsBsonArray)
            {
                BsonDocument docPar = bVal.AsBsonDocument;
                GlobalVariableModuleParameter oPar = new GlobalVariableModuleParameter();

                oPar.Name = docPar["Name"].ToString();
                oPar.Type = docPar["Type"].ToString();
                oPar.UnderlineLocalVariableType = GetBsonStringField(docPar, "UnderlineLocalVariableType");

                if (oPar.UnderlineLocalVariableType != null && oPar.Type != ANY_VALUE_TYPE_NAME)
                {
                    errors.Add(plpDescription + ", in 'GlobalVariableModuleParameters',  'UnderlineLocalVariableType' is defined while type '" + oPar.Type + "' is not '" + ANY_VALUE_TYPE_NAME + "'!");
                }
                plp.GlobalVariableModuleParameters.Add(oPar);
            }

            foreach (BsonValue bVal in bPlp["LocalVariablesInitializationFromGlobalVariables"].AsBsonArray)
            {
                BsonDocument docVar = bVal.AsBsonDocument;
                LocalVariablesInitializationFromGlobalVariable oVar = new LocalVariablesInitializationFromGlobalVariable();

                oVar.FromGlobalVariable = docVar["FromGlobalVariable"].ToString();
                oVar.InputLocalVariable = docVar["InputLocalVariable"].ToString();
                plp.LocalVariablesInitializationFromGlobalVariables.Add(oVar);
            }

            foreach (BsonValue bVal in bPlp["ModuleResponse"]["EnumResponse"].AsBsonArray)
            {
                if (plp.EnumResponse.Contains(bVal.ToString()))
                {
                    errors.Add(plpDescription + ", 'EnumResponse' contains duplicate values ('" + bVal.ToString() + "')");
                }
                plp.EnumResponse.Add(bVal.ToString());
                if (bVal.ToString().Contains(" ") || bVal.ToString().Contains("*"))
                {
                    errors.Add(plpDescription + ", in 'EnumResponse', response value cannot contain spaces or special characters");
                }
            }

            plp.ResponseType = bPlp["ModuleResponse"]["Type"].ToString();


            foreach (BsonValue bVal in bPlp["ModuleResponse"]["ResponseRules"].AsBsonArray)
            {
                BsonDocument docResponseRule = bVal.AsBsonDocument;
                ResponseRule oResponseRule = new ResponseRule();

                oResponseRule.Condition = docResponseRule["ConditionCodeWithLocalVariables"].ToString();
                oResponseRule.Response = docResponseRule["Response"].ToString();
                oResponseRule.Comment = GetBsonStringField(docResponseRule, "Comment");
                plp.ResponseRules.Add(oResponseRule);
            }

            plp.Preconditions_GlobalVariableConditionCode = GetBsonStringField(bPlp, "Preconditions", "GlobalVariableConditionCode");
            plp.Preconditions_GlobalVariableConditionCode = plp.Preconditions_GlobalVariableConditionCode == null ? "true" : plp.Preconditions_GlobalVariableConditionCode;

            plp.Preconditions_PlannerAssistancePreconditions = GetBsonStringField(bPlp, "Preconditions", "PlannerAssistancePreconditions");
            plp.Preconditions_PlannerAssistancePreconditions = plp.Preconditions_PlannerAssistancePreconditions == null ? "true" : plp.Preconditions_PlannerAssistancePreconditions;


            plp.Preconditions_ViolatingPreconditionPenalty = bPlp.Contains("Preconditions") &&
                    bPlp["Preconditions"].AsBsonDocument.Contains("ViolatingPreconditionPenalty") ?
                    bPlp["Preconditions"]["ViolatingPreconditionPenalty"].AsInt32 : 0;

            foreach (BsonValue bVal in bPlp["ModuleExecutionTimeDynamicModel"].AsBsonArray)
            {
                BsonDocument docAssignment = bVal.AsBsonDocument;
                Assignment oAssignment = new Assignment();

                oAssignment.AssignmentName = docAssignment["AssignmentName"].ToString();
                oAssignment.AssignmentCode = docAssignment["AssignmentCode"].ToString().Replace(" ", "");

                if (docAssignment.Contains("TempVarType"))
                {
                    BsonDocument docTemp = docAssignment["TempVarType"].AsBsonDocument;
                    TempVarType oTemp = new TempVarType();
                    oTemp.Type = GetBsonStringField(docTemp, "Type");
                    if (oTemp.Type == null)
                    {
                        errors.Add(plpDescription + ", 'TempVarType', field 'Type' is mandatory (when 'TempVarType' is defined)!");
                    }

                    if (oTemp.Type != "enum" && oTemp.Type != "bool" && oTemp.Type != "int")
                    {
                        errors.Add(plpDescription + ", 'TempVarType', valid values for field 'Type' are: 'enum','int' or 'bool'!");
                    }
                    if (oTemp.Type == "enum")
                    {
                        oTemp.EnumName = GetBsonStringField(docTemp, "EnumName");
                        if (oTemp.EnumName == null)
                        {
                            errors.Add(plpDescription + ", 'TempVarType', field 'EnumName' is mandatory (when 'Type' is 'enum')!");
                        }

                        List<string> enumErrors;
                        oTemp.EnumValues.AddRange(GetEnumValues(docTemp, "EnumValues", out enumErrors));
                        errors.AddRange(enumErrors);
                    }

                }
                plp.ModuleExecutionTimeDynamicModel.Add(oAssignment);
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

            foreach (BsonValue bVal in bPlp["DynamicModel"]["NextStateAssignments"].AsBsonArray)
            {
                BsonDocument docAssignment = bVal.AsBsonDocument;
                Assignment oAssignment = new Assignment();

                oAssignment.AssignmentName = docAssignment["AssignmentName"].ToString();
                oAssignment.AssignmentCode = docAssignment["AssignmentCode"].ToString().Replace(" ", "");

                if (docAssignment.Contains("TempVarType"))
                {
                    BsonDocument docTemp = docAssignment["TempVarType"].AsBsonDocument;
                    TempVarType oTemp = new TempVarType();
                    oTemp.Type = GetBsonStringField(docTemp, "Type");
                    if (oTemp.Type == null)
                    {
                        errors.Add(plpDescription + ", 'TempVarType', field 'Type' is mandatory (when 'TempVarType' is defined)!");
                    }

                    if (oTemp.Type != "enum" && oTemp.Type != "bool" && oTemp.Type != "int")
                    {
                        errors.Add(plpDescription + ", 'TempVarType', valid values for field 'Type' are: 'enum','int' or 'bool'!");
                    }
                    if (oTemp.Type == "enum")
                    {
                        oTemp.EnumName = GetBsonStringField(docTemp, "EnumName");
                        if (oTemp.EnumName == null)
                        {
                            errors.Add(plpDescription + ", 'TempVarType', field 'EnumName' is mandatory (when 'Type' is 'enum')!");
                        }

                        List<string> enumErrors;
                        oTemp.EnumValues.AddRange(GetEnumValues(docTemp, "EnumValues", out enumErrors));
                        errors.AddRange(enumErrors);
                    }

                }
                plp.DynamicModel_VariableAssignments.Add(oAssignment);
            }
            return plp;
        }
    }

    public class EnumVarTypePLP
    {
        public string TypeName;
        public List<string> Values;

        public EnumVarTypePLP()
        {
            Values = new List<string>();
        }
    }

    public class CompoundVarTypePLP
    {
        public string TypeName;
        public List<CompoundVarTypePLP_Variable> Variables;

        public CompoundVarTypePLP()
        {
            Variables = new List<CompoundVarTypePLP_Variable>();
        }
    }

    public class LocalVariableTypePLP
    {
        public string TypeName;
        public List<KeyValuePair<string, string>> VariablesNameAndType;

        public LocalVariableTypePLP()
        {
            VariablesNameAndType = new List<KeyValuePair<string, string>>();
        }
    }

    public class CompoundVarTypePLP_Variable
    {
        public string Name;
        public string Type;
        public string Default;
    }

    public class GlobalVariableDeclaration
    {
        public string Name;
        public string Type;
        public string Default;
        public string DefaultCode;
        public bool IsActionParameter;

    }

    public class LocalVariableConstant
    {
        public string Name;
        public string Type;
        public string InitCode;

    }

    public class SpecialState
    {
        public string StateConditionCode;
        public double Reward;
        public bool IsGoalState;
    }

    public enum DistributionType { Normal, Discrete, Uniform };
    public class DistributionSample
    {
        public string C_VariableName;
        public string FunctionDescription;
        public DistributionType Type;
        public List<string> Parameters;

        public DistributionSample()
        {
            Parameters = new List<string>();
        }
    }
}