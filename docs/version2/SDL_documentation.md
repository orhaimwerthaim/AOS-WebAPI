
# AOS SDL Documentation (version 2.0)
* [The three sets of state variables](#the-three-sets-of-state-variables)
* [Environment File](#environment-file)
* [Skill Documentation (SD) file](#skill-documentation-sd-file)
* [Abstraction Mapping (AM) file](#abstraction-mapping-am-file)
* [Additional documentation language functionality](#additional-documentation-language-functionality)
  
The Skill Documentation Language (SDL) is an Action Description Language (ADL) to describe planning domains for autonomous robots.
It works in the following way. A robotic engineer needs to document the behavior of each robot skill he implemented (e.g., navigation, object detection, arm manipulation, etc.), the environment the robot is working in, and the robot's objective (e.g., cleaning a room).
SDL files are divided into three types: 1) an Environment File (EF) that describes skill-dependent aspects of the domain 2) Skill Documentation (SD) files, each describing skill-dependent aspects of the domain 3) Abstraction Mapping (AM) files, describing the mapping between the planning abstract model described in an SD file to a robot skill code. More specifically, it describes how to activate the code and translate the code execution to information the planning process can reason with.

# The three sets of state variables
The documentation describes how the state changes in each epoch conditioned on the selected skill. The state can change by either the robot's skills or extrinsic events. To assist the user in documenting complex behaviors flexibly, we introduce three sets of state variables, `state`, `state_`, and `state__`, each containing all of the defined state variables. </br>

`state` stores the previous state values and cannot be changed (except in state variable definition and in the initial belief), `state_` is a copy of `state`, yet it can change by extrinsic events. Finally, `state__` is a copy of `state_` after the extrinsic events occur, and it can change by skill effects. 'extrinsic_code:' can only change variables in `state_` (can be conditioned on `state`), and skill's 'dynamic_model:' can only change `state__` (can be conditioned on `state` and `state_`).
If we defined a state variable named `robotLocation`, it has three copies that can be referred to by either `state.robotLocation`, `state_.robotLocation`, or `state__.robotLocation`.

# Environment File
We now describe how to formally write the files, starting with the Environment File (EF).</br>
The file name should be `<project name>.ef`.</br>
Each file (EF, SD, or AM) is described in different sections. Except for the project section that describes the project name and must appear first, the order of sections is insignificant.</br>
```
project: <project name>
``` 		
e.g., 
```
project: tic_tac_toe
``` 		  
Next, in the EF file, users describe horizon, which means the number of steps the AOS' look-ahead when it selects the next action.</br>
```
horizon: <number of look-ahead steps>
``` 		
e.g., 
```
horizon: 10
```
The discount factor describes how less future rewards are worth.</br>
```
horizon: <decimal discount value>
``` 	
e.g., 
```
discount: 0.99
``` 

Users can define the state variables their environment is composed of.</br> 
The state variable types can be string, float, doubt, int, double, or string. Moreover, users can define vectors of such types or define custom types as enums or structs.</br>
Enums are define by:</br>
```
define_type:  <type name>
enum_members: <enum values with a comma delimiter>
```
e.g., 
```
define_type:  tSymbols
enum_members: eEmpty,eO,eX
```

Struct types are defined:
```
define_type: <struct name>
variable: <field type> <field name> <optional default value>
variable:...
```
e.g.,
```
define_type: tLocation
variable: double x 0.0
variable: double y 0.0
variable: double z 0.0
variable: int discrete -1
```

State variables are defined as follows (referring them for initialization is by 'state.<var name>'):</br>
```
state_variable: <type> <name>
code:
<initializiation C++ code in one line>
```
e.g., 
```
state_variable: tLocation robotLocation
code:
state.robotLocation.discrete = 1;state.robotLocation.x = 1.0;
```

The definition of vector state variables is as follows:
```
state_variable: <primitive type> <name> []
code:
<initializiation C++ code in one line>
```
e.g.,
```
state_variable: int grid []
code:
state.grid={2,222,3};
```

#### initial_belief:
In some cases, the initial state of the robot is not deterministic. Users can generatively describe the initial state distribution using C++ code.</br>
Conceptually, the initial state distribution is described by using this code to sample an infinite number of states representing the initial state distribution.</br>
C++ code is usually deterministic, yet SDL provides methods to sample from Discrete or Bernoulli distributions.</br>
state variables are reffered to by `state.<state variable name>` e.g., `state.x`. The initial belief can assign the `state.` set of state variables. Their definition code sets their initial value.</br>
```
bool p = 0.8;
bool sample = AOSUtils::Bernoulli(p);
```
The variable `sample` receives a sampled value from a Bernoulli distribution with a parameter `p`.</br>
The Bernoulli parameter must range from 0.0 to 1.0.</br>

```
vector<float> weights{0.1,0.2,0.6,0.1};
int sample = AOSUtils::SampleDiscrete(weights);
```
The variable `sample` receives a sampled index integer where the weight for each index is described by `weights`.</br>
The `initial_belief:` code can be described in multiple lines to define temporal variables. The initial value of state variables is set by their definition.</br>
e.g.,
```
initial_belief:
state.robotLocation.discrete = AOSUtils::Bernoulli(0.5) ? 1 : (AOSUtils::Bernoulli(0.2) ? 2 : 3);
```

#### reward_code:
Moreover, users can define state-dependent rewards and terminal states in the `reward_code:` section. </br>
Users can use multi-line code and conditionally assign positive rewards for desired states (and vice versa) by assigning the `__reward` variable. Furthermore, users can define one-time rewards received once in a trajectory (by assigning the `__stopEvaluatingState` with true ), unlike other rewards that may describe a state that we want to maintain and to consistently deliver rewards when it is reached.</br>
Users can write multiple `reward_code:` sections to describe various one-time rewards.</br>
A terminal state is described by assigning `true` to the boolean variable `__isGoalState`. </br>
e.g.,
```
reward_code:
if (!state.v1.visited && state.v2.visited)
{
__reward=-50;
__stopEvaluatingState =true;
}
reward_code:
if (state.v1.visited && state.v2.visited && state.v3.visited)
{
__reward =7000;
__isGoalState =true;
}
```

#### extrinsic_code:
Finally, the EF file can describe external changes not invoked by the robot's actions in a multi-line code section.</br>
These changes can be conditioned on the current robot state. e.g., a robot may slip in probability dependent on its current speed. </br>
The set of state variables after external changes is accessed by `state_.<variable name>` vs. the state before these changes with `state_.<variable name>`. </br>
```
extrinsic_code:
<multi-line code>
```
An example that describes a 5% chance for a specific change to occur at each step:
```
extrinsic_code:
if (AOSUtils::Bernoulli(0.05)) state_.robotLocation.discrete = -1;
```
# Skills documentation
Each skill is documented using two files. First, a Skill Documentation (SD) file to describe the high level of the skill. Second, an Abstraction Mapping (AM) file to describe how to activate the skill code and how to translate the outcome of the skill execution to something the AOS can reason with for decision-making. 

# Skill Documentation (SD) file
#### parameter:
This section uses to define the parameters sent to a skill. The skill parameters are the context under which the skill operates. Possible parameters are, for example, 1) an enum value for navigation modes like 'fast' navigation vs. 'safe' navigation or 2) the navigation destination.

