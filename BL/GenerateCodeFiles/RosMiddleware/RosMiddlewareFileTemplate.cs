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


        public static string GetAosRosMiddlewareNodeFile(PLPsData data, InitializeProject initProj)
        {
            string file = @"#!/usr/bin/python
import datetime
import rospy 
from aos_ros_target_project.msg import *
from aos_ros_target_project.srv import *
from std_msgs.msg import String, Bool
from geometry_msgs.msg import Twist
import pymongo 
DEBUG = True
aosDbConnection = pymongo.MongoClient(""mongodb://localhost:27017/"")
aosDB = aosDbConnection[""AOS""]
aos_statisticsDB = aosDbConnection[""AOS_Statistics""]
aos_local_var_collection = aosDB[""LocalVariables""]
aosStats_local_var_collection = aos_statisticsDB[""LocalVariables""]
aos_GlobalVariablesAssignments_collection = aosDB[""GlobalVariablesAssignments""]
aos_ModuleResponses_collection = aosDB[""ModuleResponses""]
collActionForExecution = aosDB[""ActionsForExecution""]
collActions = aosDB[""Actions""]


def ltLocationToDict(lt):
    return {""x"": lt.x, ""y"": lt.y, ""z"": lt.z}

class ListenToMongoDbCommands:
    def __init__(self, _topicListener):
        self.currentActionSequenceID = 1
        self.currentActionFotExecutionId = None
        self._topicListener = _topicListener
        self.readyToActivate = """" 
        self.pickServiceName = ""/arm_manipulation/pick""
        self.observeServiceName = '/custom_actions/observe'
        self.navigateServiceName = '/custom_actions/navigate'
        self.placeServiceName = '/arm_manipulation/place'

        self.listen_to_mongodb_commands()


    def handle_pick(self, params):
        responseNotByLocalVariables = None
        cupAccurateLocation = """"
        obj_location = """"
        try:
            rospy.wait_for_service(self.pickServiceName)

            # get global variable low-level value
            cupAccurateLocation = aos_GlobalVariablesAssignments_collection.find_one(
                {""GlobalVariableName"": ""state.cupAccurateLocation""})

            obj_location = ltLocation(x=cupAccurateLocation[""LowLevelValue""][""x""],
                                      y=cupAccurateLocation[""LowLevelValue""][""y""],
                                      z=cupAccurateLocation[""LowLevelValue""][""z""])
        except:
            print(""illegalActionObs"")
            responseNotByLocalVariables = ""illegalActionObs""
        try:
            pick_proxy = rospy.ServiceProxy(self.pickServiceName, pick)
            req = pickLocation(x= obj_location.x, y=obj_location.y, z= obj_location.z)
            pick_proxy(req)
            if DEBUG:
                print(""pick service terminated"")

        except:
            print(""Service call failed"")
        return responseNotByLocalVariables

    def handle_place(self, params):
        responseNotByLocalVariables = None
        rospy.wait_for_service(self.placeServiceName)

        try:
            place_proxy = rospy.ServiceProxy(self.placeServiceName, place)
            place_proxy()
            if DEBUG:
                print(""place service terminated"")

        except rospy.ServiceException as e:
            print(""Service call failed: %s"" % e)
        return responseNotByLocalVariables

    def handle_navigate(self, params):
        responseNotByLocalVariables = None
        rospy.wait_for_service(self.navigateServiceName)

        fromGlobalVar = ""oDesiredLocation.actual_location""
      

        stateVarName = params[""ParameterLinks""][""oDesiredLocation""]
       
        fromGlobalVar = fromGlobalVar.replace(""oDesiredLocation."",stateVarName + ""."")
      
        oDesiredLocation_actual_locationDB = aos_GlobalVariablesAssignments_collection.find_one(
            {""GlobalVariableName"": fromGlobalVar})

        desired_location = ltLocation(x=oDesiredLocation_actual_locationDB[""LowLevelValue""][""x""], y=oDesiredLocation_actual_locationDB[""LowLevelValue""][""y""],
                                  z=oDesiredLocation_actual_locationDB[""LowLevelValue""][""z""])

        try:
            navigate_proxy = rospy.ServiceProxy(self.navigateServiceName, navigate)
            goal_location = pickLocation()
            goal_location.x= desired_location.x
            goal_location.y=desired_location.y
            goal_location.z= desired_location.z


            navigate_proxy(goal_location=goal_location)
            if DEBUG:
                print(""navigate service terminated"")

        except rospy.ServiceException as e:
            print(""Service call failed: %s"" % e)
        return responseNotByLocalVariables

    def handle_observe(self, params):
        responseNotByLocalVariables = None
        try:
            observe_proxy = rospy.ServiceProxy(self.observeServiceName, observe)
            __res = observe_proxy()
            observedLocation={""x"": __res.observed_location.x, ""y"": __res.observed_location.y, ""z"": __res.observed_location.z}
            self._topicListener.updateLocalVariableValue(""observedLocation"",observedLocation)
            if DEBUG:
                print(""observe service terminated"")

        except rospy.ServiceException as e:
            print(""Service call failed: %s"" % e)
        return responseNotByLocalVariables

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
                if moduleName == ""pick"":
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
    rospy.init_node('" + GenerateRosMiddleware.ROS_MIDDLEWARE_PACKAGE_NAME + @"', anonymous=True)
    AOS_InitEnvironmentFile()
    topicListener = AOS_TopicListenerServer()
    commandlistener = ListenToMongoDbCommands(topicListener)
    ";
            return file;
        }
    }
}