using MongoDB.Bson;
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

        public List<SpecialState> SpecialStates = new List<SpecialState>();
        public List<LocalVariableConstant> LocalVariableConstants = new List<LocalVariableConstant>();
        public List<LocalVariableTypePLP> LocalVariableTypes = new List<LocalVariableTypePLP>();
        public string ProjectName { get; set; }
        public List<KeyValuePair<string, string>> InitialBeliefAssignments = new List<KeyValuePair<string, string>>();
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

        #region private variables
        private string tempPlpName = "";
        private string tempPlpType = "";
        private BsonDocument environmentPLP = null;
        private BsonDocument environmentGlue = null;
        private Dictionary<string, BsonDocument> plps = new Dictionary<string, BsonDocument>();
        private Dictionary<string, BsonDocument> glues = new Dictionary<string, BsonDocument>();
        #endregion
        private List<string> AddPLPSetProjectName(BsonDocument plp)
        {
            List<string> errors = new List<string>();
            if (ProjectName == null)
            {
                tempPlpName = plp["PlpMain"]["Name"].ToString();
                tempPlpType = plp["PlpMain"]["Type"].ToString();
                ProjectName = plp["PlpMain"]["Project"].ToString();
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
                    if (plps.ContainsKey(plp["PlpMain"]["Name"].ToString()))
                    {
                        errors.Add("There are two PLPs of name '" + plp["PlpMain"]["Name"] + "' and type '" + PLP_TYPE_NAME_PLP + "', only one is allowed!");
                    }
                    else
                    {
                        plps.Add(plp["PlpMain"]["Name"].ToString(), plp);
                    }
                    break;
                case PLP_TYPE_NAME_GLUE:
                    if (glues.ContainsKey(plp["PlpMain"]["Name"].ToString()))
                    {
                        errors.Add("There are two PLPs of name '" + plp["PlpMain"]["Name"] + "' and type '" + PLP_TYPE_NAME_GLUE + "', only one is allowed!");
                    }
                    else
                    {
                        glues.Add(plp["PlpMain"]["Name"].ToString(), plp);
                    }
                    break;
            }

            return errors;
        }

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

            ProcessPLP(plps["pick"], out errors);
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
            GetEnvironmentTypes();
            errors.AddRange(GetGlobalVariablesDeclaration());
            GetBeliefStateAssignmentsAndSpecialStates();
            return errors;
        }

        private void GetBeliefStateAssignmentsAndSpecialStates()
        {
            foreach (BsonValue bVal in environmentPLP["InitialBeliefStateAssignments"].AsBsonArray)
            {
                BsonDocument docAssignment = bVal.AsBsonDocument;
                KeyValuePair<string, string> assignment = new KeyValuePair<string, string>(docAssignment["AssignmentName"].ToString(), docAssignment["AssignmentCode"].ToString().Replace(" ", ""));
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
                if (oVarDec.DefaultCode != null && oVarDec.DefaultCode != null)
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

                if(oPar.UnderlineLocalVariableType != null && oPar.Type != ANY_VALUE_TYPE_NAME)
                {
                    errors.Add(plpDescription+", in 'GlobalVariableModuleParameters',  'UnderlineLocalVariableType' is defined while type '" + oPar.Type + "' is not '" + ANY_VALUE_TYPE_NAME + "'!");
                }
                plp.GlobalVariableModuleParameters.Add(oPar);
            }

            foreach (BsonValue bVal in bPlp["GlobalVariableModuleParameters"].AsBsonArray)
            {
                BsonDocument docVar = bVal.AsBsonDocument;
                LocalVariablesInitializationFromGlobalVariable oVar = new LocalVariablesInitializationFromGlobalVariable();

                oVar.FromGlobalVariable = docVar["FromGlobalVariable"].ToString();
                oVar.InputLocalVariable = docVar["InputLocalVariable"].ToString();
                plp.LocalVariablesInitializationFromGlobalVariables.Add(oVar);
            }

            foreach (BsonValue bVal in bPlp["ModuleResponse"]["EnumResponse"].AsBsonArray)
            {
                if(plp.EnumResponse.Contains(bVal.ToString()))
                {
                    errors.Add(plpDescription + ", 'EnumResponse' contains duplicate values ('" + bVal.ToString() + "')");
                }
                plp.EnumResponse.Add(bVal.ToString());
                if(bVal.ToString().Contains(" ") || bVal.ToString().Contains("*"))
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
                oAssignment.AssignmentCode = docAssignment["AssignmentCode"].ToString().Replace(" ","");
                
                if(docAssignment.Contains("TempVarType"))
                {
                    BsonDocument docTemp = docAssignment["TempVarType"].AsBsonDocument;
                    TempVarType oTemp = new TempVarType();
                    oTemp.Type = GetBsonStringField(docTemp, "Type"); 
                    if(oTemp.Type == null)
                    {
                        errors.Add(plpDescription + ", 'TempVarType', field 'Type' is mandatory (when 'TempVarType' is defined)!");
                    }

                    if(oTemp.Type != "enum" && oTemp.Type != "bool" && oTemp.Type != "int")
                    {
                        errors.Add(plpDescription + ", 'TempVarType', valid values for field 'Type' are: 'enum','int' or 'bool'!");
                    }
                    if (oTemp.Type == "enum")
                    {
                        oTemp.EnumName = GetBsonStringField(docTemp, "EnumName");
                        if (oTemp.EnumName == null)
                        {
                            errors.Add(plpDescription + ", 'TempVarType', field 'EnumName' is mandatory (when 'Type' is 'enum')!")
                        }

                        List<string> enumErrors;
                        oTemp.EnumValues.AddRange(GetEnumValues(docTemp, "EnumValues", out enumErrors));
                        errors.AddRange(enumErrors);
                    }

                }
                plp.ModuleExecutionTimeDynamicModel.Add(oAssignment);
                if(oAssignment.AssignmentCode.Contains("state_.") || oAssignment.AssignmentCode.Contains("state__."))
                {
                    errors.Add(plpDescription + ", 'ModuleExecutionTimeDynamicModel' cannot be dependent on 'state_' (the state after extrinsic environment changes) or 'state__' (the next state, also after module effects), see AssignmentCode='"+oAssignment.AssignmentCode+"'!")
                }
                
            }
            if(plp.ModuleExecutionTimeDynamicModel.Count > 0)
            {
                if (plp.ModuleExecutionTimeDynamicModel.Where(x=> x.AssignmentCode.Contains(MODULE_EXECUTION_TIME_VARIABLE_NAME+"=")).ToList().Count > 0)
                {
                    errors.Add(plpDescription + ", 'ModuleExecutionTimeDynamicModel' is defined but there is no assignment for '"+MODULE_EXECUTION_TIME_VARIABLE_NAME+"'!");
                }
            }

            foreach (BsonValue bVal in bPlp["DynamicModel"]["NextStateAssignments"].AsBsonArray)
            {
                BsonDocument docAssignment = bVal.AsBsonDocument;
                Assignment oAssignment = new Assignment();

                oAssignment.AssignmentName = docAssignment["AssignmentName"].ToString();
                oAssignment.AssignmentCode = docAssignment["AssignmentCode"].ToString().Replace(" ","");
                
                if(docAssignment.Contains("TempVarType"))
                {
                    BsonDocument docTemp = docAssignment["TempVarType"].AsBsonDocument;
                    TempVarType oTemp = new TempVarType();
                    oTemp.Type = GetBsonStringField(docTemp, "Type"); 
                    if(oTemp.Type == null)
                    {
                        errors.Add(plpDescription + ", 'TempVarType', field 'Type' is mandatory (when 'TempVarType' is defined)!");
                    }

                    if(oTemp.Type != "enum" && oTemp.Type != "bool" && oTemp.Type != "int")
                    {
                        errors.Add(plpDescription + ", 'TempVarType', valid values for field 'Type' are: 'enum','int' or 'bool'!");
                    }
                    if (oTemp.Type == "enum")
                    {
                        oTemp.EnumName = GetBsonStringField(docTemp, "EnumName");
                        if (oTemp.EnumName == null)
                        {
                            errors.Add(plpDescription + ", 'TempVarType', field 'EnumName' is mandatory (when 'Type' is 'enum')!")
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
}