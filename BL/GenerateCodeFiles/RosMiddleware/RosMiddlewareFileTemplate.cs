using System.Reflection.Metadata.Ecma335;
using System.Security.Principal;
using MongoDB.Bson;
using System;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using WebApiCSharp.Services;
using WebApiCSharp.Models;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
namespace WebApiCSharp.GenerateCodeFiles
{
    public class RosMiddlewareFileTemplate
    {
        private static string GetPackageFileTargetProjectDependencies(InitializeProject initProj)
        {
            string result = "";
            foreach (string targetPackage in initProj.RosTarget.RosTargetProjectPackages)
            {
                result += GenerateFilesUtils.GetIndentationStr(0, 0, "<build_depend>" + targetPackage + "</build_depend>");
                result += GenerateFilesUtils.GetIndentationStr(0, 0, "<exec_depend>" + targetPackage + "</exec_depend>");
            }
            return result;
        }
        public static string GetPackageFile(InitializeProject initProj)
        {
            string file = @"<?xml version=""1.0""?>
<package format=""2"">
  <name>" + GenerateRosMiddleware.ROS_MIDDLEWARE_PACKAGE_NAME + @"</name>
  <version>0.0.0</version>
  <description>The " + GenerateRosMiddleware.ROS_MIDDLEWARE_PACKAGE_NAME + @" package</description>

  <maintainer email=""or@todo.todo"">or</maintainer>


  <license>TODO</license>



  <buildtool_depend>catkin</buildtool_depend>
" + GetPackageFileTargetProjectDependencies(initProj) + @"
 
  <build_depend>roscpp</build_depend>
  <build_depend>rospy</build_depend>
  <build_depend>std_msgs</build_depend>
<build_depend>message_generation</build_depend>
  <build_export_depend>roscpp</build_export_depend>
  <build_export_depend>rospy</build_export_depend>
  <build_export_depend>std_msgs</build_export_depend> 
  <exec_depend>roscpp</exec_depend> 
  <exec_depend>rospy</exec_depend>
  <exec_depend>std_msgs</exec_depend> 
<exec_depend>message_runtime</exec_depend>


  <export>

  </export>
</package>";
            return file;
        }

        public static string GetCMakeListsFile()
        {
            string file = @"cmake_minimum_required(VERSION 3.0.2)
project(" + GenerateRosMiddleware.ROS_MIDDLEWARE_PACKAGE_NAME + @")
 
find_package(catkin REQUIRED COMPONENTS
  roscpp
  rospy
  std_msgs
  message_generation
  genmsg 
)


  

 
 generate_messages(
   DEPENDENCIES
   std_msgs 
 )

 
catkin_package(
CATKIN_DEPENDS message_runtime
)

include_directories( 
# include
  ${catkin_INCLUDE_DIRS}
)
 ";
            return file;
        }


        private static string GetImportsForMiddlewareNode(PLPsData data, InitializeProject initProj)
        {
            string result = "";
            Dictionary<string, HashSet<string>> unImports = new Dictionary<string, HashSet<string>>();
            List<RosImport> imports = new List<RosImport>();

            foreach (RosGlue glue in data.RosGlues.Values)
            {
                imports.AddRange(glue.RosServiceActivation.Imports);

                foreach (var lVar in glue.GlueLocalVariablesInitializations)
                {
                    imports.AddRange(lVar.Imports);
                }
            }

            foreach (RosImport im in imports)
            {
                im.From = im.From == null ? "" : im.From;
                if (!unImports.ContainsKey(im.From))
                {
                    unImports.Add(im.From, new HashSet<string>());
                }
                foreach (string sIm in im.Imports)
                {
                    unImports[im.From].Add(sIm);
                }
            }

            foreach (KeyValuePair<string, HashSet<string>> keyVal in unImports)
            {
                string baseS = keyVal.Key.Replace(" ", "").Length == 0 ? "" : "from " + keyVal.Key + " ";
                result += GenerateFilesUtils.GetIndentationStr(0, 0, baseS + "import " + String.Join(",", keyVal.Value));
            }
            return result;
        }

        private static string GetLocalVariableTypeClasses(PLPsData data)
        {
            string result = "";
            foreach (LocalVariableTypePLP type in data.LocalVariableTypes)
            {
                result += GenerateFilesUtils.GetIndentationStr(0, 4, "def " + type.TypeName + "ToDict(lt):");

                List<string> saFields = type.SubFields.Select(x => "\"" + x.FieldName + "\": lt." + x.FieldName).ToList();

                result += GenerateFilesUtils.GetIndentationStr(1, 4, "return {" + String.Join(", ", saFields) + "}");

                result += Environment.NewLine;

                result += GenerateFilesUtils.GetIndentationStr(0, 4, "class " + type.TypeName + ":");
                result += GenerateFilesUtils.GetIndentationStr(1, 4, "def __init__(self, " + String.Join(", ", type.SubFields.Select(x => x.FieldName)) + "):");
                foreach (LocalVariableCompoundTypeField field in type.SubFields)
                {
                    result += GenerateFilesUtils.GetIndentationStr(2, 4, "self." + field.FieldName + "=" + field.FieldName);
                }

                result += Environment.NewLine;

                result += GenerateFilesUtils.GetIndentationStr(1, 4, "def __init__(self):");
                foreach (LocalVariableCompoundTypeField field in type.SubFields)
                {
                    result += GenerateFilesUtils.GetIndentationStr(2, 4, "self." + field.FieldName + "=None");
                }
            }
            return result;
        }

