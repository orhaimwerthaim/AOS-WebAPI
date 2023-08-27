
The Skill Documentation Language (SDL) is an Action Description Language (ADL) to describe planning domains for autonomous robots.
It works in the following way. A robotic engineer needs to document the behavior of each robot skill he implemented (e.g., navigation, object detection, arm manipulation, etc.), the environment the robot is working in, and the robot's objective (e.g., cleaning a room).
SDL files are divided into three types: 1) an Environment File (EF) that describes skill-dependent aspects of the domain 2) Skill Documentation (SD) files, each describing skill-dependent aspects of the domain 3) Abstraction Mapping (AM) files, describing the mapping between the planning abstract model described in an SD file to a robot skill code. More specifically, it describes how to activate the code and translate the code execution to information the planning process can reason with.

## Environment File (EF)
We now describe how to formally write the files, starting with the EF.
Each file (EF, SD, or AM) is described in different sections. Except for the project section that describes the project name and must appear first, the order of sections is insignificant.
```
project: <project name>
``` 		
e.g., 
```
project: tic_tac_toe
``` 		  
Next, in the EF file, users describe horizon, which means the number of steps the AOS' look-ahead when it selects the next action.
```
horizon: <number of look-ahead steps>
``` 		
e.g., 
```
horizon: 10
```
The discount factor describes how less future rewards are worth.
```
horizon: <decimal discount value>
``` 	
e.g., 
```
discount: 0.99
``` 

Users can define the state variables their environment is composed of. The state variable types can be string, float, doubt, int, double, or string. Moreover, users can define vectors of such types or define custom types as enums or structs.
Enums are define by:
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

State variables are defined as follows (referring them for initialization is by 'state.<var name>'):
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
In some cases, the initial state of the robot is not deterministic. Users can generatively describe the initial state distribution using C++ code.
Conceptually, the initial state distribution is described by using this code to sample an infinite number of states representing the initial state distribution.
C++ code is usually deterministic, yet SDL provides methods to sample from known distributions.
state variables are reffered to by `state.<state variable name>` e.g., `state.x`. The initial belief can assign the `state.` set of state variables. Their definition code sets their initial value.
```
bool p = 0.8;
bool sample = AOS.Bernoulli(p);
```
The variable `sample` receives a sampled value from a Bernoulli distribution with a parameter `p`. The Bernoulli parameter must range from 0.0 to 1.0.

```
vector<float> weights{0.1,0.2,0.6,0.1};
int sample = AOSUtils::SampleDiscrete(weights);
```
The variable `sample` receives a sampled index integer where the weight for each index is described by `weights`.
The `initial_belief:` code can be described in multiple lines to define temporal variables. The initial value of state variables is set by their definition and .
e.g.,
```
initial_belief:
state.robotLocation.discrete = AOS.Bernoulli(0.5) ? 1 : (AOS.Bernoulli(0.2) ? 2 : 3);
```

#### reward_code:
Moreover, users can define state-dependent rewards and terminal states in the `reward_code:` section. Users can use multi-line code and conditionally assign positive rewards for desired states (and vice versa) by assigning the `__reward` variable. Furthermore, users can define one-time rewards received once in a trajectory (by assigning the `__stopEvaluatingState` with true ), unlike other rewards that may describe a state that we want to maintain and to consistently deliver rewards when it is reached. Users can write multiple `reward_code:` sections to describe various one-time rewards.
A terminal state is described by assigning `true` to the boolean variable `__isGoalState`. 
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
Finally, the EF file can describe external changes not invoked by the robot's actions in a multi-line code section.
These changes can be conditioned on the current robot state. e.g., a robot may slip in probability dependent on its current speed. The set of state variables after external changes is accessed by `state_.<variable name>` vs. the state before these changes with `state_.<variable name>`. 
```
extrinsic_code:
<multi-line code>
```
An example that describes a 5% chance for a specific change to occur at each step:
```
extrinsic_code:
if (AOS.Bernoulli(0.05)) state_.robotLocation.discrete = -1;
```
## SDL code syntax
SDL uses C++ code with some small extensions.
1. Sampling from known distributions:
```
bool p = 0.8;
bool sample = AOS.Bernoulli(p);
```
The variable `sample` receives a sampled value from a Bernoulli distribution with a parameter `p`. The Bernoulli parameter must range from 0.0 to 1.0.

```
vector<float> weights{0.1,0.2,0.6,0.1};
int sample = AOSUtils::SampleDiscrete(weights);
```
The variable `sample` receives a sampled index integer where the weight for each index is described by `weights`.

2.
