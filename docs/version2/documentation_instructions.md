# AOS Documentation (version 1.2)
* [General](#general)
  - [General](#general)
  - [The AOS documentation structure](#the-aos-documentation-structure)
* [Environment File](#environment-file)
  - [PlpMain section](#plpmain)
  - [EnvironmentGeneral section](#environmentgeneral)
  - [Real Panda robot tic-tac-toe](#real-panda-robot-tic-tac-toe-experiments)
    - [Basic](#basic-experiments)
    - [Probabilistic](#probabilistic-tic-tac-toe-experiments)
    - [Changing the rules of the game](#changing-the-rules-of-the-game-experiments)
    - [Unknown Initial state](#unknown-initial-state)

  - [Armadillo Gazebo](#armadillo-gazebo-experiments) 
  - [Real Armadillo Robot](#real-armadillo-robot-experiments)
  - [TurtleBot3 Gazebo](#turtlebot3-gazebo-experiments)
  - [RDDLSim vs. AOS Sampling rates (link to external git)](https://github.com/orhaimwerthaim/AOS-experiments/tree/main/AdditionalExamples/compareRDDLSimToAOS)
* [installation](#aos-installtion)  

# General
## The AOS documentation: General
The AOS is designed to assist engineers in integrating AI capabilities within their robots. The engineers should only document their basic skills (e.g., navigation, object detection, etc.) and robot objectives, and the AOS will use this documentation to operate the robot using automated decision-making techniques (online planning algorithms or deep reinforcement learning). It will reason when and how to activate each skill and invoke the skill code.</br>

## The AOS documentation structure:
For each robot project, the user should document a single environment file, and for each skill, a Skill Documentation (SD) file and an Abstraction Mapping (AM) file (all files are in JSON format).</br>

# The AOS  
# Environment File
The environment file is used to specify documentation sections that are not skill-specific. The user can define the state variables, initial belief state of the robot, extrinsic events, and general robot objectives (special states: desired, undesired, or goals).</br>


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
The supported state variable types are the primitive C++ types (e.g., string, int, float). The "GlobalVariableTypes" section enables users to define custom state variable types. More specifically, enums and complex data structures that aggregate multiple data items.</br>
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