        private static string GetListenToMongoDbCommandsInitFunction(PLPsData data)
        {
            string result = "";

            result += GenerateFilesUtils.GetIndentationStr(1, 4, "def __init__(self, _topicListener):");
            result += GenerateFilesUtils.GetIndentationStr(2, 4, "self.currentActionSequenceID = 1");
            result += GenerateFilesUtils.GetIndentationStr(2, 4, "self.currentActionFotExecutionId = None");
            result += GenerateFilesUtils.GetIndentationStr(2, 4, "self._topicListener = _topicListener");
            result += GenerateFilesUtils.GetIndentationStr(2, 4, "self.readyToActivate = \"\" ");

            foreach (RosGlue glue in data.RosGlues.Values)
            {
                if (glue.RosServiceActivation != null && !string.IsNullOrEmpty(glue.RosServiceActivation.ServiceName))
                {
                    result += GenerateFilesUtils.GetIndentationStr(2, 4, "self." + glue.Name + "ServiceName = \"" + glue.RosServiceActivation.ServicePath + "\"");
                }
            }

            result += Environment.NewLine;

            result += GenerateFilesUtils.GetIndentationStr(2, 4, "self.listen_to_mongodb_commands()");
            return result;
        }

        private static CompoundVarTypePLP GetCompundTypeByName(string compTypeName, PLPsData data)
        {
            List<CompoundVarTypePLP> cl = data.GlobalCompoundTypes.Where(x => x.TypeName.Equals(compTypeName)).ToList();
            return cl.Count == 0 ? null : cl[0];
        }
        private static CompoundVarTypePLP_Variable GetCompundVariableByName(CompoundVarTypePLP oComp, string subFields, PLPsData data)
        {
            string[] bits = subFields.Split(".");
            if (oComp == null || bits.Length == 0) return null;
            List<CompoundVarTypePLP_Variable> lv = oComp.Variables.Where(x => x.Name.Equals(bits[0])).ToList();
            if (lv.Count == 0) return null;
            if (bits.Length == 1) return lv[0];
            bits[0] = "";
            return GetCompundVariableByName(GetCompundTypeByName(lv[0].Type, data), String.Join(".", bits.Where(x => !String.IsNullOrEmpty(x))), data);
        }

        private static LocalVariableTypePLP GetUnderlineLocalVariableTypeByVarName(PLPsData data, PLP plp, string variableName)
        {
            string underlineTypeName = GetUnderlineLocalVariableNameTypeByVarName(data, plp, variableName);
            underlineTypeName = underlineTypeName == null ? "" : underlineTypeName;
            return data.LocalVariableTypes.Where(x => x.TypeName.Equals(underlineTypeName)).FirstOrDefault();

        }