```
parameter: <parameter type> <name>
... 
```
For example:
```
parameter: int oCellP
parameter: string type 
```
#### available_parameters_code:
Each SD file is an action template. The possible actions the planning engine considers are grounded actions in which each parameter has a specified value.
The `available_parameters_code:` section defines the possible grounded actions using code. Users add items to the `__possibleParameters` tuple vector. Each tuple maps to a single grounded action whose parameter values are the tuple members that correspond to the parameter definition ordered in the SD file.
 
Example:</br>
```
available_parameters_code:
std::set<std::string> setOfStr = { "fast","slow"};
for (auto elem : setOfStr)
{
	for(int i=0;i<9;i++)
	{
	  __possibleParameters.push_back(std::make_tuple(i,elem));
	}
}
```
The example above defines 18 grounded actions for navigating fast or slow to each of the nine locations.</br>
 
### precondition:
This section defines a skill' precondition given the grounded skill parameters and a known set of state variables.
The multi-line code should assign a value to the system variable `__meetPrecondition` (default is true).
It can use any state variable from `state`.</br>

Example:</br>
```
precondition:
__meetPrecondition = false;
bool holding = false;
bool typeMatch = false;
for(int i=0; i < state.tBlockObjects.size(); i++){
    if(state.robotArm == state.tBlockObjects[i]->location){ holding = true; break;}
    if(state.robotLocation == state.tBlockObjects[i]->location && state.tBlockObjects[i]->type == toyType) typeMatch=true;
}
__meetPrecondition = !holding && typeMatch && state.robotLocation != state.child;
violate_penalty: -10 
```
The `violate_penalty:` is a decimal field that allows engineers to tune how undesirable it is to activate a particular skill when its preconditions are not met.</br>

