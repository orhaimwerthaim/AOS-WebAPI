# AOS
##### Links:
* [General](#general)
  - [General](#general)
* [Environment File](#environment-file)
  - [videos](#videos)
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

## The AOS documentation: documentation structure:
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