        private static string GetUnderlineLocalVariableNameTypeByVarName(PLPsData data, PLP plp, string variableName)
        {

            string[] bits = variableName.Split(".");

            string baseVarName = bits[0] + "." + (bits.Length > 1 ? bits[1] : "");
            List<GlobalVariableDeclaration> dl = data.GlobalVariableDeclarations.Where(x => ("state." + x.Name).Equals(baseVarName)).ToList();

            if (dl.Count > 0)
            {
                if (GenerateFilesUtils.IsPrimitiveType(dl[0].Type))
                {
                    return null;
                }
                if (PLPsData.ANY_VALUE_TYPE_NAME.Equals(dl[0].Type))
                {
                    return dl[0].UnderlineLocalVariableType;
                }
                CompoundVarTypePLP comp = GetCompundTypeByName(dl[0].Type, data);
                CompoundVarTypePLP_Variable oVar = GetCompundVariableByName(comp, string.Join(".", bits.Skip(1)), data);
                return oVar == null || !oVar.Type.Equals(PLPsData.ANY_VALUE_TYPE_NAME) ? null : oVar.UnderlineLocalVariableType;
            }
            else//is a parameter
            {
                List<GlobalVariableModuleParameter> temp = plp.GlobalVariableModuleParameters.Where(x => x.Name.Equals(bits[0])).ToList();
                if (temp.Count == 0) return null;
                CompoundVarTypePLP comp = GetCompundTypeByName(temp[0].Type, data);
                CompoundVarTypePLP_Variable oVar = GetCompundVariableByName(comp, string.Join(".", bits.Skip(1)), data);
                return oVar == null || !oVar.Type.Equals(PLPsData.ANY_VALUE_TYPE_NAME) ? null : oVar.UnderlineLocalVariableType;
            }
        }
        private static string GetHandleModuleFunction(PLPsData data)
        {
            string result = "";

            foreach (RosGlue glue in data.RosGlues.Values)
            {
                PLP plp = data.PLPs[glue.Name];
                result += GenerateFilesUtils.GetIndentationStr(1, 4, "def handle_" + glue.Name + "(self, params):");
                result += GenerateFilesUtils.GetIndentationStr(2, 4, "responseNotByLocalVariables = None");
                Dictionary<string, LocalVariablesInitializationFromGlobalVariable> localVarsFromGlobal = new Dictionary<string, LocalVariablesInitializationFromGlobalVariable>();

                bool hasVar = false;

                foreach (LocalVariablesInitializationFromGlobalVariable oGlVar in glue.LocalVariablesInitializationFromGlobalVariables)
                {
                    hasVar = true;
                    result += GenerateFilesUtils.GetIndentationStr(2, 4, oGlVar.InputLocalVariable + " = \"\"");
                }
                if (hasVar)
                {
                    result += GenerateFilesUtils.GetIndentationStr(2, 4, "try:");

                    foreach (LocalVariablesInitializationFromGlobalVariable oGlVar in glue.LocalVariablesInitializationFromGlobalVariables)
                    {
                        LocalVariableTypePLP underlineType = GetUnderlineLocalVariableTypeByVarName(data, plp, oGlVar.FromGlobalVariable);

                        if (underlineType != null)
                        {
                            if (oGlVar.FromGlobalVariable.StartsWith(PLPsData.GLOBAL_VARIABLE_STATE_REF))
                            {
                                result += GenerateFilesUtils.GetIndentationStr(3, 4, "globVarName = \"" + oGlVar.FromGlobalVariable + "\"");
                            }
                            else
                            {
                                //globVarName = "oDesiredLocation.actual_location".replace("oDesiredLocation", params["ParameterLinks"]["oDesiredLocation"])
                                string baseGlobalParameter = plp.GlobalVariableModuleParameters
                                    .Where(x => oGlVar.FromGlobalVariable.StartsWith(x.Name + ".") || oGlVar.FromGlobalVariable.Equals(x.Name))
                                    .Select(x => x.Name).FirstOrDefault();
                                result += GenerateFilesUtils.GetIndentationStr(3, 4, "globVarName = \"" + oGlVar.FromGlobalVariable + "\".replace(\"" + baseGlobalParameter + "\", params[\"ParameterLinks\"][\"" + baseGlobalParameter + "\"], 1)");
                            }

                            result += GenerateFilesUtils.GetIndentationStr(3, 4, "dbVar = aos_GlobalVariablesAssignments_collection.find_one({\"GlobalVariableName\": globVarName})");
                            result += GenerateFilesUtils.GetIndentationStr(3, 4, oGlVar.InputLocalVariable + " = " + underlineType.TypeName + "()");
                            foreach (LocalVariableCompoundTypeField field in underlineType.SubFields)
                            {
                                //obj_location.z=cupAccurateLocation[""LowLevelValue""][""z""]
                                result += GenerateFilesUtils.GetIndentationStr(3, 4, oGlVar.InputLocalVariable + "." + field.FieldName + " = dbVar[\"LowLevelValue\"][\"" + field.FieldName + "\"]");
                            }
                            result += GenerateFilesUtils.GetIndentationStr(3, 4, "self._topicListener.localVarNamesAndValues[\"" + glue.Name + "\"][\"" + oGlVar.InputLocalVariable + "\"] = " + underlineType.TypeName + "ToDict(" + oGlVar.InputLocalVariable + ")");
                        }
                        else
                        {
                            string[] bits = oGlVar.FromGlobalVariable.Split(".");
                            string varDesc = "[\"" + String.Join("\"][\"", bits) + "\"]";
                            result += GenerateFilesUtils.GetIndentationStr(3, 4, oGlVar.InputLocalVariable + " = params[\"ParameterValues\"]" + varDesc);

                            result += GenerateFilesUtils.GetIndentationStr(3, 4, "self._topicListener.updateLocalVariableValue(\""+oGlVar.InputLocalVariable+"\", "+oGlVar.InputLocalVariable+")");
                            //self._topicListener.updateLocalVariableValue("nav_to_y", nav_to_y)
                        }
                        //  result += GenerateFilesUtils.GetIndentationStr(3, 4, "self._topicListener.localVarNamesAndValues[\"" + glue.Name + "\"][\"" + oGlVar.InputLocalVariable + "\"] = " + underlineType.TypeName + "ToDict(" + oGlVar.InputLocalVariable + ")");
                        //self._topicListener.localVarNamesAndValues["navigate"]["desired_location"]
                    }
                    result += GenerateFilesUtils.GetIndentationStr(2, 4, "except Exception as e:");
                    result += GenerateFilesUtils.GetIndentationStr(3, 4, "registerError(str(e), traceback.format_exc(e), 'Action: "+glue.Name+", illegalActionObs')");
                    result += GenerateFilesUtils.GetIndentationStr(3, 4, "responseNotByLocalVariables = \"illegalActionObs\"");
                }

                if (glue.RosServiceActivation != null && !string.IsNullOrEmpty(glue.RosServiceActivation.ServiceName))
                {
                    result += GenerateFilesUtils.GetIndentationStr(2, 4, "try:");
                    result += GenerateFilesUtils.GetIndentationStr(3, 4, "registerLog(\"wait for service: moduleName:"+glue.Name+", serviceName:"+glue.RosServiceActivation.ServiceName+"\")");
                    result += GenerateFilesUtils.GetIndentationStr(3, 4, "rospy.wait_for_service(self." + glue.Name + "ServiceName)");
                    result += GenerateFilesUtils.GetIndentationStr(3, 4, glue.Name + "_proxy = rospy.ServiceProxy(self." + glue.Name + "ServiceName, " + glue.RosServiceActivation.ServiceName + ")");

                    string serviceCallParam = string.Join(", ", glue.RosServiceActivation.ParametersAssignments.Select(x => x.MsgFieldName + "=(" + x.AssignServiceFieldCode + ")"));

                    result += GenerateFilesUtils.GetIndentationStr(3, 4, "registerLog(\"Sending request to service, moduleName:"+ glue.Name+"\")");
                    result += GenerateFilesUtils.GetIndentationStr(3, 4, "__input = " + glue.Name + "_proxy(" + serviceCallParam + ")");
                    GlueLocalVariablesInitialization localVarFromServiceReponse = glue.GlueLocalVariablesInitializations.Where(x => x.FromROSServiceResponse.HasValue && x.FromROSServiceResponse.Value).FirstOrDefault();
                    result += GenerateFilesUtils.GetIndentationStr(3, 4, "registerLog(\"Service response received, moduleName:"+ glue.Name+"\")");
                    if (localVarFromServiceReponse != null)
                    {
                        result += GenerateFilesUtils.GetIndentationStr(3, 4, localVarFromServiceReponse.AssignmentCode, true, true);
                        result += GenerateFilesUtils.GetIndentationStr(3, 4, "self._topicListener.updateLocalVariableValue(\"" + localVarFromServiceReponse.LocalVarName + "\"," + localVarFromServiceReponse.LocalVarName + ")");
                    }


                    result += GenerateFilesUtils.GetIndentationStr(3, 4, "if DEBUG:");
                    result += GenerateFilesUtils.GetIndentationStr(4, 4, "print(\"" + glue.Name + " service terminated\")");
                    result += GenerateFilesUtils.GetIndentationStr(2, 4, "except Exception as e:");
                    result += GenerateFilesUtils.GetIndentationStr(3, 4, "registerError(str(e),traceback.format_exc(e),'Action: "+glue.Name+"')");
                    result += GenerateFilesUtils.GetIndentationStr(3, 4, "print(\"Service call failed\")");
                    result += GenerateFilesUtils.GetIndentationStr(2, 4, "");
                }


                result += GenerateFilesUtils.GetIndentationStr(2, 4, "return responseNotByLocalVariables");

            }
            return result;
        }