#### dynamic_model:
The `dynamic_model:` defines the high-level behavior of a skill. More specifically, the transition reward and observation models, how a skill changes the state, what costs (or rewards) are applied, and which observations are returned. It assigns the next state by setting the state variables in `state__`,  reward (`__reward` system variable), and observation (`__moduleResponse` system variable) conditioned on the previous state (`state`), the state after extrinsic changes (`state_`) and if the preconditions were met (`__meetPrecondition`). 

Example:</br>
```{r, attr.source='.numberLines'}
dynamic_model: 
vector<float> chances{0.5, 0.7, 0.8, 0.85, 0.95, 0.5, 0.7, 0.8, 0.85};
bool success = AOSUtils::Bernoulli(chances[oCellP]);
if(state.isRobotTurn)
{
	state__.grid[oCellP] = state__.grid[oCellP] == eEmpty && success ? eO : state__.grid[oCellP];
	state__.isRobotTurn=!state.isRobotTurn;
}
__moduleResponse= success ? res_success : res_failed;
__reward = 0;
```

### SD observations must correspond to the AM observations
The observation must correspond to the observations specified in the AM file.</br> 
Observations are enumerable values when using the `__moduleResponse` system variable or strings when using `__moduleResponseStr`.</br> 
The AOS runs simulations to decide the next best skill to apply. Next, the selected skill code is executed, and the AOS translates the execution outcome to an observation which is then used to update the distribution on the current state (current belief).</br>

# Abstraction Mapping (AM) file

#### response:
`response:` section defines the translation between an actual execution outcome of a skill, to observations the AOS planning engine can reason about. The planning engine uses the SD documentation to simulate what might happen. The AM `response:` section is used to translate what really happened to the language used in the SD documentation.</br>
The planning engine uses this information to update the robot's belief.</br>

The `response:` describe a single possible observations a skill can return.</br>
The response order is importent.</br>
Each observation in the array has the following fields:</br>
* `response:` defines the observation name. The SD `__moduleResponse` variable can only receive values defined as `response:`.
* response_rule: defines when the skill returns the current response (code is in Python for ROS). The condition may depend on "Local Variable" values.
Example:</br>
```
response: eSuccess
response_rule: skillSuccess and goal_reached
response: eFailed
response_rule: True
```
The returned observation is the first `response:` that its `response_rule:` is met (they are ordered the same as defined).</br>

Another not overlapping option is to define the observation as the value of a string local variable calculated during the skill execution.
```
response_local_variable: <local variable name>
``` 
Example:</br>
```
response_local_variable: obsr
local_variable: obsr
type: string
from_ros_reservice_response: true  
code: 
obsr = __input.state
```
In the example above, we define the `obsr` local variable and designate it as the observation using the`
response_local_variable: obsr`.


