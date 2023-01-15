# AOS Documentation (version 1.2)
* [General](#general)
  - [General](#general)
  - [The AOS documentation structure](#the-aos-documentation-structure)
* [Environment File](#environment-file)
  - [PlpMain section](#plpmain)
  - [EnvironmentGeneral section](#environmentgeneral)
  - [GlobalVariableTypes section](#globalvariabletypes)
  - [GlobalVariablesDeclaration section](#globalvariablesdeclaration)
  - [ExtrinsicChangesDynamicModel section](#extrinsicchangesdynamicmodel)
    - [Assignments blocks](#assignments-blocks) 
  - [InitialBeliefStateAssignments section](#initialbeliefstateassignments)
  - [SpecialStates section](#specialstates)
* [The three sets of state variables](#the-three-sets-of-state-variables)
* [Skills Documentation](#skills-documentation)
* [Skill Documentation (SD) file](#skill-documentation-sd-file)
  - [PlpMain (for SD) section](#plpmain-sd)
  - [GlobalVariableModuleParameters section](#globalvariablemoduleparameters)
  - [Assisting the planner](#assisting-the-planner) 
    - [GlobalVariablePreconditionAssignments section](#globalvariablepreconditionassignments) 
    - [PlannerAssistancePreconditionsAssignments section](#plannerassistancepreconditionsassignments)
  - [DynamicModel](#dynamicmodel)
* [Abstraction Mapping (AM) file](#abstraction-mapping-am-file)
  -  [PlpMain (for AM) section](#plpmain-am)
  -  [Robot framework (GlueFramework)](#glueframework)
  -  [ModuleResponse section](#moduleresponse)
    - [ResponseRules](#responserules)
  - [ModuleActivation](#moduleactivation)
    - [RosService section](#rosservice)
  - [LocalVariablesInitialization section](#localvariablesinitialization)
* [Additional documentation language functionality](#additional-documentation-language-functionality)
  - [Sample from Discrete distributions](#sample-from-discrete-distribution)
  - [Sample from Bernoulli distributions](#sample-from-bernoulli-distribution)
  - [Iterate over all state variables of a specific type](#iterate-over-all-state-variables-of-a-specific-type)
   

# General
## The AOS documentation: General
The AOS is designed to assist engineers in integrating AI capabilities within their robots. The engineers should only document their basic skills (e.g., navigation, object detection, etc.) and robot objectives, and the AOS will use this documentation to operate the robot using automated decision-making techniques (online planning algorithms or deep reinforcement learning). It will reason when and how to activate each skill and invoke the skill code.</br>

## The AOS documentation structure:
For each robot project, the user should document a single environment file, and for each skill, a Skill Documentation (SD) file and an Abstraction Mapping (AM) file (all files are in JSON format).</br>
 
# Environment File
The environment file is used to specify documentation sections that are not skill-specific. The user can define the state variables, initial belief state of the robot, extrinsic events, and general robot objectives (special states: desired, undesired, or goals).</br>

The environment file structure is:</br>
```
{
    "PlpMain": { },
    "EnvironmentGeneral": { },
    "GlobalVariableTypes": [ ],
    "GlobalVariablesDeclaration": [ ],
    "InitialBeliefStateAssignments": [ ],
    "SpecialStates": [ ],
    "ExtrinsicChangesDynamicModel": [ ]
}

```
See [environment file template](https://github.com/orhaimwerthaim/AOS-WebAPI/blob/master/docs/version2/File%20Templates/environment.json)</br>
We now explain in detail about each section in the environment file.</br>

## PlpMain
The "PlpMain" is the header section that each documentation file contains. </br>
It has the following fields:</br>
* "Project" field of type string. It describes the project name, all documentation files in the project must have the same name (e.g., "cleaning_robot1").</br>
* "Name" field of type string. Stores the file name; environment files should always be named "environment," skill's AM and SD files are named by the skill they document, and corresponding AM and SD files must have the same name.</br>
* "Type" field of type string. It is the file type. "Environment" for environment files, "PLP" for SD files, and "Glue" for AM files.</br>
* "Version" should contain the current documentation version ("Version":1.2)</br>
Example:</br>
```
"PlpMain": {
        "Project": "cleaning_robot1",
        "Name": "environment",
        "Type": "Environment",
        "Version": 1.2
    },
```

## EnvironmentGeneral
The "EnvironmentGeneral" describes the general parameters of the planning problem. </br>
It has the following fields:</br>
* "Horizon" field of type integer. It describes how many forward steps the AOS considers when it decides on an action. On sampling algorithms, a longer horizon means fewer trajectories sampled on the same planning period, so it is better to specify an exact number; remember that if the horizon is too small, the robot cannot simulate reaching the goal. 
* "Discount" field of type decimal. Legal values are in the range of (1,0). It defines the discount factor on the value of future rewards. For values close to 1, we don't care so much about when we receive a reward; on the other hand, for values close to 0, we only care about rewards received in the next step, ignoring the future. In robotic domains, where each action has a cost, a value close to 1 is recommended (e.g., 0.9999).  </br>

Example</br>
```
"EnvironmentGeneral": { 
        "Horizon": 50,
        "Discount": 0.9999
    },
```

## GlobalVariableTypes
The supported state variable types are the primitive C++ types (e.g., string, int, float). The "GlobalVariableTypes" section enables users to define custom state variable types. More specifically, enums and compound data structures that aggregate multiple data items.</br>
After a type is defined, the user may use it to define a state variable.</br>
Example:</br>
```
"GlobalVariableTypes": [
        {
            "TypeName": "tDiscreteLocation",
            "Type": "enum",
            "EnumValues": [
                "eCorridor",
                "eLocationAuditoriumSide1",
                "eLocationAuditoriumSide2",
                "eRobotHand",
                "eUnknown"
            ]
        },
        {
            "TypeName": "tLocation",
            "Type": "compound",
            "Variables": [
                {
                    "Name": "discrete_location",
                    "Type": "tDiscreteLocation",
                    "Default": "eUnknown"
                },
                {
                    "Name": "continuous_location_x",
                    "Type": "float"
                },
                {
                    "Name": "continuous_location_y",
                    "Type": "float"
                },
            ]
        }
    ],
```
In the above example, we see a user that defined a 'tLocation' type to define a location. Each tLocation contains a tDiscreteLocation that can be used for decision making and an abstract description of the model and continuous x,y coordinates of the robot that can be used for either decision making as a parameter sent to a navigation skill activation. </br>

## GlobalVariablesDeclaration
The "GlobalVariablesDeclaration" defines the state variables and possible action parameters.</br>
The field is an array of defined types and action parameters, each item contains the following fields:</br>
* "Name" of type string that defines the variables name.
* "Type" of type string that defines the state variable types. Supported types are the C++ primitive types (e.g., int, float, double, string, char, bool) and custom types (defined in the GlobalVariableTypes section).
* "DefaultCode" of type string can be used to initialize the variable. This field value contains C++ code. the variable is referred to as 'state.<Name>' where <Name> is replaced by the variable "Name".
* "IsActionParameterValue" is of type boolean (can take 'true' or 'false' values). It determines if this item is an action parameter. Action parameters are values the AOS can send as skill parameters. A Skill that takes a parameter of type int and a parameter of type string can receive any combination of action parameter values defined from type int and string. Suppose a user has two skills that take a parameter of the same type but have different possible values. In that case, the user should define a custom type that will wrap the skill parameters, and the skill will receive a parameter of that type.</br>
  
Example:</br>
```
"GlobalVariablesDeclaration": [
        {
            "Name": "locationCorridor",
            "Type": "tLocation",
            "DefaultCode": "state.locationCorridor.discrete_location = eCorridor; state.locationCorridor.continuous_location_x = 0.45343;state.locationCorridor.continuous_location_x = 0.1;",
            "IsActionParameterValue": true
        },
        {
            "Name": "locationAuditorium_toCan1",
            "Type": "tLocation",
            "DefaultCode": "state.locationAuditorium_toCan1.discrete_location = eLocationAuditoriumSide1; state.locationCorridor.continuous_location_x = 0.15;state.locationCorridor.continuous_location_x = 0.2;",
            "IsActionParameterValue": true
        },
        {
            "Name": "robotGenerallocation",
            "Type": "tDiscreteLocation"
        },
        {
            "Name": "holding_can",
            "Type": "bool",
            "DefaultCode": "state.holding_can=false;"
        }
    ],
```

## InitialBeliefStateAssignments
In many robotic domains, the initial state is unknown, and the robot should reason about this uncertainty. This section allows the user to define the uncertainty about the initial state using code. He can use the AOS extensions to sample from known distributions (Bernoulli, Discrete or Normal, [see](#additional-documentation-language-functionality)). </br>
The state variables can be referred to and changed using 'state.variable_name'. </br>
More technically, the AOS generates a particle set to represent the distribution of the initial belief state; each particle is a fully initialized state, and its values are taken from the default state variable value and sampled from this code section.</br>
#### Assignments blocks
There are a few sections of Assignments blocks like "InitialBeliefStateAssignments" they all have the same syntax.</br>
An Assignments block is an array of assignments. Each assignment item may have an "AssignmentName" for readability, and it must have an "AssignmentCode" field which is a string (a single code line) or an array of strings such that each string is a code line.</br>
Example:</br>
```
"InitialBeliefStateAssignments": [
    {
        "AssignmentName":"set locations",
        "AssignmentCode":
        [
            "if (AOSUtils::Bernoulli(0.75))",
	    "	{",
	    "		state.robotGenerallocation=eLocationAuditoriumSide1;",
	    "	}",
	    "else state.robotGenerallocation=eLocationAuditoriumSide1;

        ]
    },
    {
        "AssignmentName":"set holding_can",
        "AssignmentCode": "state.holding_can = Bernoulli(0.1);"
    }
    ]
```
## SpecialStates
The "SpecialStates" section uses to define desired or undesired states. The robot's goal is to maximize the expected discounted reward. This section allows the engineer to define desired or undesired states (e.g., "the drone should be balanced," "hitting a wall is terrible"). It can also define landmarks in the form of states the given a one-time reward the first time a state is reached (unlike "hitting a wall is terrible"). Moreover, the user can define goal rewards; these are termination conditions for the robot that ends its current operation (the robot will stop if its current belief distribution indicates a terminal state with a mean of more than 0.9 and a standard deviation of less than 0.1).</br>
Each special state has the following fields:</br>
* "StateConditionCode" field is a string code line that defines the condition indicating that we reached the state. The condition is defined over some or all state variable assignments, and it can only be defined in a single line (use lambda expressions for conditions that require iterating over multiple variables).
* "Reward" is a decimal field indicating the reward given when the condition is met.
* "IsGoalState" is a boolean field to define if it is a terminal state (default value is false).
* "IsOneTimeReward"  is a boolean field to define if the reward will be given only one time or multiple times (default value is false).
Example:</br>
```
 "SpecialStates": [
        {
            "StateConditionCode": "state.holding_can && state.robotGenerallocation == eCorridor",
            "Reward": 100.0,
            "IsGoalState": true
        },
        {
            "StateConditionCode": "state.holding_can",
            "Reward": 10.0,
            "IsGoalState": false,
	    "IsOneTimeReward":true
        }
    ],
```
## ExtrinsicChangesDynamicModel
This section uses to define extrinsic changes. These are changes that the robot did not invoke by its skills. For example, let's say there is a probability that it will start to rain, making the floor wet and making it harder to navigate. The agent did not invoke the rain, but it affected the robot's decisions.</br>
Example (this section is an [Assinment block](#assignments-blocks). See [The three sets of state variables](#the-three-sets-of-state-variables)):</br>
```
"ExtrinsicChangesDynamicModel": [
        {
            "AssignmentCode": "if (AOS.Bernoulli(0.05)) state_.robotLocation.discrete = -1;"
        }
    ]
```
# The three sets of state variables
The documentation describes how the state changes in each epoch conditioned on the selected skill. The state can change by either the robot's skills or extrinsic events. To assist the user in documenting complex behaviors flexibly, we introduce three sets of state variables, `state`, `state_`, and `state__`, each containing all of the defined state variables. </br>

`state` stores the previous state values and cannot be changed, `state_` is a copy of `state`, yet it can change by extrinsic events. Finally, `state__` is a copy of `state_` after the extrinsic events occur, and it can change by skill effects. 'ExtrinsicChangesDynamicModel' can only change variables in `state_` (can be conditioned on `state`), and skill's 'DynamicModel' can only change `state__` (can be conditioned on `state` and `state_`).
If we defined a state variable named `robotLocation`, it has three copies that can be referred to by either `state.robotLocation`, `state_.robotLocation`, or `state__.robotLocation`.

# Skills documentation
Each skill is documented using two files. First, a Skill Documentation (SD) file to describe the high level of the skill. Second, an Abstraction Mapping (AM) file to describe how to activate the skill code and how to translate the outcome of the skill execution to something the AOS can reason with for decision-making. 

# Skill Documentation (SD) file
The SD file structure is:</br>
```
{
    "PlpMain": {},
    "GlobalVariableModuleParameters": [ ], 
    "Preconditions": {
        "GlobalVariablePreconditionAssignments": [],
        "PlannerAssistancePreconditionsAssignments": [],
        "ViolatingPreconditionPenalty": 0
    },
    "DynamicModel": {
        "NextStateAssignments": [ ]
    }
}
```
See [environment file template](https://github.com/orhaimwerthaim/AOS-WebAPI/blob/master/docs/version2/File%20Templates/SD.json)

## PlpMain-SD
See6 [PlpMain](#plpmain) for detailed explanation.</br>
Example:</br>
```
"PlpMain": {
        "Project": "cleaning_robot1",
        "Name": "navigate",
        "Type": "PLP",
        "Version": 1.2
    },
```
## GlobalVariableModuleParameters
This section uses to define the parameters sent to a skill. The skill parameters are the context under which the skill operates. Possible parameters are, for example, 1) an enum value for navigation modes like 'fast' navigation vs. 'safe' navigation or 2) the navigation destination.
Formally the "GlobalVariableModuleParameters" is an array of parameter items, each containing the following fields:</br>
* "Name" is the string name to refer to the parameter. 
* "Type" is a string indicating the parameter type. </br>

The AOS considers activating a skill with each of the possible combinations of state variables marked as 'ActionParameterValue.' So if we have a skill with two parameters of type 'int' and three state variables of type int marked as 'ActionParameterValue,' the AOS will run simulations to select the best combination out of the six possible combinations. </br>

Example:</br>
```
"GlobalVariableModuleParameters": [
        {
            "Name": "oDestination",
            "Type": "tLocation"
        },
	{
            "Name": "oNavigationMode",
            "Type": "int"
        }
    ], 
```
The example above shows that user-defined types such as 'tLocation' can be used as skill parameters.</br>


## Assisting the planner
The engineer can guide the AOS to the direction of desired solutions.</br>

One way is to define preconditions to activate a skill. Since the robot is not always fully aware of its current state, it cannot entirely revoke illegal actions (e.g., try driving when there is no fuel). Nevertheless, the user can define when a skill is illegal and penalize activating it by its relative weight in the current belief state distribution.</br> 

Moreover, MCTS algorithms (supported by the AOS) build a search tree where each node is a distribution over states (belief state), and the leaf nodes are evaluated using a default (rollout) policy. The rollout policy is performed using a single state and is crucial to find a good solution. We can use the preconditions to revoke illegal skills.</br> 

Furthermore, the user can define preferred action to define the rollout policy.</br>

### GlobalVariablePreconditionAssignments
This section is used to define when a skill with given parameters met the preconditions (default is true). This field is an [Assinment block](#assignments-blocks), the should assign a value to the reserved variable `__meetPrecondition` and it can use (not change) any state variable from `state` (see [The three sets of state variables](#the-three-sets-of-state-variables)).</br>

Example:</br>
```
"Preconditions": {
	"GlobalVariablePreconditionAssignments": [
            {
                "AssignmentCode": "__meetPrecondition = oDestination != state.robotLocation;"
            }
          ],
	"ViolatingPreconditionPenalty":-1.5
}
```
The "ViolatingPreconditionPenalty" is a decimal field that allows engineers to tune how undesirable it is to activate a particular skill when its preconditions are not met.</br>

### PlannerAssistancePreconditionsAssignments
In this section, the user can define a default (rollout) policy. This field is an [Assinment block](#assignments-blocks) that should set the value of the reserved variable `__heuristicValue`. The code assignment can be conditioned on variable from `state` (but cannot change their value. See [The three sets of state variables](#the-three-sets-of-state-variables)). </br>
The default policy will draw between all available skills and parameter assignments. The weight of each skill will be its computed `__heuristicValue` </br>
Example (taken from the [push skill SD file](https://github.com/orhaimwerthaim/AOS-mini-project/blob/main/collectValuableToys_base/navigate.json) of the box-pushing domain found in the [AOS experiments GitHub](https://github.com/orhaimwerthaim/AOS-experiments)):</br>
```
"Preconditions":{
	"PlannerAssistancePreconditionsAssignments": [
            {
                "AssignmentName": "__heuristicValue for second agent joint push with first agent",
                "AssignmentCode": "if(oIsJointPush == JointPush && !state.isAgentOneTurn && oDirection == state.JointPushDirection)__heuristicValue=100;"
            },
            {
                "AssignmentName": "__heuristicValue push box when possible (don't ovverride first rule)",
                "AssignmentCode": "if(__heuristicValue == 0) __heuristicValue=1;"
            }
        ]
}
```
The default heuristic value for a skill is zero. This feature is ignored if no heuristic value is defined for any skill.</br>

## DynamicModel
The DynamicModel defines the high-level behavior of a skill. More specifically, the transition reward and observation models, how a skill changes the state, what costs (or rewards) are applied and which observations are returned. The "NextStateAssignments" is an [assignments block]() that sets the next state (`state__`), reward (`__reward` reserved variable), and observation (`__moduleResponse` reserved variable) conditioned on the previose state (`state`), the state after extrinsic changes (`state_`) and if the preconditions were met (`__meetPrecondition`). 

Example:</br>
```{r, attr.source='.numberLines'}
"DynamicModel": {
        "NextStateAssignments": [
            {
                "AssignmentCode": [" state__.robotLocation.discrete = !__meetPrecondition || AOS.Bernoulli(0.1) ? -1: oDesiredLocation.discrete;",
		" if(state__.robotLocation.discrete == oDesiredLocation.discrete){ state__.robotLocation.x = oDesiredLocation.x; state__.robotLocation.y = oDesiredLocation.y; state__.robotLocation.z = oDesiredLocation.z;}",
		"__moduleResponse = (state__.robotLocation.discrete == -1 && AOS.Bernoulli(0.8)) ? eFailed : eSuccess;",
		"__reward = state_.robotLocation.discrete == -1 ? -5 : -(sqrt(pow(state.robotLocation.x-oDesiredLocation.x,2.0)+pow(state.robotLocation.y-oDesiredLocation.y,2.0)))*10;",
                "if (state__.robotLocation.discrete == -1) __reward =  -10;"
                ]
           }]}
```

### SD observations must correspond to the AM observations
The observation must correspond to the observations specified in the AM file. The AOS runs simulations to decide the next best skill to apply. Next, the selected skill code is executed, and the AOS translates the execution outcome to an observation which is then used to update the distribution on the current state (current belief).

## Abstraction Mapping (AM) file
The AM file structure is:</br>
```
{
    "PlpMain": { },
    "GlueFramework": "",
    "ModuleResponse": { },
    "ModuleActivation": { },
    "LocalVariablesInitialization": []
}
```

See [environment file template](https://github.com/orhaimwerthaim/AOS-WebAPI/blob/master/docs/version2/File%20Templates/SD.json)

## PlpMain-AM
See [PlpMain](#plpmain) for detailed explanation.</br>
Example:</br>
```
"PlpMain": {
        "Project": "cleaning_robot1",
        "Name": "navigate",
        "Type": "Glue",
        "Version": 1.2
    },
```

## GlueFramework
GlueFramework is a string field to specify the type of robot framework (e.g., "ROS" for ROS1).</br>
Example:</br>
```
"GlueFramework": "ROS"
```
## ModuleResponse
ModuleResponse section defines the translation between an actual execution outcome of a skill, to observations the AOS planning engine can reason about. The planning engine uses the SD documentation to simulate what might happen. The AM ModuleResponse section is used to translate what really happened to the language used in the SD documentation (see [DynamicModel](#dynamicmodel)). The planning engine uses this information to update the robot's belief. 
### ResponseRules
The ResponseRules is the array of possible observations a skill can return.</br>
Each observation in the array has the following fields:</br>
* "Response" is a string field that defines the observation name. The SD `__moduleResponse` variable can only receive values defined as "Response."
* "ConditionCodeWithLocalVariables" is a string field that uses the user to define when the skill returns the current response (code is in Python for ROS). The condition may depend on "Local Variable" values (see [Local Variables](#)).
Example:</br>
```
ModuleResponse": {
        "ResponseRules": [
            {
                "Response": "eSuccess",
                "ConditionCodeWithLocalVariables": "skillSuccess and goal_reached"
            },
            {
                "Response": "eFailed",
                "ConditionCodeWithLocalVariables": "True"
            }
        ]
    },
```
The returned observation is the first "Response" that its condition is met (they are ordered the same as defined).

## ModuleActivation
The ModuleActivation section describes how to activate the skill code.</br>
The activation can use  SD's "GlobalVariableModuleParameters" modified to local variables (see [Local Variables](#localvariablesinitialization)).

### RosService
This section defines how to activate a ROS1 service.</br>
It has the following sections:</br>
* "ImportCode" defines imported modules used when calling the service. 
* "ServicePath" is the service path (called <service name> in the [service ROS Wiki](http://wiki.ros.org/rosservice)   
* "ServiceName" is the name of the service "srv" file (`<ServiceName>.srv`).
* "ServiceParameters" is the array of parameters sent in the service request. </br> 
	Each parameter has a: </br>
  - "ServiceFieldName" which is the name of the parameter as defined in the `<ServiceName>.srv` file. 
  - "AssignServiceFieldCode" is the value of the parameter. The user can define a Python code with local variables (see [Local Variables](#localvariablesinitialization)). 

Example:</br>
```
"ModuleActivation": {
        "RosService": {
            "ImportCode": [
                {
                    "From": "geometry_msgs.msg",
                    "Import": [
                        "Point"
                    ]
                },
                {
                    "From": "simple_navigation_goals.srv",
                    "Import": [
                        "navigate",
                        "navigateResponse"
                    ]
                }
            ],
            "ServicePath": "/navigate_to_point",
            "ServiceName": "navigate",
            "ServiceParameters": [
                {
                    "ServiceFieldName": "goal",
                    "AssignServiceFieldCode": "Point(x= nav_to_x, y= nav_to_y, z= nav_to_z)"
                }
            ]
        }
    },
```

## LocalVariablesInitialization
The "LocalVariablesInitialization" section is used to define local variables.</br>
Local variables can take their value from three possible sources:</br>
* SD file skill parameters (see [GlobalVariableModuleParameters section](#globalvariablemoduleparameters)). Only this type of local variable can be used to activate the skill since the other local variables' value is calculated when the skill execution ends.
  -  "InputLocalVariable" is the name of the local variable.
  -  "FromGlobalVariable" is the name of the skill parameter defined in the SD "GlobalVariableModuleParameters" section.
  
Example:</br>
```
{
	"InputLocalVariable": "nav_to_x",
	"FromGlobalVariable": "oDesiredLocation.x"
}
```
* Skill-code returned value. Using the value returned from the skill code. The user can define a Python function to manipulate the returned value to something more meaningful or convenient. </br>
This local variable definition has the following fields:
  -  "LocalVariableName" is the local variable name.
  -  "VariableType" this optional field is the type of the variable when converted to C++ (used for the "state given observation" feature).
  -  "FromROSServiceResponse" should be `true` when the value is taken from the service response.
  -  "AssignmentCode" is the Python code for assigning the local variable value from the ROS service response (returned value). The reserved word `__input` is used to reference the service returned value. This field value is the string code. Nevertheless, users can define it as an array of strings representing complex Python code (the indentations are preserved).
  -  "ImportCode" is an array of imports needed when receiving the service response.
  
Example:</br>
```
{
    "LocalVariableName": "skillSuccess",
    "VariableType":"bool",
    "FromROSServiceResponse": true,
    "AssignmentCode": "skillSuccess=__input.success",
    "ImportCode": [
	{
	    "From": "std_msgs.msg",
	    "Import": [
		"Bool"
	    ]
	}
    ]
}
```
* Public data published in the robot framework (e.g., ROS topics). 
This type of local variable is constantly updated when certain public information is published in the robot framework(e.g., when a ROS topic message is published). It can capture events that occur during the skill execution. It's last value will be used when the skill observation is calculated. </br>This type of local variable is defined using the following fields:
  -  "LocalVariableName" is the local variable name.
  -  "RosTopicPath" is the topic path.\
  -  "InitialValue" defines the value used to initialize the variable.
  -  "TopicMessageType" is the type of the topic message ([see](http://wiki.ros.org/Topics)).
  -  "VariableType" this optional field is the type of the variable when converted to C++ (used for the "state given observation" feature). 
  -  "AssignmentCode" is the Python code for assigning the local variable value from the ROS service response (returned value). The reserved word `__input` is used to reference the service returned value. This field value is the string code. Nevertheless, users can define it as an array of strings representing complex Python code (the indentations are preserved).
  -  "ImportCode" is an array of imports needed when receiving the service response.

Example:</br>
```
{
            "LocalVariableName": "goal_reached",
            "RosTopicPath": "/rosout",
            "VariableType": "bool",
            "InitialValue": "False",
            "TopicMessageType": "Log",
            "ImportCode": [
                {
                    "From": "rosgraph_msgs.msg",
                    "Import": [
                        "Log"
                    ]
                }
            ], 
            "AssignmentCode":[ 
            "if goal_reached == True:",
            "    return True",
            "else:",
            "    return __input.msg.find('Goal reached') > -1"]
        },
```
## Additional documentation language functionality
### Sample from Discrete distribution
Users can describe sampling from discrete distribution by using the  SampleDiscrete function that takes a vectore of floats as weights.</br>
`int AOSUtils::SampleDiscrete(vector<double> weights)`</br>
`int AOSUtils::SampleDiscrete(vector<float> weights)`</br>
Example:</br>
```
vector<float> weights{0.25,0.25,0.4,0.1};
int sampledVal = AOSUtils::SampleDiscrete(weights)
```
The sampled value will range from 0 to weights.size()-1 ({0,1,2,3} in the example above).</br>

### Sample from Bernoulli distribution
The user can also sample from Bernoulli distribution using the following function:</br>
`bool AOSUtils::Bernoulli(double p)`</br>
```
success = AOSUtils::Bernoulli(0.75)
```
### Iterate over all state variables of a specific type
The user can iterate over all state variables of a specific custom compound
 type. Pointers to all of the state variables of a specific type are stored inside a special autogenerated state variable called '<customTypeName>Objects' .
For example if we have a custom compound
 type called 'tLocation
' with a subfield called 'continuous_location_x
' of type float, a user can iterate over all these state variables to perform a calculation or change one of them.
 Example:
```
bool hasLocationInRange=false;
for(int i=0; i < state.tLocationObjects.size();i++)
{
	if(state.tLocationObjects[i]->continuous_location_x > 1 && state.tLocationObjects[i]->continuous_location_x < 2)
    {
      hasLocationInRange = true;
    }
}
```
