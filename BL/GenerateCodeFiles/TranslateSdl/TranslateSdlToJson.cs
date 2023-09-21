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
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace WebApiCSharp.JsonTextModel
{
    public static class TranslateSdlToJson
    {
        private static string[] FirstLevelSavedWords = "response: module_activation: rollout_policy: local_variable: available_parameters_code: parameter: precondition: violate_penalty: dynamic_model: extrinsic_code: reward_code: initial_belief: action_parameter: define_type: horizon: discount: state_variable: response_local_variable: project: ".Split(" ");
        public static string Translate(string fileName, string fileContent)
        {
            string fileType = fileName.ToLower().Substring(fileName.LastIndexOf('.')+1);
            switch(fileType)
            {
                case "sd":
                return TranslateSD(fileName.Substring(0, fileName.LastIndexOf('.')), fileContent);
                case "am":
                return TranslateAM(fileName.Substring(0, fileName.LastIndexOf('.')), fileContent);
                case "ef":
                return TranslateEF(fileName.Substring(0, fileName.LastIndexOf('.')), fileContent);
                default:
                throw new Exception("file format not supported for SDL to JSON (file name:'"+fileName+"')");
            }
        }
        private static string RemoveHiddenChar(string str)
        {
            return str.Replace("\t","");
        }
        public static string TranslateSD(string skillName, string fileContent)
        {
            string errorStart = "SD file of skill '"+skillName+"': ";
            SdFile sdFile = new SdFile();
            List<GlobalVariableModuleParameter> GlobalVariableModuleParameters= new List<GlobalVariableModuleParameter>(); 
            
            string[] lineContent = fileContent.Split('\n');
            int i=0;
            int prevI = -1;
            while(i<lineContent.Length)
            { 
                if(i == prevI) i++;
                if(i >= lineContent.Length)break;
                prevI=i;
                if(i==0)
                {
                    if(lineContent[i].Trim().StartsWith("project:"))
                    {
                        string project = lineContent[i].Substring("project:".Length).Replace(" ","");
                        project = RemoveHiddenChar(project);
                        PlpMain main= new PlpMain{Name=skillName,Project=project, Type="PLP"};
                        sdFile.PlpMain=main;  
                        i++; 
                    }
                    else throw new Exception(errorStart + "does not start with 'project: <project_name>'");
                }
                else if(lineContent[i].Trim().StartsWith("available_parameters_code:"))
                {
                    List<string> codeLines = new List<string>();
                    i++;
                    while(i < lineContent.Length && !IsFirstLevelSavedWord(lineContent[i]))codeLines.Add(lineContent[i++]);
                    for(int j=codeLines.Count-1; j>-1;j--)
                    {
                        if(codeLines[j].Replace(" ","").Length == 0)codeLines.RemoveAt(j);
                        else break;
                    }
                    if(codeLines.Count == 0)throw new Exception(errorStart + "code is expected in the lines below 'available_parameters_code' ");
                    sdFile.PossibleParametersValue = new CodeAssignment[]{new CodeAssignment(){AssignmentCode = codeLines.ToArray()}};
                }
                else if(lineContent[i].Trim().StartsWith("parameter:"))
                {  
                    GlobalVariableModuleParameter p = new GlobalVariableModuleParameter();
                    string[] delimiters = {" ",":"};
                    List<string> bits = lineContent[i].Split(delimiters,StringSplitOptions.None).ToList();
                    bits = bits.Select(x=>x.Replace(" ","")).Where(x=> x.Length > 0 && x != "parameter").ToList();
                    if(bits.Count != 2)throw new Exception(errorStart + "a '<type> <name>' must be defined aftter 'parameter:' you wrote '"+lineContent[i]+"'");
                    p.Type = bits[0];
                    p.Name = bits[1];
                    GlobalVariableModuleParameters.Add(p);
                    sdFile.GlobalVariableModuleParameters = GlobalVariableModuleParameters.ToArray();
                }
                else if(lineContent[i].Trim().StartsWith("precondition:"))
                {
                    sdFile.Preconditions = sdFile.Preconditions == null ? new Preconditions() : sdFile.Preconditions; 
                    List<string> codeLines = new List<string>();
                    i++;
                    while(i < lineContent.Length && !IsFirstLevelSavedWord(lineContent[i]))codeLines.Add(lineContent[i++]);
                    for(int j=codeLines.Count-1; j>-1;j--)
                    {
                        if(codeLines[j].Replace(" ","").Length == 0)codeLines.RemoveAt(j);
                        else break;
                    }
                    if(codeLines.Count == 0)throw new Exception(errorStart + "code is expected in the lines below 'precondition' ");
                    sdFile.Preconditions.GlobalVariablePreconditionAssignments = new CodeAssignment[]{new CodeAssignment(){AssignmentCode = codeLines.ToArray()}};
                }
                else if(lineContent[i].Trim().StartsWith("rollout_policy:"))
                {
                    sdFile.Preconditions = sdFile.Preconditions == null ? new Preconditions() : sdFile.Preconditions; 
                    List<string> codeLines = new List<string>();
                    i++;
                    while(i < lineContent.Length && !IsFirstLevelSavedWord(lineContent[i]))codeLines.Add(lineContent[i++]);
                    for(int j=codeLines.Count-1; j>-1;j--)
                    {
                        if(codeLines[j].Replace(" ","").Length == 0)codeLines.RemoveAt(j);
                        else break;
                    }
                    if(codeLines.Count == 0)throw new Exception(errorStart + "code is expected in the lines below 'rollout_policy' ");
                    sdFile.Preconditions.PlannerAssistancePreconditionsAssignments = new CodeAssignment[]{new CodeAssignment(){AssignmentCode = codeLines.ToArray()}};
                }
                else if(lineContent[i].Trim().StartsWith("violate_penalty:"))
                {
                    string typeFirstLine = lineContent[i];
                    int penalty;
                    if(int.TryParse(lineContent[i++].Trim().Substring("violate_penalty:".Length).Replace(" ",""), out penalty))
                    {
                        sdFile.Preconditions = sdFile.Preconditions == null ? new Preconditions() : sdFile.Preconditions;
                        sdFile.Preconditions.ViolatingPreconditionPenalty = penalty;
                    }
                    else throw new Exception(errorStart + "<int> is expected after 'violate_penalty:', you wrote'"+lineContent[i-1]+"'");
                }
                else if(lineContent[i].Trim().StartsWith("dynamic_model:"))
                {
                     
                    List<string> codeLines = new List<string>();
                    i++;
                    while(i < lineContent.Length && !IsFirstLevelSavedWord(lineContent[i]))codeLines.Add(lineContent[i++]);
                    for(int j=codeLines.Count-1; j>-1;j--)
                    {
                        if(codeLines[j].Replace(" ","").Length == 0)codeLines.RemoveAt(j);
                        else break;
                    }
                    if(codeLines.Count == 0)throw new Exception(errorStart + "code is expected in the lines below 'available_parameters_code' ");
                    CodeAssignment code = new CodeAssignment(){AssignmentCode = codeLines.ToArray()};
                    sdFile.DynamicModel = new DynamicModel(){NextStateAssignments =  new CodeAssignment[]{code}};
                }   
            }           
            string jsonString = JsonSerializer.Serialize<SdFile>(sdFile);
            jsonString=RemoveNullsElements(jsonString);
            jsonString = PrettyJson(jsonString);
            return jsonString;
        }
         public static string TranslateAM(string skillName, string fileContent)
        {
            string errorStart = "AM file of skill '"+skillName+"': ";
            AmFile amFile = new AmFile();
            amFile.GlueFramework="ROS";
             
            string[] lineContent = fileContent.Split('\n');
            int i=0;
            int prevI = -1;
            while(i<lineContent.Length)
            { 
                if(i == prevI) i++;
                if(i >= lineContent.Length-1)break;
                prevI=i; 
                if(i==0)
                {
                    if(lineContent[i].Trim().StartsWith("project:"))
                    {
                        string project = lineContent[i].Substring("project:".Length).Replace(" ","");
                        project = RemoveHiddenChar(project);
                        PlpMain main= new PlpMain{Name=skillName,Project=project, Type="Glue"};
                        amFile.PlpMain=main;   
                        i++; 
                    }
                    else throw new Exception(errorStart + "does not start with 'project: <project_name>'");
                }
                else if(lineContent[i].Trim().StartsWith("response:"))
                {
                        ModuleResponse responses = new ModuleResponse();
                        amFile.ModuleResponse = responses;
                        List<ResponseRule> responseRules = new List<ResponseRule>();
                        int prev2 = i-1;
                        while(i < lineContent.Length)
                        {
                            if(i == prev2)i++;
                            prev2=i;
                            if(i > lineContent.Length-1)break;
                            if(lineContent[i].Trim().StartsWith("response:"))
                            {
                                string response = lineContent[i++].Substring("response:".Length).Replace(" ","");
                                while(i < lineContent.Length-2 && lineContent[i].Replace(" ","").Length == 0)i++;
                                string responseCondition="";
                                if (lineContent[i].Trim().StartsWith("response_rule:"))
                                {
                                    responseCondition = lineContent[i++].Substring("response_rule:".Length);
                                }
                                else throw new Exception(errorStart + "'response_rule: ' is expected after 'response: '");
                                
                                ResponseRule res = new ResponseRule(){Response=response, ConditionCodeWithLocalVariables=responseCondition};
                                responseRules.Add(res);
                                responses.ResponseRules = responseRules.ToArray(); 
                            }
                            else if(lineContent[i].Trim().StartsWith("response_local_variable:")) 
                                responses.FromStringLocalVariable = lineContent[i++].Substring("response_local_variable:".Length).Replace(" ","");
                            else if(lineContent[i].Replace(" ","").Length == 0) i++;
                            else if(IsFirstLevelSavedWord(lineContent[i])) 
                            {
                                // if(responseRules.Count > 0)
                                // {
                                //     responses.ResponseRules = responseRules.ToArray(); 
                                // }
                                break;
                            }
                        } 
                }else if(lineContent[i].Trim().StartsWith("module_activation:"))
                    {
                        ModuleActivation a = new ModuleActivation();
                        amFile.ModuleActivation = a;
                        string activationType = lineContent[i++].Substring("module_activation: ".Length).Replace(" ", "");
                        if(activationType == "ros_service")
                        {
                            RosService ross = new RosService();
                            List<ImportCode> ic = new List<ImportCode>();
                            a.RosService = ross;
                            int prev3=i-1;
                            while(i < lineContent.Length)
                            {
                                if(i==prev3)i++;prev3=i;
                                if(i > lineContent.Length-1)break;
                                
                                if(lineContent[i].Trim().StartsWith("imports:"))
                                {
                                    
                                    ic.Add(GetAmImport(errorStart, lineContent[i]));
                                    ross.ImportCode = ic.ToArray();
                                    i++;
                                }
                                else if (lineContent[i].Trim().StartsWith("path:")) ross.ServicePath = lineContent[i++].Substring("path:".Length).Trim();
                                else if(lineContent[i].Trim().StartsWith("srv:")) ross.ServiceName = lineContent[i++].Substring("srv:".Length).Trim();
                                else if(lineContent[i].Trim().StartsWith("parameter:"))
                                {
                                    if(ross.ServiceParameters == null)ross.ServiceParameters = new List<ServiceParameter>();
                                    ServiceParameter p = new ServiceParameter();
                                    ross.ServiceParameters.Add(p);
                                    p.ServiceFieldName = lineContent[i++].Substring("parameter:".Length).Replace(" ","");
                                    while(i < lineContent.Length -2 && lineContent[i].Replace(" ","").Length==0)i++;
                                    if(lineContent[i].Trim().StartsWith("code:"))
                                    {
                                        i++;
                                        p.AssignServiceFieldCode = lineContent[i++];
                                    }
                                    else throw new Exception(errorStart + "'code: ' is expected after 'parameter: '");
                                }
                                else if(IsFirstLevelSavedWord(lineContent[i]))
                                {
                                    break;
                                }
                            }   
                        }  
                    }else if(lineContent[i].Trim().StartsWith("local_variable:"))
                        {
                            amFile.LocalVariablesInitialization = amFile.LocalVariablesInitialization == null ? 
                                new List<LocalVariableInitialization>() : amFile.LocalVariablesInitialization;
                            List<string> codeLine = new List<string>();
                            LocalVariableInitialization var = new LocalVariableInitialization();
                            amFile.LocalVariablesInitialization.Add(var);
                            List<ImportCode> ic = new List<ImportCode>();
                            string localVarName = lineContent[i++].Substring("local_variable:".Length).Replace(" ","");
                            string varType = null;
                            bool? bFromService = null;
                            string topic = null;
                            string topicType = null;
                            string from_action_parameter = null;
                            string initial_value = null;
                            int prev4 = i-1;
                            while(i < lineContent.Length)
                            {
                                if(i == prev4)i++;prev4=i;
                                if(i > lineContent.Length-1)break;
 
                                if(lineContent[i].Trim().StartsWith("imports:"))ic.Add(GetAmImport(errorStart, lineContent[i++])); 
                                else if(lineContent[i].Trim().StartsWith("type:"))varType = lineContent[i++].Substring("type:".Length).Replace(" ","");
                                else if(lineContent[i].Trim().StartsWith("initial_value:"))initial_value = lineContent[i++].Substring("initial_value:".Length).Replace(" ","");
                                else if(lineContent[i].Trim().StartsWith("topic:"))
                                {
                                    topic = lineContent[i++].Substring("topic:".Length).Replace(" ","");
                                }
                                else if(lineContent[i].Trim().StartsWith("message_type:"))topicType = lineContent[i++].Substring("message_type:".Length).Replace(" ","");
                                else if(lineContent[i].Trim().StartsWith("action_parameter:"))
                                {
                                    from_action_parameter = lineContent[i++].Substring("action_parameter:".Length).Replace(" ","");
                                    break;
                                }
                                else if(lineContent[i].Trim().StartsWith("from_ros_reservice_response:"))
                                {
                                    string fromService = lineContent[i++].Substring("from_ros_reservice_response:".Length).Replace(" ","").ToLower();
                                    if (fromService != "true" && fromService != "false")throw new Exception(errorStart +"only 'from_ros_reservice_response: true' or 'from_ros_reservice_response: false' are allowed");
                                    bFromService = fromService == "true";
                                }
                                else if(lineContent[i].Trim().StartsWith("code:"))
                                {
                                    i++;
                                    while(i < lineContent.Length && !IsFirstLevelSavedWord(lineContent[i]))
                                    {
                                        codeLine.Add(lineContent[i++]);
                                    }
                                    for(int j=codeLine.Count-1; j >= 0; j--)
                                    {
                                        if(codeLine[j].Replace(" ","").Length==0)codeLine.RemoveAt(j);
                                        else break;
                                    }
                                    break;
                                } 
                            }
                            if(from_action_parameter == null)
                            {
                                var.LocalVariableName = localVarName;
                                var.RosTopicPath = topic;
                                var.TopicMessageType = topicType;
                                var.VariableType = varType;
                                var.InitialValue = initial_value;
                                var.AssignmentCode = codeLine.ToArray();
                                var.FromROSServiceResponse = bFromService; 
                                var.ImportCode = ic.ToArray();
                            }
                            else
                            {
                                var.InputLocalVariable = localVarName;
                                var.FromGlobalVariable = from_action_parameter;
                            }   
                        } 
                } 
            
            string jsonString = JsonSerializer.Serialize<AmFile>(amFile);
            jsonString=RemoveNullsElements(jsonString);
            jsonString = PrettyJson(jsonString);
            return jsonString;
        }

        public static string  PrettyJson(string unPrettyJson)
        {
            var options = new JsonSerializerOptions(){
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                WriteIndented = true
            };

            var jsonElement = JsonSerializer.Deserialize<JsonElement>(unPrettyJson);

            string output = JsonSerializer.Serialize(jsonElement, options);
            output = output.Replace("\\t", "    ");
            return output;
        }

        private static string RemoveNullsElements(string json)
        {
            while(json.Contains("null"))
            {
                int nullInd = json.IndexOf("null");
                int removeFrom = json.LastIndexOf("\"", nullInd,nullInd);
                removeFrom = json.LastIndexOf("\"", removeFrom-1,removeFrom);
                //removeFrom = json.LastIndexOf("{", nullInd,nullInd) > removeFrom ? json.LastIndexOf("{", nullInd,nullInd) : removeFrom;
                //removeFrom = json.LastIndexOf("[", nullInd,nullInd) > removeFrom ? json.LastIndexOf("[", nullInd,nullInd) : removeFrom;

                json = json.Remove(removeFrom, nullInd+"null".Length-removeFrom);
            }
            bool removed=true;
            while(removed)
            {
                int l = json.Length;
                json = json.Replace(",,",",");
                json = json.Replace("{,","{");
                json = json.Replace("[,","[");
                json = json.Replace(",}","}");
                json = json.Replace(",]","]");
                removed = l != json.Length;
            }
            return json;
        }

        private static ImportCode GetAmImport(string errorStart, string lineContent)
        {
            string s = lineContent;
            if (s.IndexOf("from: ") == -1 || s.IndexOf("import: ") == -1) throw new Exception(errorStart + "'import: ' must contain 'from: ' and 'import: '");
            ImportCode im = new ImportCode();
            int fromStart = s.IndexOf("from: ") + "from: ".Length;
            im.From = s.Substring(fromStart, s.IndexOf("import: ")-fromStart).Replace(" ", "");
            im.Import = s.Substring(s.IndexOf("import: ") + "import: ".Length).Replace(" ", "").Split(",").ToArray();
            return im;
        }

       public static string TranslateEF(string fileName, string fileContent)
        {
            string errorStart = "EF file '"+fileName+"': ";
            EfFile efFile = new EfFile();
            List<SpecialStateCode> SpecialStateCodes = new List<SpecialStateCode>();
            List<GlobalVariableType> types = new List<GlobalVariableType>();
            List<GlobalVariableDeclaration> state_vars = new List<GlobalVariableDeclaration>(); 
            string[] lineContent = fileContent.Split('\n');
            int i=0;
            int prevI = -1;
            while(i<lineContent.Length)
            { 
                if(i == prevI) i++;
                if(i >= lineContent.Length)break;
                prevI=i;
                if(i==0)
                {
                    if(lineContent[i].Trim().StartsWith("project:"))
                    {
                        string project = lineContent[i].Substring("project:".Length).Replace(" ","");
                        project = RemoveHiddenChar(project);
                        PlpMain main= new PlpMain{Name="environment",Project=project, Type="Environment"};
                        efFile.PlpMain=main;  
                        i++; 
                    }
                    else throw new Exception(errorStart + "does not start with 'project: <project_name>'");
                }
                else if(lineContent[i].Trim().StartsWith("horizon:"))
                {
                    int t;
                     if(Int32.TryParse(lineContent[i++].Substring("horizon:".Length).Replace(" ",""), out t))
                     {
                        efFile.EnvironmentGeneral = efFile.EnvironmentGeneral == null ? new EnvironmentGeneral() : efFile.EnvironmentGeneral;
                        efFile.EnvironmentGeneral.Horizon = t;
                     }
                     else throw new Exception(errorStart + "horizon must by an int 'horizon: <int_value>') ");
                }
                else if(lineContent[i].Trim().StartsWith("discount:"))
                {
                    float t;
                     if(float.TryParse(lineContent[i++].Substring("discount:".Length).Replace(" ",""), out t))
                     {
                        efFile.EnvironmentGeneral = efFile.EnvironmentGeneral == null ? new EnvironmentGeneral() : efFile.EnvironmentGeneral;
                        efFile.EnvironmentGeneral.Discount = t;
                     }
                     else throw new Exception(errorStart + "discount must by an int 'discount: <float_value>') ");
                }
                else if(lineContent[i].Trim().StartsWith("define_type:"))
                {
                    string typeFirstLine = lineContent[i];
                    string typeName = lineContent[i++].Substring("define_type:".Length).Replace(" ","");
                    List<string> enumMembers = new List<string>();
                    List<GlobalCompoundTypeVariable> typeVariables = new List<GlobalCompoundTypeVariable>();
                    while(!lineContent[i].Contains(" ") || !IsFirstLevelSavedWord(lineContent[i]))
                    {
                        while(i < lineContent.Length && lineContent[i].Replace(" ","").Length==0)i++;
                        if(lineContent[i].Trim().StartsWith("enum_members:"))enumMembers = lineContent[i++].Substring("enum_members:".Length).Replace(" ","").Split(",").ToList();
                        else if(lineContent[i].Trim().StartsWith("variable:"))
                        {
                            List<string> bits = lineContent[i++].Substring("variable:".Length).Split(" ").ToList();
                            bits = bits.Select(x=> x.Replace(" ","")).Where(x=> x.Length > 0).ToList();
                            if(bits.Count > 4 || bits.Count < 2)throw new Exception(errorStart + "'variable:' definition is 'variable: <type> <name> <optional_default_value> <optional_ML_ignore_variable or optional_max_possible_value_for_ML>' the definition sent was '"+lineContent[i-1]+"'");
                            GlobalCompoundTypeVariable tt = new GlobalCompoundTypeVariable();
                            tt.Name = bits[1];
                            tt.Type = bits[0];
                            tt.Default = bits.Count > 2 ? bits[2] : null;
                            float temp; 
                            if(bits.Count > 3)
                            {
                                if(float.TryParse(bits[3], out temp))
                                {
                                    tt.ML_MaxPossibleValue = temp;
                                }
                                else
                                {
                                    if (bits[3].ToLower() == "ml_ignore")
                                    {
                                        tt.ML_IgnoreVariable = true;
                                    }
                                    else
                                    {
                                        throw new Exception(errorStart + "'variable:' definition is 'variable: <type> <name> <optional_default_value> <optional_ML_ignore_variable or optional_max_possible_value_for_ML>', <optional_ML_ignore_variable or optional_max_possible_value_for_ML> must be a decimal number or 'ml_ignore'!, the definition sent was '"+lineContent[i-1]+"'");
                                    }
                                }
                            }
                            typeVariables.Add(tt);
                        }else break;
                    }
                    if(enumMembers.Count == 0 && typeVariables.Count ==0)throw new Exception(errorStart + "after '"+typeFirstLine+"' you must define 'variable: ...' or 'enum_members: ...'");
                    if(enumMembers.Count > 0 && typeVariables.Count >0)throw new Exception(errorStart + "you cannot define both 'variable: ...' and 'enum_members: ...' for the same type. see '"+typeFirstLine+"'");
                    GlobalVariableType t = new GlobalVariableType();
                    types.Add(t);
                    efFile.GlobalVariableTypes = types.ToArray(); 
                    t.TypeName = typeName;
                    if(enumMembers.Count > 0)
                    {   
                        t.Type = "enum";
                        t.EnumValues = enumMembers.ToArray();
                    }
                    else
                    {
                        t.Type = "compound";
                        t.Variables = typeVariables.ToArray();
                    }
                }else if(lineContent[i].Trim().StartsWith("state_variable:") || lineContent[i].Trim().StartsWith("action_parameter:"))
                {
                    bool isActionParam = lineContent[i].Trim().StartsWith("action_parameter:");
                    GlobalVariableDeclaration var = new GlobalVariableDeclaration();
                    List<string> codeLines = new List<string>();
                    state_vars.Add(var);
                    efFile.GlobalVariablesDeclaration = state_vars.ToArray();
                    string[] delimiters = {" ",":"};
                    List<string> bits = lineContent[i].Split(delimiters,StringSplitOptions.None).ToList();
                    bits = bits.Select(x=>x.Replace(" ","")).Where(x=> x.Length > 0 && x != "action_parameter" && x != "state_variable").ToList();
                    if(bits.Count != 2 && bits.Count != 3)throw new Exception(errorStart + "a '<type> <name> [<is_array>]' must be defined aftter 'action_parameter:' or 'state_variable:' you wrote '"+lineContent[i]+"'");
                    var.Type = bits[0];
                    var.Name = bits[1];
                    var.IsArray = bits.Count == 3 && (bits[2] == "[]" || bits[2].ToLower()=="true"); 
                    i++;
                    // var.Name = isActionParam ? lineContent[i++].Substring("action_parameter:".Length).Replace(" ","") :
                    //     lineContent[i++].Substring("state_variable:".Length).Replace(" ",""); 
                    
                    while(!IsFirstLevelSavedWord(lineContent[i]) && i < lineContent.Length)
                    {   
                        int startI = i;
                        while(i < lineContent.Length && lineContent[i].Replace(" ","").Length == 0)i++;
                        if(lineContent[i].Replace(" ","")=="code:")
                        {
                            i++;
                            while(i < lineContent.Length && !IsFirstLevelSavedWord(lineContent[i])) codeLines.Add(lineContent[i++]);
                            codeLines = codeLines.Where(x=> x.Replace(" ","").Length > 0).ToList();
                        }
                        if(lineContent[i].Replace(" ","").Trim().StartsWith("ml_max_value:"))
                        {
                            string max_v = lineContent[i].Replace("ml_max_value:","").Replace(" ","");
                            float t;
                            if(float.TryParse(max_v, out t))
                            {
                                var.ML_MaxPossibleValue = t;
                            }
                            else
                            {
                                throw new Exception(errorStart + " 'ml_max_value:' must be in the form 'ml_max_value: <decimal number>', you wrote:"+lineContent[i]+"'");
                            }
                            i++; 
                        }
                        if(lineContent[i].Replace(" ","").Trim().StartsWith("ml_ignore:"))
                        {
                            string b_s = lineContent[i].Replace("ml_ignore:","").Replace(" ","");
                            var.ML_IgnoreVariable = b_s.ToLower().Equals("true");
                            i++; 
                        }
                        i = i == startI ? i+1 : i;

                    } 
                    var.DefaultCode = codeLines.Count > 0 ? codeLines[0] : "";
                    var.IsActionParameterValue = isActionParam;
                }
                else if(lineContent[i].Trim().StartsWith("initial_belief:"))
                {
                    i++;
                    List<string> codeLines = new List<string>();
                    while(i < lineContent.Length && !IsFirstLevelSavedWord(lineContent[i])) codeLines.Add(lineContent[i++]);
                    CodeAssignment code = new CodeAssignment(){AssignmentCode=codeLines.ToArray()};
                    efFile.InitialBeliefStateAssignments = new CodeAssignment[]{code};
                }
                // else if(lineContent[i].Trim().StartsWith("initial_belief:"))
                // {
                //     i++;
                //     List<string> codeLines = new List<string>();
                //     while(i < lineContent[i].Length && !IsFirstLevelSavedWord(lineContent[i])) codeLines.Add(lineContent[i++]);
                //     CodeAssignment code = new CodeAssignment();
                //     code.AssignmentCode = codeLines.ToArray();
                //     efFile.InitialBeliefStateAssignments = new CodeAssignment[]{code};
                // }
                else if(lineContent[i].Trim().StartsWith("reward_code:"))
                {
                    i++;
                    List<string> codeLines = new List<string>();
                    while(i < lineContent.Length && !IsFirstLevelSavedWord(lineContent[i])) codeLines.Add(lineContent[i++]);
                    CodeAssignment code = new CodeAssignment(){AssignmentCode=codeLines.ToArray()}; 
                    SpecialStateCode sCode = new SpecialStateCode(){StateFunctionCode = new CodeAssignment[]{code}};
                    SpecialStateCodes.Add(sCode);
                    efFile.SpecialStates = SpecialStateCodes.ToArray();
                }

                else if(lineContent[i].Trim().StartsWith("extrinsic_code:"))
                {
                    i++;
                    List<string> codeLines = new List<string>();
                    while(i < lineContent.Length && !IsFirstLevelSavedWord(lineContent[i])) codeLines.Add(lineContent[i++]);
                    CodeAssignment code = new CodeAssignment(){AssignmentCode = codeLines.ToArray()};
                    efFile.ExtrinsicChangesDynamicModel = new CodeAssignment[]{code};
                }
                
            }           
            string jsonString = JsonSerializer.Serialize<EfFile>(efFile);
            jsonString=RemoveNullsElements(jsonString);
            jsonString = PrettyJson(jsonString);
            return jsonString;
        }
        private static bool IsFirstLevelSavedWord(string word)
        {
            word=word.Trim();
            if(!word.Contains(":"))return false;
            return FirstLevelSavedWords.Where(x=> x == word.Substring(0, word.IndexOf(":")+1)).Count() > 0;
        }
    }
}