#### module_activation:
The `module_activation:` section describes how to activate the skill code.</br>
The activation can use  SD's "parameters" copied to local variables.</br>
Example:
```
module_activation: ros_service
imports: from: panda_demo.srv import: *
path: /mark_tic_tac_toe_cell_service
srv: mark_cell
parameter: cell
code:
str(cell_to_mark)
```
In the example above, the skill is a ROS service whose path is `/mark_tic_tac_toe_cell_service`, the service srv file name is `mark_cell`, the service has a single parameter named `cell` whose value is initialized using the code `str(cell_to_mark)`. </br> `cell_to_mark` is a local variable we use its value.</br>
We are importing the service objects.

#### local_variable:
The `local_variable:` section is used to define local variables.</br>
Local variables can take their value from three possible sources:</br>
* SD file skill parameters. Only this type of local variable can be used to activate the skill since the other local variables' value is calculated when the skill execution ends.
  -  `local_variable: <local variable name>` is the name of the local variable.
  -  `action_parameter: <name>` sets the name of the skill parameter defined in the SD `parameter:` section.
  
Example:</br>
```
local_variable: cell_to_mark
action_parameter: oCellP
```
* Skill-code returned value. Using the value returned from the skill code. The user can define a Python function to manipulate the returned value to something more meaningful or convenient. </br>
This local variable definition has the following fields:
  -  `local_variable: <name>` is the local variable name.
  -  `type: <type>` this optional field is the type of the variable when converted to C++ (used for the "extrinsic actions" feature).
  -  `from_ros_reservice_response: true` declares that the value is taken from the service response.
  -  ```code:  \n <user code> ```
is the Python code for assigning the local variable value from the ROS service response (returned value). The reserved word `__input` is used to reference the service returned value. This field value is   -  `imports: from: <package> import: <import objects>` imports needed objects for receiving the service response (multiple import sections are allowed).
  
Example:</br>
```
local_variable: obsr
type: string
from_ros_reservice_response: true  
code: 
obsr = __input.state
```
* Public data published in the robot framework (e.g., ROS topics). 
This type of local variable is constantly updated when certain public information is published in the robot framework(e.g., when a ROS topic message is published). It can capture events that occur during the skill execution. It's last value will be used when the skill observation is calculated. </br>This type of local variable is defined using the following fields:
  -  `local_variable: <name>` is the local variable name.
  -  `topic: <topic path>` is the topic path.\
  -  `initial_value: <initial value>` defines the value used to initialize the variable.
  -  `message_type: <topic message type>` is the type of the topic message ([see](http://wiki.ros.org/Topics)).
  -  `type: <c++ type>` this optional field is the type of the variable when converted to C++ (used for the "extrinsic actions" feature). 
  -
  -  ```code: \n <user code> `` is the Python code for assigning the local variable value from the ROS service response (returned value). The reserved word `__input` is used to reference the service returned value. This field value is the string code. Nevertheless, users can define it as an array of strings representing complex Python code (the indentations are preserved).
  -  `imports: from: <package> import: <import objects>` imports needed objects for receiving the topic message (multiple import sections are allowed).

Example:</br>
```
local_variable: goal_reached
topic: /rosout
message_type: Log
imports: from: rosgraph_msgs.msg import: Log
type: bool 
initial_value: False
code:
if goal_reached == True:
    return True
```

## Additional documentation language functionality
The AOS currently only supports Discrete and Bernoulli distributions.
### Sample from Discrete distribution
Users can describe sampling from discrete distribution by using the  SampleDiscrete function that takes a vectore of floats as weights.</br>
`int AOSUtils::SampleDiscrete(vector<double> weights)`</br>
`int AOSUtils::SampleDiscrete(vector<float> weights)`</br>
The user must specifically declare the weight vector as vector<float> or vectordouble> so it won't be ambiguous.</br>
Example:</br>
```
vector<float> weights{0.25,0.25,0.4,0.1};
int sampledVal = AOSUtils::SampleDiscrete(weights)
```
The sampled value will range from 0 to weights.size()-1 ({0,1,2,3} in the example above).</br>
The total of the weights vector must be 1.0</br>

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