        private static string GetListenToMongoCommandsFunctionPart(PLPsData data)
        {
            string result = "";
            foreach (PLP plp in data.PLPs.Values)
            {
                result += GenerateFilesUtils.GetIndentationStr(4, 4, "if moduleName == \"" + plp.Name + "\":");
                result += GenerateFilesUtils.GetIndentationStr(5, 4, "print(\"handle " + plp.Name + "\")");
                result += GenerateFilesUtils.GetIndentationStr(5, 4, "responseNotByLocalVariables = self.handle_" + plp.Name + "(actionParameters)");
            }
            return result;
        }
        private static string GetModuleResponseFunctionPart(PLPsData data)
        {
            string result = "";
            foreach (RosGlue glue in data.RosGlues.Values)
            {
                PLP plp = data.PLPs[glue.Name];
                result += GenerateFilesUtils.GetIndentationStr(2, 4, "if moduleName == \"" + glue.Name + "\":");
                HashSet<string> localVarNames = new HashSet<string>();
                foreach (var oVar in glue.GlueLocalVariablesInitializations)
                {
                    localVarNames.Add(oVar.LocalVarName);
                    result += GenerateFilesUtils.GetIndentationStr(3, 4, oVar.LocalVarName + " = self._topicListener.localVarNamesAndValues[\"" + glue.Name + "\"][\"" + oVar.LocalVarName + "\"]");
                }
                foreach (var oVar in glue.LocalVariablesInitializationFromGlobalVariables)
                {
                    localVarNames.Add(oVar.InputLocalVariable);
                    result += GenerateFilesUtils.GetIndentationStr(3, 4, oVar.InputLocalVariable + " = self._topicListener.localVarNamesAndValues[\"" + glue.Name + "\"][\"" + oVar.InputLocalVariable + "\"]");
                }

                result += GenerateFilesUtils.GetIndentationStr(3, 4, "if DEBUG:");
                result += GenerateFilesUtils.GetIndentationStr(4, 4, "print(\"" + glue.Name + " action local variables:\")");
                foreach (var oVar in glue.GlueLocalVariablesInitializations)
                {
                    if(!oVar.IsHeavyVariable)
                    {
                        result += GenerateFilesUtils.GetIndentationStr(4, 4, "print(\"" + oVar.LocalVarName + ":\")");
                        result += GenerateFilesUtils.GetIndentationStr(4, 4, "print(" + oVar.LocalVarName + ")");
                    }
                }

                if(glue.ResponseRules.Count == 0)
                {
                    result += GenerateFilesUtils.GetIndentationStr(3, 4, "if moduleResponse == \"\":");
                    result += GenerateFilesUtils.GetIndentationStr(4, 4, "moduleResponse = \"DefaultObservation\"");
                }
                foreach (var responseRule in glue.ResponseRules)
                {
                    result += GenerateFilesUtils.GetIndentationStr(3, 4, "if moduleResponse == \"\" and " + (string.IsNullOrEmpty(responseRule.Condition) ? "True" : responseRule.Condition) + ":");
                    result += GenerateFilesUtils.GetIndentationStr(4, 4, "moduleResponse = \"" + glue.Name + "_" + responseRule.Response + "\"");
                }

                foreach (var responseRule in glue.ResponseRules)
                {
                    foreach (var assign in responseRule.ResponseAssignmentsToGlobalVar)
                    {
                        result += GenerateFilesUtils.GetIndentationStr(3, 4, "if moduleResponse == \"" + glue.Name + "_" + responseRule.Response + "\":");
                        result += GenerateFilesUtils.GetIndentationStr(4, 4, "assignGlobalVar[\"" + assign.GlobalVarName + "\"] = " + assign.Value);
                    }
                }

                if(!String.IsNullOrEmpty(glue.ResponseFromStringLocalVariable))
                {
                    result += GenerateFilesUtils.GetIndentationStr(3, 4, "moduleResponse = str(" + glue.ResponseFromStringLocalVariable + ")");
                }
                result += Environment.NewLine;
            }
            return result;
        }

