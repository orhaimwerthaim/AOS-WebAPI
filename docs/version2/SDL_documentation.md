
# AOS SDL Documentation (version 2.0)
* [Environment File](#environment-file)
* [Additional documentation language functionality](#additional-documentation-language-functionality)
* [Skill Documentation (SD) file](#skill-documentation-sd-file)

The Skill Documentation Language (SDL) is an Action Description Language (ADL) to describe planning domains for autonomous robots.
It works in the following way. A robotic engineer needs to document the behavior of each robot skill he implemented (e.g., navigation, object detection, arm manipulation, etc.), the environment the robot is working in, and the robot's objective (e.g., cleaning a room).
SDL files are divided into three types: 1) an Environment File (EF) that describes skill-dependent aspects of the domain 2) Skill Documentation (SD) files, each describing skill-dependent aspects of the domain 3) Abstraction Mapping (AM) files, describing the mapping between the planning abstract model described in an SD file to a robot skill code. More specifically, it describes how to activate the code and translate the code execution to information the planning process can reason with.

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
C++ code is usually deterministic, yet SDL provides methods to sample from known distributions.</br>
state variables are reffered to by `state.<state variable name>` e.g., `state.x`. The initial belief can assign the `state.` set of state variables. Their definition code sets their initial value.</br>
```
bool p = 0.8;
bool sample = AOS.Bernoulli(p);
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
state.robotLocation.discrete = AOS.Bernoulli(0.5) ? 1 : (AOS.Bernoulli(0.2) ? 2 : 3);
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
if (AOS.Bernoulli(0.05)) state_.robotLocation.discrete = -1;
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

## Assisting the planner
The engineer can guide the AOS to the direction of desired solutions.</br>

One way is to define preconditions to activate a skill. Since the robot is not always fully aware of its current state, it cannot entirely revoke illegal actions (e.g., try driving when there is no fuel). Nevertheless, the user can define when a skill is illegal and penalize activating it by its relative weight in the current belief state distribution.</br> 

Moreover, MCTS algorithms (supported by the AOS) build a search tree where each node is a distribution over states (belief state), and the leaf nodes are evaluated using a default (rollout) policy. The rollout policy is performed using a single state and is crucial to find a good solution. We can use the preconditions to revoke illegal skills.</br> 

Furthermore, the user can define preferred action to define the rollout policy.</br>

### GlobalVariablePreconditionAssignments
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
The observation must correspond to the observations specified in the AM file. Observations are enumerable values. The AOS runs simulations to decide the next best skill to apply. Next, the selected skill code is executed, and the AOS translates the execution outcome to an observation which is then used to update the distribution on the current state (current belief).

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
