
The Skiil Documentation Language (SDL) is an Action Description Language (ADL) to describe planning domains for autonomous robots.
It works in the following way. A robotic engineer needs to document the behavior of each robot skill he implemented (e.g., navigation, object detection, arm manipulation, etc.), the environment the robot is working in, and the robot's objective (e.g., cleaning a room).
SDL files are divided into three types: 1) an Environment File (EF) that describes skill-dependent aspects of the domain 2) Skill Documentation (SD) files, each describing skill-dependent aspects of the domain 3) Abstraction Mapping (AM) files, describing the mapping between the planning abstract model described in an SD file to a robot skill code. More specifically, it describes how to activate the code and translate the code execution to information the planning process can reason with.

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