        private static string GetCodeLineWithLocalVarRefference(string codeLine, HashSet<string> localVarNames)
        {
            string result = codeLine;
            foreach (string varName in localVarNames.OrderByDescending(x => x.Length))
            {
                string pattern = @"\b(?<!\.)" + varName + @"\b";
                string replaceTo = "self.localVarNamesAndValues[self.listenTargetModule][\"" + varName + "\"]";
                result = Regex.Replace(result, pattern, replaceTo);
            }
            return result;
        }
        private static string GetAOS_TopicListenerServerClass(PLPsData data)
        {
            string result = "";
            result += GenerateFilesUtils.GetIndentationStr(0, 4, "class AOS_TopicListenerServer:");
            result += GenerateFilesUtils.GetIndentationStr(1, 4, "def __init__(self):");
            result += GenerateFilesUtils.GetIndentationStr(2, 4, "self.localVarNamesAndValues = {", false);

            List<RosGlue> gluesWithLocalVars = data.RosGlues.Values.Where(x => x.LocalVariablesInitializationFromGlobalVariables.Count > 0 || x.GlueLocalVariablesInitializations.Count > 0).ToList();

            HashSet<string> localVarNames = new HashSet<string>();
            for (int j = 0; gluesWithLocalVars.Count > j; j++)
            {
                RosGlue glue = gluesWithLocalVars[j];

                result += GenerateFilesUtils.GetIndentationStr(0, 4, "\"" + glue.Name + "\":{", false);

                for (int i = 0; glue.GlueLocalVariablesInitializations.Count > i; i++)
                {
                    var localVar = glue.GlueLocalVariablesInitializations[i];
                    localVarNames.Add(localVar.LocalVarName);
                    result += GenerateFilesUtils.GetIndentationStr(0, 4, "\"" + localVar.LocalVarName + "\": " +
                            (string.IsNullOrEmpty(localVar.InitialValue) ? "None" : localVar.InitialValue) +
                            (i == glue.GlueLocalVariablesInitializations.Count - 1 && glue.LocalVariablesInitializationFromGlobalVariables.Count == 0 ? "" : ", "), false);
                }
                for (int i = 0; i < glue.LocalVariablesInitializationFromGlobalVariables.Count; i++)
                {
                    var localFromGlob = glue.LocalVariablesInitializationFromGlobalVariables[i];
                    localVarNames.Add(localFromGlob.InputLocalVariable);
                    result += GenerateFilesUtils.GetIndentationStr(0, 4, "\"" + localFromGlob.InputLocalVariable + "\": None" +
                            (i == glue.LocalVariablesInitializationFromGlobalVariables.Count - 1 ? "" : ", "), false);
                }
                result += GenerateFilesUtils.GetIndentationStr(0, 4, "}" + (j < gluesWithLocalVars.Count - 1 ? ", " : ""), false);
            }
            result += GenerateFilesUtils.GetIndentationStr(0, 4, "}");
            result += GenerateFilesUtils.GetIndentationStr(2, 4, "self.setListenTarget(\"initTopicListener\")");


            Dictionary<string, Dictionary<string, List<GlueLocalVariablesInitialization>>> topicsToListen = new Dictionary<string, Dictionary<string, List<GlueLocalVariablesInitialization>>>();
            foreach (RosGlue glue in data.RosGlues.Values)
            {
                foreach (var oLVar in glue.GlueLocalVariablesInitializations)
                {
                    if (!string.IsNullOrEmpty(oLVar.RosTopicPath))
                    {
                        string cbFunc = "cb_" + oLVar.RosTopicPath.Replace("/", "_");
                        if (!topicsToListen.ContainsKey(oLVar.RosTopicPath))
                        {
                            topicsToListen[oLVar.RosTopicPath] = new Dictionary<string, List<GlueLocalVariablesInitialization>>();
                        }
                        if (!topicsToListen[oLVar.RosTopicPath].ContainsKey(glue.Name))
                        {
                            topicsToListen[oLVar.RosTopicPath][glue.Name] = new List<GlueLocalVariablesInitialization>();
                        }
                        topicsToListen[oLVar.RosTopicPath][glue.Name].Add(oLVar);
                    }
                }
            }

            foreach (var topic in topicsToListen)
            {
                result += GenerateFilesUtils.GetIndentationStr(2, 4, "rospy.Subscriber(\"" + topic.Key + "\", " + topic.Value.Values.ToList()[0][0].TopicMessageType + ", self.cb_" + topic.Key.Replace("/", "_") + ", queue_size=1000)");

            }
            result += Environment.NewLine;

            foreach (var topic in topicsToListen)
            {
                result += GenerateFilesUtils.GetIndentationStr(1, 4, "def cb_" + topic.Key.Replace("/", "_") + "(self, data):");
                result += GenerateFilesUtils.GetIndentationStr(2, 4, "try:");
                foreach (var glueTopic in topic.Value)
                {
                    result += GenerateFilesUtils.GetIndentationStr(3, 4, "if self.listenTargetModule == \"" + glueTopic.Key + "\":");
                    result += GenerateFilesUtils.GetIndentationStr(4, 4, "if DEBUG:");
                    result += GenerateFilesUtils.GetIndentationStr(5, 4, "print(\"handling topic call:" + glueTopic.Key + "\")");
                    result += GenerateFilesUtils.GetIndentationStr(5, 4, "print(data)");
                    foreach (var localVar in glueTopic.Value)
                    {
                        result += GenerateFilesUtils.GetIndentationStr(4, 4, "#-----------------------------------------------------------------------");
                        result += GenerateFilesUtils.GetIndentationStr(4, 4, "value = self." + glueTopic.Key + "_get_value_" + localVar.LocalVarName + "(data)");
                        result += GenerateFilesUtils.GetIndentationStr(4, 4, "self.updateLocalVariableValue(\"" + localVar.LocalVarName + "\", value)");
                    }
                }
                result += GenerateFilesUtils.GetIndentationStr(2, 4, "except Exception as e:");
                result += GenerateFilesUtils.GetIndentationStr(3, 4, "registerError(str(e), traceback.format_exc(e), 'topic "+topic.Key+"')");


                result += Environment.NewLine;
            }



            foreach (var topic in topicsToListen)
            {
                foreach (var glueTopic in topic.Value)
                {
                    foreach (var localVar in glueTopic.Value)
                    {
                        result += GenerateFilesUtils.GetIndentationStr(1, 4, "def " + glueTopic.Key + "_get_value_" + localVar.LocalVarName + "(self, __input):");
                        result += GenerateFilesUtils.GetIndentationStr(2, 4, GetCodeLineWithLocalVarRefference(localVar.AssignmentCode, localVarNames), true, true);

                        result += Environment.NewLine;
                    }
                }
            }

            result += @"
    def initLocalVars(self, moduleNameToInit):
        if DEBUG:
            print(""initLocalVars:"")
            print(moduleNameToInit)
        for moduleName, localVarNamesAndValuesPerModule in self.localVarNamesAndValues.items():
            for localVarName, value in localVarNamesAndValuesPerModule.items():
                if moduleName == moduleNameToInit:
                    if DEBUG:
                        print (""init var:"")
                        print(localVarName)
                    aos_local_var_collection.replace_one({""Module"": moduleName, ""VarName"": localVarName},
                                                         {""Module"": moduleName, ""VarName"": localVarName, ""Value"": value},
                                                         upsert=True)
                    aosStats_local_var_collection.insert_one(
                        {""Module"": moduleName, ""VarName"": localVarName, ""value"": value, ""Time"": datetime.datetime.utcnow()})


    def setListenTarget(self, _listenTargetModule):
        self.initLocalVars(_listenTargetModule)
        if DEBUG:
            print('setListenTopicTargetModule:')
            print(_listenTargetModule)
        self.listenTargetModule = _listenTargetModule
";
            Dictionary<string, List<GlueLocalVariablesInitialization>> rosParamVariables = new Dictionary<string, List<GlueLocalVariablesInitialization>>();
            foreach (RosGlue glue in data.RosGlues.Values)
            {
                List<GlueLocalVariablesInitialization> glueParamVariables = glue.GlueLocalVariablesInitializations.Where(x => !string.IsNullOrEmpty(x.RosParameterPath)).ToList();
                if (glueParamVariables.Count > 0)
                {
                    rosParamVariables.Add(glue.Name, glueParamVariables);
                }
            }

            foreach (var glueRosParamLocalVars in rosParamVariables)
            {
                result += GenerateFilesUtils.GetIndentationStr(2, 4, "if self.listenTargetModule == \"" + glueRosParamLocalVars.Key + "\":");
                foreach (var localParam in glueRosParamLocalVars.Value)
                {
                    result += GenerateFilesUtils.GetIndentationStr(3, 4, "self.checkParameterValue_" + localParam.LocalVarName + "()");
                }
            }



            foreach (var glueRosParamLocalVars in rosParamVariables)
            {
                foreach (var localParam in glueRosParamLocalVars.Value)
                {
                    result += GenerateFilesUtils.GetIndentationStr(1, 4, "def checkParameterValue_" + localParam.LocalVarName + "(self):#TODO:: need to see how to update ROS parameters. using threading disable other topic listeners");
                    result += GenerateFilesUtils.GetIndentationStr(2, 4, "if self.listenTargetModule == \"" + glueRosParamLocalVars.Key + "\":");
                    result += GenerateFilesUtils.GetIndentationStr(3, 4, "try:");
                    result += GenerateFilesUtils.GetIndentationStr(4, 4, "#__input = rospy.get_param('" + localParam.RosParameterPath + "')");
                    result += GenerateFilesUtils.GetIndentationStr(4, 4, "#" + localParam.LocalVarName + " = __input");
                    result += GenerateFilesUtils.GetIndentationStr(4, 4, "#self.updateLocalVariableValue(\"" + localParam.LocalVarName + "\", " + localParam.LocalVarName + ")");
                    result += GenerateFilesUtils.GetIndentationStr(4, 4, "self.updateLocalVariableValue(\"" + localParam.LocalVarName + "\", True)");
                    result += GenerateFilesUtils.GetIndentationStr(3, 4, "except:");
                    result += GenerateFilesUtils.GetIndentationStr(4, 4, "pass");
                    result += GenerateFilesUtils.GetIndentationStr(3, 4, "#threading.Timer(1, self.checkParameterValue_" + localParam.LocalVarName + ").start()");
                }
            }




            result += @"

    def updateLocalVariableValue(self, varName, value):
        if DEBUG and varName not in getHeavyLocalVarList(self.listenTargetModule):
            print(""update local var:"")
            print(varName)
            print(value)
        if self.listenTargetModule not in self.localVarNamesAndValues:
            return
        if self.localVarNamesAndValues[self.listenTargetModule][varName] != value:
            if DEBUG:
                print(""ACTUAL UPDATE --------------------------------------------------------------------------"")
            self.localVarNamesAndValues[self.listenTargetModule][varName]=value
            if varName not in getHeavyLocalVarList(self.listenTargetModule):
                aos_local_var_collection.replace_one({""Module"": self.listenTargetModule, ""VarName"":varName}, {""Module"": self.listenTargetModule, ""VarName"":varName, ""Value"":value}, upsert=True)
                aosStats_local_var_collection.insert_one(
                    {""Module"": self.listenTargetModule, ""VarName"": varName, ""value"": value, ""Time"": datetime.datetime.utcnow()})
                if DEBUG:
                    print(""WAS UPDATED --------------------------------------------------------------------------"")

";
            return result;
        }


