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
            if (bits.Length == 0) return lv[0];
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

            string baseVarName = bits[0] + (bits.Length > 1 ? bits[1] : "");
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
                foreach (LocalVariablesInitializationFromGlobalVariable oGlVar in plp.LocalVariablesInitializationFromGlobalVariables)
                {
                    hasVar = true;
                    result += GenerateFilesUtils.GetIndentationStr(2, 4, oGlVar.InputLocalVariable + " = \"\"");
                }
                if (hasVar)
                {
                    result += GenerateFilesUtils.GetIndentationStr(2, 4, "try:");

                    foreach (LocalVariablesInitializationFromGlobalVariable oGlVar in plp.LocalVariablesInitializationFromGlobalVariables)
                    {
                        result += GenerateFilesUtils.GetIndentationStr(3, 4, "dbVar = aos_GlobalVariablesAssignments_collection.find_one({\"GlobalVariableName\": \"" + oGlVar.FromGlobalVariable + "\"})");

                        LocalVariableTypePLP underlineType = GetUnderlineLocalVariableTypeByVarName(data, plp, oGlVar.FromGlobalVariable);
                        if (underlineType != null)
                        {
                            result += GenerateFilesUtils.GetIndentationStr(3, 4, oGlVar.InputLocalVariable + " = " + underlineType.TypeName + "()");
                            foreach (LocalVariableCompoundTypeField field in underlineType.SubFields)
                            {
                                //obj_location.z=cupAccurateLocation[""LowLevelValue""][""z""]
                                result += GenerateFilesUtils.GetIndentationStr(3, 4, oGlVar.InputLocalVariable + "." + field.FieldName + " = dbVar[\"LowLevelValue\"][\"" + field.FieldName + "\"]");
                            }
                        }
                        else
                        {
                            result += GenerateFilesUtils.GetIndentationStr(3, 4, oGlVar.InputLocalVariable + " = dbVar[\"LowLevelValue\"]");
                        }
                    }
                    result += GenerateFilesUtils.GetIndentationStr(2, 4, "except:");
                    result += GenerateFilesUtils.GetIndentationStr(3, 4, "responseNotByLocalVariables = \"illegalActionObs\"");
                }

                if (glue.RosServiceActivation != null && !string.IsNullOrEmpty(glue.RosServiceActivation.ServiceName))
                {
                    result += GenerateFilesUtils.GetIndentationStr(2, 4, "try:");
                    result += GenerateFilesUtils.GetIndentationStr(3, 4, "rospy.wait_for_service(self." + glue.Name + "ServiceName)");
                    result += GenerateFilesUtils.GetIndentationStr(3, 4, glue.Name + "_proxy = rospy.ServiceProxy(self." + glue.Name + "ServiceName, " + glue.RosServiceActivation.ServiceName + ")");

                    string serviceCallParam = string.Join(", ", glue.RosServiceActivation.ParametersAssignments.Select(x => x.MsgFieldName + "=(" + x.AssignServiceFieldCode + ")"));


                    result += GenerateFilesUtils.GetIndentationStr(3, 4, "__res = " + glue.Name + "_proxy(" + serviceCallParam + ")");

                    result += GenerateFilesUtils.GetIndentationStr(3, 4, "if DEBUG:");
                    result += GenerateFilesUtils.GetIndentationStr(4, 4, "print(\"" + glue.Name + " service terminated\")");
                    result += GenerateFilesUtils.GetIndentationStr(2, 4, "except:");
                    result += GenerateFilesUtils.GetIndentationStr(3, 4, "print(\"Service call failed\")");
                    result += GenerateFilesUtils.GetIndentationStr(2, 4, "");
                }


                result += GenerateFilesUtils.GetIndentationStr(2, 4, "return responseNotByLocalVariables");

            }
            return result;
        }
        public static string GetAosRosMiddlewareNodeFile(PLPsData data, InitializeProject initProj)
        {

            string file = @"#!/usr/bin/python
import datetime
import rospy  
import pymongo
" + GetImportsForMiddlewareNode(data, initProj) + @"
 
DEBUG = False
aosDbConnection = pymongo.MongoClient(""mongodb://localhost:27017/"")
aosDB = aosDbConnection[""AOS""]
aos_statisticsDB = aosDbConnection[""AOS_Statistics""]
aos_local_var_collection = aosDB[""LocalVariables""]
aosStats_local_var_collection = aos_statisticsDB[""LocalVariables""]
aos_GlobalVariablesAssignments_collection = aosDB[""GlobalVariablesAssignments""]
aos_ModuleResponses_collection = aosDB[""ModuleResponses""]
collActionForExecution = aosDB[""ActionsForExecution""]
collActions = aosDB[""Actions""]



" + GetLocalVariableTypeClasses(data) + @"

class ListenToMongoDbCommands:
" + GetListenToMongoDbCommandsInitFunction(data) + @"


" + GetHandleModuleFunction(data) + @"

    def registerModuleResponse(self, moduleName, startTime, actionSequenceID, responseNotByLocalVariables):
        if DEBUG:
            print(""registerModuleResponse()"")

        if responseNotByLocalVariables is not None:
            moduleResponseItem = {""Module"": moduleName, ""ActionSequenceId"": actionSequenceID,
                                  ""ModuleResponseText"": responseNotByLocalVariables, ""StartTime"": startTime,
                                  ""EndTime"": datetime.datetime.utcnow(),
                                  ""ActionForExecutionId"": self.currentActionFotExecutionId}
            aos_ModuleResponses_collection.insert_one(moduleResponseItem)

        moduleResponse = """"
        assignGlobalVar = {}
        if moduleName == ""pick"":
            top_gripper_pressure = self._topicListener.localVarNamesAndValues[""pick""][""top_gripper_pressure""]
            gripper_opening = self._topicListener.localVarNamesAndValues[""pick""][""gripper_opening""]
            gripper_pressure = self._topicListener.localVarNamesAndValues[""pick""][""gripper_pressure""]

            if DEBUG:
                print(""pick action local variables:"")
                print(""top_gripper_pressure"")
                print(top_gripper_pressure)
                print(""gripper_opening:"")
                print(gripper_opening)
                print(""gripper_pressure:"")
                print(gripper_pressure)
            if moduleResponse == """" and gripper_pressure > 0 and gripper_opening > 0:
                moduleResponse = ""pick_res_pick_action_success""
            if moduleResponse == """" and gripper_pressure == 0:
                moduleResponse = ""pick_res_not_holding""
            if moduleResponse == """" and top_gripper_pressure > 200:
                moduleResponse = ""pick_res_broke_the_object""

        if moduleName == ""place"":
            dropped_object = self._topicListener.localVarNamesAndValues[""place""][""dropped_object""]
            gripper_pressure = self._topicListener.localVarNamesAndValues[""place""][""gripper_pressure""]
            arm_attached_to_object_at_end = self._topicListener.localVarNamesAndValues[""place""][""arm_attached_to_object_at_end""]
            arm_attached_to_object_at_start = self._topicListener.localVarNamesAndValues[""place""][""arm_attached_to_object_at_start""]
            arm_plan_path_success = self._topicListener.localVarNamesAndValues[""place""][""arm_plan_path_success""]
            if moduleResponse == """" and arm_plan_path_success == True and  arm_attached_to_object_at_start == True and arm_attached_to_object_at_end == False and dropped_object == False:
                moduleResponse = ""place_ePlaceActionSuccess""
            if moduleResponse == """" and dropped_object == True:
                moduleResponse = ""place_eDroppedObject""
            if moduleResponse == """" and True:
                moduleResponse = ""place_eFailedUnknown""

        if moduleName == ""observe"":
            self.listenTargetModule = ""none""
            observedLocation = self._topicListener.localVarNamesAndValues[""observe""][""observedLocation""]
            if moduleResponse == """" and observedLocation is None:
                moduleResponse = ""observe_eNotObserved""
                varName1 = ""state.cupAccurateLocation""
                value1 = None
                assignGlobalVar[varName1] = value1
            if moduleResponse == """" and observedLocation is not None:
                moduleResponse = ""observe_eObserved""
                varName1 = ""state.cupAccurateLocation""
                value1 = observedLocation
                assignGlobalVar[varName1] = value1

        if moduleName == ""navigate"":
            navigationModuleSuccessOutput = self._topicListener.localVarNamesAndValues[""navigate""][""navigationModuleSuccessOutput""]
            if moduleResponse == """" and navigationModuleSuccessOutput == True:
                moduleResponse = 'navigate_eSuccess'
            if moduleResponse == """" and True:
                moduleResponse = 'navigate_eFailed'

        if DEBUG:
            print(""moduleResponse result:"")
            print(moduleResponse)
        moduleResponseItem = {""Module"": moduleName, ""ActionSequenceId"": actionSequenceID,
                ""ModuleResponseText"": moduleResponse, ""StartTime"": startTime, ""EndTime"": datetime.datetime.utcnow(), ""ActionForExecutionId"":self.currentActionFotExecutionId}


        aos_ModuleResponses_collection.insert_one(moduleResponseItem)
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
                if moduleName == ""pick"":
                    print(""handle pick"")
                    responseNotByLocalVariables = self.handle_pick(actionParameters)
                if moduleName == ""observe"":
                    responseNotByLocalVariables = self.handle_observe(actionParameters)
                if moduleName == ""navigate"":
                    responseNotByLocalVariables = self.handle_navigate(actionParameters)
                if moduleName == ""place"":
                    responseNotByLocalVariables = self.handle_place(actionParameters)
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

class AOS_TopicListenerServer:
    def __init__(self):
        self.localVarNamesAndValues = {""pick"":{""top_gripper_pressure"": 0, ""gripper_opening"": None, ""gripper_pressure"": None},
                                       ""observe"":{""observedLocation"": None},
                                       ""navigate"":{""navigationModuleSuccessOutput"": False},
                                       ""place"":{""arm_plan_path_success"": None, ""arm_attached_to_object_at_start"": None, ""arm_attached_to_object_at_end"": None, ""gripper_pressure"":None, ""dropped_object"":None}}

        self.setListenTarget(""initTopicListener"")
        rospy.Subscriber(""/gripper/gripper_pressure"", Twist, self.cb_gripper_gripper_pressure, queue_size=1000)
        rospy.Subscriber(""/gripper/gripper_opening"", Force, self.cb__gripper_gripper_opening, queue_size=1000)
        rospy.Subscriber(""/navigation/planner_output"", String, self.cb_navigation_planner_output, queue_size=1000)
        rospy.Subscriber(""/moveit/collisions/arm_collision"", Bool, self.cb_moveit_collisions_arm_collision, queue_size=1000)

    def cb_moveit_collisions_arm_collision(self, data):
        if self.listenTargetModule == ""place"":
            if DEBUG:
                print(""handling topic call:place"")
                print(data)
            # ----------------------------------------------------------
            value = self.place_get_value_arm_attached_to_object_at_end(data)
            self.updateLocalVariableValue(""arm_attached_to_object_at_end"",value)
            # ----------------------------------------------------------
            value = self.place_get_value_arm_attached_to_object_at_start(data)
            self.updateLocalVariableValue(""arm_attached_to_object_at_start"", value)

    def cb_navigation_planner_output(self, data):
        if self.listenTargetModule == ""navigate"":
            if DEBUG:
                print(""handling topic call:navigate"")
                print(data)
            # ----------------------------------------------------------
            value = self.navigate_get_value_navigationModuleSuccessOutput(data)
            self.updateLocalVariableValue(""navigationModuleSuccessOutput"",value)

    def cb__gripper_gripper_opening(self, data):
        if self.listenTargetModule == ""pick"":
            if DEBUG:
                print(""handling topic call:pick"")
                print(data)
            # ----------------------------------------------------------
            value = self.pick_get_value_gripper_opening(data)
            self.updateLocalVariableValue(""gripper_opening"", value)

    def cb_gripper_gripper_pressure(self, data):
        if self.listenTargetModule == ""pick"":
            if DEBUG:
                print(""handling topic call:pick"")
                print(data)
            # ----------------------------------------------------------
            value = self.pick_get_value_gripper_pressure(data)
            self.updateLocalVariableValue(""gripper_pressure"", value)
            #----------------------------------------------------------
            value = self.pick_get_value_top_gripper_pressure(data)
            self.updateLocalVariableValue(""top_gripper_pressure"", value)

        if self.listenTargetModule == ""place"":
            if DEBUG:
                print(""handling topic call:place"")
                print(data)
            # ----------------------------------------------------------
            value = self.place_get_value_gripper_pressure(data)
            self.updateLocalVariableValue(""gripper_pressure"", value)
            # ----------------------------------------------------------
            value = self.place_get_value_dropped_object(data)
            self.updateLocalVariableValue(""dropped_object"", value)

    def place_get_value_dropped_object(self, __input):
        if self.localVarNamesAndValues[self.listenTargetModule][""dropped_object""] == True:
            return self.localVarNamesAndValues[self.listenTargetModule][""dropped_object""]
        else:
            return self.localVarNamesAndValues[self.listenTargetModule][""arm_attached_to_object_at_end""] == False and __input.linear.x == 0



    def place_get_value_gripper_pressure(self, __input):
        return __input.linear.x



    def place_get_value_arm_attached_to_object_at_end(self, __input):
        return __input.data



    def place_get_value_arm_attached_to_object_at_start(self, __input):
        if self.localVarNamesAndValues[self.listenTargetModule][""arm_attached_to_object_at_start""] == True:
            return True
        else:
            return __input.data

    def checkParameterValue_arm_plan_path_success(self):#TODO:: need to see how to update ROS parameters. using threading disable other topic listeners
        if self.listenTargetModule == ""place"":
            try:
                #__input = rospy.get_param('/moveit/plan_path/planning_success')
                #arm_plan_path_success = __input
                #self.updateLocalVariableValue(""arm_plan_path_success"", arm_plan_path_success)
                self.updateLocalVariableValue(""arm_plan_path_success"", True)
            except:
                pass
            #threading.Timer(1, self.checkParameterValue_arm_plan_path_success).start()


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
        if self.listenTargetModule == ""place"":
            self.checkParameterValue_arm_plan_path_success()

    def updateLocalVariableValue(self, varName, value):
        if DEBUG:
            print(""update local var:"")
            print(varName)
            print(value)
        if self.localVarNamesAndValues[self.listenTargetModule][varName] != value:
            if DEBUG:
                print(""ACTUAL UPDATE --------------------------------------------------------------------------"")
            self.localVarNamesAndValues[self.listenTargetModule][varName]=value
            aos_local_var_collection.replace_one({""Module"": self.listenTargetModule, ""VarName"":varName}, {""Module"": self.listenTargetModule, ""VarName"":varName, ""Value"":value}, upsert=True)
            aosStats_local_var_collection.insert_one(
                {""Module"": self.listenTargetModule, ""VarName"": varName, ""value"": value, ""Time"": datetime.datetime.utcnow()})
            if DEBUG:
                print(""WAS UPDATED --------------------------------------------------------------------------"")


    def pick_get_value_gripper_pressure(self, __input):
        return __input.linear.x

    def pick_get_value_top_gripper_pressure(self, __input):
        if self.localVarNamesAndValues[self.listenTargetModule][""top_gripper_pressure""] > __input.linear.x:
            return self.localVarNamesAndValues[self.listenTargetModule][""top_gripper_pressure""]
        else:
            return __input.linear.x



    def pick_get_value_gripper_opening(self, __input):
        return __input.x



    def navigate_get_value_navigationModuleSuccessOutput(self, __input):
        if self.localVarNamesAndValues[self.listenTargetModule][""navigationModuleSuccessOutput""] == True:
            return True
        return __input.data.find('success') > -1





class AOS_InitEnvironmentFile:
    def __init__(self):
        ___locationOutside_lab211 = ltLocation()
        ___locationOutside_lab211.x = 2
        ___locationOutside_lab211.y = 3

        ___locationAuditorium = ltLocation()
        ___locationAuditorium.x = 4
        ___locationAuditorium.y = 5

        ___locationNear_elevator1 = ltLocation()
        ___locationNear_elevator1.x = 6
        ___locationNear_elevator1.y = 7

        ___locationCorridor = ltLocation()
        ___locationCorridor.x = 8
        ___locationCorridor.y = 9

        self.updateGlobalVarLowLevelValue(""state.locationOutside_lab211.actual_location"",ltLocationToDict(___locationOutside_lab211))
        self.updateGlobalVarLowLevelValue(""state.locationAuditorium.actual_location"", ltLocationToDict(___locationAuditorium))
        self.updateGlobalVarLowLevelValue(""state.locationNear_elevator1.actual_location"", ltLocationToDict(___locationNear_elevator1))
        self.updateGlobalVarLowLevelValue(""state.locationCorridor.actual_location"", ltLocationToDict(___locationCorridor))

    def updateGlobalVarLowLevelValue(self, varName, value):
        aos_GlobalVariablesAssignments_collection.replace_one({""GlobalVariableName"": varName},{""GlobalVariableName"": varName, ""LowLevelValue"": value,
                                                                                               ""IsInitialized"": True, ""UpdatingActionSequenceId"": ""initialization"",
                                                                                               ""ModuleResponseId"": ""initialization""},upsert=True)





if __name__ == '__main__':
    rospy.init_node('aos_ros_middleware_auto', anonymous=True)
    AOS_InitEnvironmentFile()
    topicListener = AOS_TopicListenerServer()
    commandlistener = ListenToMongoDbCommands(topicListener)
    ";
            return file;
        }
    }
}