    private static string GetHeavyLocalVariablesList(PLPsData data)
    {
        string result = "";;
        foreach(PLP plp in data.PLPs.Values)
        {
            string plpHeavyVars = string.Join<String>(",", data.LocalVariablesListings.Where(x=>x.IsHeavyVariable && x.SkillName == plp.Name).Select(x=> "\"" + x.VariableName + "\"" ).ToArray());
            if (plpHeavyVars.Length > 0)
            {
                result += (result.Length > 0 ? ", " : "") + "\"" + plp.Name + "\" : [" + plpHeavyVars + "]";
            }
        } 
        return "HEAVY_LOCAL_VARS={" + result + "}";
    }
        public static string GetAosRosMiddlewareNodeFile(PLPsData data, InitializeProject initProj)
        {
            string pythonVersion = initProj.RosTarget.RosDistribution == "kinetic" ? "python" : "python3"; 
            string file = @"#!/usr/bin/"+pythonVersion+@"
import datetime
import rospy  
import pymongo
import operator
import traceback
" + GetImportsForMiddlewareNode(data, initProj) + @"
 
DEBUG = " + (initProj.MiddlewareConfiguration.DebugOn ? "True" : "False") + Environment.NewLine +
GetHeavyLocalVariablesList(data) + @"
aosDbConnection = pymongo.MongoClient(""mongodb://localhost:27017/"")
aosDB = aosDbConnection[""AOS""]
aos_statisticsDB = aosDbConnection[""AOS_Statistics""]
aos_local_var_collection = aosDB[""LocalVariables""]
aosStats_local_var_collection = aos_statisticsDB[""LocalVariables""]
aos_GlobalVariablesAssignments_collection = aosDB[""GlobalVariablesAssignments""]
aos_ModuleResponses_collection = aosDB[""ModuleResponses""]
collActionForExecution = aosDB[""ActionsForExecution""] 
collLogs = aosDB[""Logs""]
collActions = aosDB[""Actions""]

 


def registerError(errorStr,trace, comments=None):
    error = {""Component"": ""RosMiddleware"", ""Event"": errorStr,""Advanced"":trace,""LogLevel"":2,""LogLevelDesc"":""Error"",
             ""Time"": datetime.datetime.utcnow()}
    if comments is not None:
        error = {""Component"": ""RosMiddleware"", ""Error"": errorStr, ""Advanced"": str(comments) + "". ""+str(trace), ""Time"": datetime.datetime.utcnow()}
    collLogs.insert_one(error)


def registerLog(str):
    log = {""Component"": ""RosMiddleware"", ""Event"": str, ""LogLevel"":5,""LogLevelDesc"":""Debug"",""Advanced"":"""",
             ""Time"": datetime.datetime.utcnow()}
    collLogs.insert_one(log)

def getHeavyLocalVarList(moduleName):
    if moduleName in HEAVY_LOCAL_VARS:
        return HEAVY_LOCAL_VARS[moduleName]
    else:
        return []

" + GetLocalVariableTypeClasses(data) + @"

class ListenToMongoDbCommands:
" + GetListenToMongoDbCommandsInitFunction(data) + @"


" + GetHandleModuleFunction(data) + @"


    def saveHeavyLocalVariableToDB(self, moduleName):
        for varName in getHeavyLocalVarList(moduleName):
            value = self._topicListener.localVarNamesAndValues[moduleName][varName]
            aos_local_var_collection.replace_one({""Module"": moduleName, ""VarName"": varName},
                                                 {""Module"": moduleName, ""VarName"": varName,
                                                  ""Value"": value}, upsert=True)
            aosStats_local_var_collection.insert_one(
                {""Module"": moduleName, ""VarName"": varName, ""value"": value,
                 ""Time"": datetime.datetime.utcnow()})




    def registerModuleResponse(self, moduleName, startTime, actionSequenceID, responseNotByLocalVariables):
        self.saveHeavyLocalVariableToDB(moduleName)
        filter1 = {""ActionSequenceId"": actionSequenceID}
        if DEBUG:
            print(""registerModuleResponse()"")

        if responseNotByLocalVariables is not None:
            moduleResponseItem = {""Module"": moduleName, ""ActionSequenceId"": actionSequenceID,
                                  ""ModuleResponseText"": responseNotByLocalVariables, ""StartTime"": startTime,
                                  ""EndTime"": datetime.datetime.utcnow(),
                                  ""ActionForExecutionId"": self.currentActionFotExecutionId}
            #aos_ModuleResponses_collection.insert_one(moduleResponseItem)
            aos_ModuleResponses_collection.replace_one(filter1,moduleResponseItem, upsert=True)
            return

        moduleResponse = """"
        assignGlobalVar = {}
" + GetModuleResponseFunctionPart(data) + @"

        if DEBUG and len(getHeavyLocalVarList(moduleName)) == 0:
            print(""moduleResponse result:"")
            print(moduleResponse)
        moduleLocalVars = {}
        if moduleName in self._topicListener.localVarNamesAndValues.keys():
            moduleLocalVars=self._topicListener.localVarNamesAndValues[moduleName]
        moduleResponseItem = {""Module"": moduleName, ""ActionSequenceId"": actionSequenceID,
                ""ModuleResponseText"": moduleResponse, ""StartTime"": startTime, ""EndTime"": datetime.datetime.utcnow(), ""ActionForExecutionId"":self.currentActionFotExecutionId, 
                ""LocalVariables"":moduleLocalVars}


        #aos_ModuleResponses_collection.insert_one(moduleResponseItem)
        aos_ModuleResponses_collection.replace_one(filter1,moduleResponseItem, upsert=True)
        for varName, value in assignGlobalVar.items():
            isInit = False
            if value is not None:
                isInit = True
            aos_GlobalVariablesAssignments_collection.replace_one({""GlobalVariableName"": varName},
                                                                  {""GlobalVariableName"": varName, ""LowLevelValue"": value,
                                                                   ""IsInitialized"": isInit, ""UpdatingActionSequenceId"":actionSequenceID,
                                                                   ""ModuleResponseId"":moduleResponseItem[""_id""]}, upsert=True)

    def listen_to_mongodb_commands(self):
        while(True):
            filter1 = {""ActionSequenceId"": self.currentActionSequenceID}
            actionForExecution = collActionForExecution.find_one(filter1)
            if(actionForExecution is not None):
                if DEBUG:
                    print(""~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~"")
                    print(""actionForExecution:"")
                    print(actionForExecution)
                    print(""actionID:"")
                    print(actionForExecution[""ActionID""])
                moduleName = actionForExecution[""ActionName""]
                actionParameters = actionForExecution[""Parameters""] 
                self.currentActionFotExecutionId = actionForExecution[""_id""]
                self._topicListener.setListenTarget(moduleName)
                rospy.sleep(0.3)#0.03 is a tested duration, not to drop updates
                moduleActivationStartTime = datetime.datetime.utcnow()
                responseNotByLocalVariables = None
                print(""module name:"")
                print(moduleName)
                registerLog(""Request to call to module:""+moduleName)
                
" + GetListenToMongoCommandsFunctionPart(data) + @"
                rospy.sleep(0.3)#0.015 is a tested duration, not to drop updates
                self._topicListener.setListenTarget(""after action"")

                self.registerModuleResponse(moduleName, moduleActivationStartTime, self.currentActionSequenceID, responseNotByLocalVariables)
                if DEBUG:
                    print(""self.currentActionSequenceID:"")
                    print(self.currentActionSequenceID)
                self.currentActionSequenceID = self.currentActionSequenceID +1
                if DEBUG:
                    print(""self.currentActionSequenceID:"")
                    print(self.currentActionSequenceID)
                self.currentActionFotExecutionId = None
            rospy.sleep(0.1)

" + GetAOS_TopicListenerServerClass(data) + @"





" + GetAOS_InitEnvironmentFile(data) + @"





if __name__ == '__main__':
    try:
        rospy.init_node('aos_ros_middleware_auto', anonymous=True)
        AOS_InitEnvironmentFile()
        topicListener = AOS_TopicListenerServer()
        commandlistener = ListenToMongoDbCommands(topicListener)
    except Exception as e:
        registerError(str(e), traceback.format_exc(e))
    ";
            return file;
        }

        private static Dictionary<string, string> GetLocalConstantAssignments(PLPsData data, HashSet<string> constants)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            List<string> tempCodeLines = data.GlobalVariableDeclarations.Select(x => x.DefaultCode).Where(x => !string.IsNullOrEmpty(x)).ToList();
            List<string> codeLines = new List<string>();
            foreach (string codeLine in tempCodeLines)
            {
                codeLines.AddRange(codeLine.Replace("if", "").Replace("else", "").Replace("{", "").Replace("}", "").Replace(" ", "").Split(";")
                    .Where(x => !string.IsNullOrEmpty(x) && constants.Any(sConst => x.Contains(sConst))).ToList());
            }

            foreach (string line in codeLines)
            {
                string[] bits = line.Split("=");
                if (bits.Length != 2) throw new Exception("unexpected code ('" + line + "')");
                result[bits[0]] = bits[1];
            }




            return result;
        }

        private static string GetAOS_InitEnvironmentFile(PLPsData data)
        {
            string result = "";
            result += GenerateFilesUtils.GetIndentationStr(0, 4, "class AOS_InitEnvironmentFile:");
            result += GenerateFilesUtils.GetIndentationStr(1, 4, "def __init__(self):");
            result += GenerateFilesUtils.GetIndentationStr(2, 4, "pass");

            Dictionary<string, LocalVariableConstant> constants = new Dictionary<string, LocalVariableConstant>();
            foreach (var lConst in data.LocalVariableConstants)
            {
                constants[lConst.Name] = lConst;
                if (!GenerateFilesUtils.IsPrimitiveType(lConst.Type))
                {
                    result += GenerateFilesUtils.GetIndentationStr(2, 4, lConst.Name + " = " + lConst.Type + "()");
                }
                result += GenerateFilesUtils.GetIndentationStr(2, 4, lConst.InitCode, true, true);
                result += GenerateFilesUtils.GetIndentationStr(1, 4, "");
                result += Environment.NewLine;
            }

            Dictionary<string, string> assignments = GetLocalConstantAssignments(data, constants.Select(x => x.Key).ToHashSet<string>());
            foreach (var assignment in assignments)
            {
                string value = "";
                value = GenerateFilesUtils.IsPrimitiveType(constants[assignment.Value].Type) ? assignment.Value : constants[assignment.Value].Type + "ToDict(" + constants[assignment.Value].Name + ")";
                result += GenerateFilesUtils.GetIndentationStr(2, 4, "self.updateGlobalVarLowLevelValue(\"" + assignment.Key + "\"," + value + ")");
            }

            foreach (var anyValueVar in data.AnyValueStateVariableNames)
            {
                bool added = false;
                foreach (var assignment in assignments)
                {
                    added |= assignment.Key.Equals(anyValueVar);
                    if (added) break;
                }
                if (!added)
                {
                    result += GenerateFilesUtils.GetIndentationStr(2, 4, "self.updateGlobalVarLowLevelValue(\"" + anyValueVar + "\", None)");
                }
            }


            result += @"
    def updateGlobalVarLowLevelValue(self, varName, value):
        isInit = value is not None
        aos_GlobalVariablesAssignments_collection.replace_one({""GlobalVariableName"": varName},{""GlobalVariableName"": varName, ""LowLevelValue"": value,
                                                                                               ""IsInitialized"": isInit, ""UpdatingActionSequenceId"": ""initialization"",
                                                                                               ""ModuleResponseId"": ""initialization""},upsert=True)


";
            return result;
        }
    }
}