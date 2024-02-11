# AOS
##### Links:
* [Experimets](#experiments-and-videos)
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
* [Tutorial](#tutorial)
  

# Experiments and Videos:
##### Videos:
* [Basic tic-tac-toe](#basic-tic-tac-toe-vidoes)
* [Probabilistic tic-tac-toe](#probabilistic-tic-tac-toe-videos)
* [Tic-tac-toe changing the game rules](#tic-tac-toe-changing-the-game-rules-videos)
* [Tic-tac-toe with unknown initial state](#tic-tac-toe-with-unknown-initial-state-videos)
* [Armadillo Gazebo](#armadillo-gazebo-videos)
* [Real Armadillo Robot](#real-armadillo-robot-videos)
* [TurtleBot3](#turtlebot3-videos) 

## Real Panda robot tic-tac-toe experiments:
#### Basic experiment:
skills documentation can be found [at](https://github.com/orhaimwerthaim/AOS-experiments/tree/main/panda_tic_tac_toe/panda_tic_tac_toe_original).</br>
In this experiment, we programmed a Panda CoBot to play tic-tac-toe with a human. 
An Intel RealSense D415 camera was attached to the robot arm, and an erasable board
with a tic-tac-toe grid was placed within its reach. 
The experiment was based on two skills: marking a circle in a specific grid cell, and detecting a change in the board state and
extracting the new board state. 
The first skill was implemented using our own PID controller based on libfranka, which we
wrapped as ROS service. The second skill was adapted from code found on the web. After experimenting with the code to
see its properties, PLP and AM files were specified for each
skill. The AOS allows the specification of PLPs that describe
exogenous events and are executed prior to every agent’s
action. This feature was used to model the human’s action. We
modeled the human as making random legal choices. Finally,
in the environment file we defined the goal reward, the initial
state of an empty board and the starting player. From this point,
it was plug’n play, requiring no additional effort. The AOS
auto-generated the code, and we run the game (changing the
starting player, as desired). Because the human was modeled as
a random player, you can observe in the videos that the robot
sometimes ”counts” on a human mistake of not completing a
sequence of three.</br> </br>

##### Basic tic-tac-toe vidoes:
* [tic-tac-toe Video 1: first expriment robot starts, we are letting it win :)](https://www.youtube.com/watch?v=45Vaf3C-Pco)
* [tic-tac-toe Video 2: robot starts and plays as expected](https://www.youtube.com/watch?v=45Vaf3C-Pco)
* [tic-tac-toe Video 3: robot starts](https://www.youtube.com/watch?v=Jlp6ddtmVz8)
* [tic-tac-toe Video 4: human starts](https://www.youtube.com/watch?v=-rqX1sG5m9Y)
* [tic-tac-toe Video 5: human starts](https://www.youtube.com/watch?v=fpwuh_1mInU)
* [tic-tac-toe Video 6: human starts](https://www.youtube.com/watch?v=ZA5Wg_8KkMM)
* [tic-tac-toe Video 7: human starts robot losses because it counts on the random opponent mistake.](https://www.youtube.com/watch?v=R4dBrP7SLe8)

#### Probabilistic tic-tac-toe experiments:
skills documentation can be found [at](https://github.com/orhaimwerthaim/AOS-experiments/tree/main/panda_tic_tac_toe/panda_tic_tac_toe_prob).</br>
We tested the AOS’s ability to adapt to changes in
the robot, environment, and task. First, we changed the circle
drawing skill to emulate an arm with difficulty drawing in
the center square, succeeding only in half the cases. The
turn changes following the robot’s attempt regardless of the
outcome. The only effort required on our part was to trivially
modify the draw circle skill’s PLP to reflect its modified
behavior. The POMDP-solver now optimizes given this up-
dated PLP, and you can observe in the videos that the robot
prefers drawing a circle in other positions when possible, only
selecting the center square when it is crucial. Imagine the effort
of changing a script to adapt to this capability change. Notice
that a classical solver cannot even model it.</br>
</br>
##### Probabilistic tic-tac-toe videos: 
* [Probabilistic tic-tac-toe video 1](https://www.youtube.com/watch?v=wdVrZBAW9k8)
* [Probabilistic tic-tac-toe video 2](https://www.youtube.com/watch?v=gO49ZPzf6eE)
* [Probabilistic tic-tac-toe video 3](https://www.youtube.com/watch?v=ZypCr8SoJr4)

#### Changing the rules of the game experiments:
skills documentation can be found [at](https://github.com/orhaimwerthaim/AOS-experiments/tree/main/panda_tic_tac_toe/panda_tic_tac_toe_chaged_game).</br>
Our next experiment considered the case of a new task:
players get a score for marking positions adjacent to corner
squares they marked before. This is a different game played
with the same skills. To play it, the AOS requires minimal
user effort of modifying the reward function. The resulting
behavior, however, is quite different. For this, a
completely new script would have to be written and would
require figuring out good strategies for this game. Classical
planners would not be able to model this objective well.</br> </br>

##### tic-tac-toe changing the game rules videos: 
* [Altered tic-tac-tow video 1](https://www.youtube.com/watch?v=llIDKMD1yCA)
* [Altered tic-tac-tow video 2](https://www.youtube.com/watch?v=HhgTrNxjLzE)
* [Altered tic-tac-tow video 3](https://www.youtube.com/watch?v=XLj1i3W5c2c)

#### Unknown Initial state experiments:  
skills documentation can be found [at](https://github.com/orhaimwerthaim/AOS-experiments/tree/main/panda_tic_tac_toe/panda_tic_tac_toe_initialStateUnknown).</br>
In this experiment, the robot starts with a board that was already played for three moves. We documented the initial belief, such that any legal three moves are possible. 
An autonomous robot must be able to start from different possible states of the environment.
And again, this is not a manually written script for tic-tac-toe. It is a general-purpose algorithm to operate an autonomous robot to maximize its objectives in a partially observable stochastic environment.</br></br>

##### tic-tac-toe with unknown initial state videos:
* [tic-tac-toe with unknown initial state video 1](https://www.youtube.com/watch?v=Q86ZCDcSAGk)
* [tic-tac-toe with unknown initial state video 2](https://www.youtube.com/watch?v=O5zKoOIBL-U)
* [tic-tac-toe with unknown initial state video 1](https://www.youtube.com/watch?v=Q86ZCDcSAGk)
* [tic-tac-toe with unknown initial state video 1](https://www.youtube.com/watch?v=Q86ZCDcSAGk)
* [tic-tac-toe with unknown initial state video 1](https://www.youtube.com/watch?v=Q86ZCDcSAGk)

## Armadillo Gazebo experiments: 
A detailed description of how we built this experiment and the  documentation used can be found [at](https://github.com/orhaimwerthaim/AOS-experiments/tree/main/armadillo_pick).</br>
The experiment simulation environment included a room with two
tables, and a corridor with a person. Each table had a can
on it. One of the cans was very difficult to pick (its true size
was 10% of the size perceived by the robot). The robot was
located near the table with the difficult can. The goal was to
give the can to a person in the corridor. We implemented three
skills: pick-can, navigate to a person or a table, and serve-
can which handed the can to the person. For the experiments,
we used two versions of the pick PLP: a ”rough” model that
assumes that the probability of successful pick is independent
of the selected table, and a ”finer” model in which the success
probability is conditioned on the robot’s position.</br>

First, we experimented with each skill and used observed
statistics of their behavior to write their PLP files. Then, we
specified the AM files and the task specification. As above,
this information was enough to enable the AOS to control the
robot through the task. During plan execution, we observed
that, occasionally, the pick skill ends with the arm outstretched.
Attempting to serve the person in this state causes a collision
(i.e., injured the person). Moreover, pick returned success if
motion planning and motion execution succeeded, but this did
not imply that the can was successfully picked. Therefore,
we wrote two new skills: detect-hold-can and detect-arm-
stretched. This was almost immediate for two reasons: these
sensing skills are easy to define using the AM files because
they simply map low-level data published by the robot (gripper
pressure, arm-joint angles) into the abstract variables used
by the PLPs. We also implemented an alternative pick skill
with integrated success sensing. Its return value reflected the
outcome of sensing whether the can is held. This, too, is
very easy to do through the output specification in the PLP.
Both changes involve adding two lines to the respective file.
Detect-hold-can is noisy and was modeled as such. Detect-
arm-stretched is not noisy.</br>

First, with the rough model, the robot (correctly) tries to
pick the problematic can because it saves the cost of navigating
to the other table, while with the finer mode, it first moves
to the other table where pick is more likely to succeed.
Second, without sensing actions, the robot serves the can,
but then, because it has no feedback, goes back to the tables
and tries to repeat the process, while with sensing, the robot
verifies success, if the result is yes, only then does it serve
the can and stop. Moreover, since sensing is noisy, the robot
performs multiple sense actions to achieve a belief state with
less uncertainty because the results of the sensing actions are
modeled as independent. However, when sensing is integrated
into the pick action, it cannot do independent sensing, and
repeating the pick action is not desirable.</br> </br>

##### Armadillo Gazebo videos:</br>
* [An experiment 7x video](https://youtu.be/10sTQ8a_N6c) 
* [AOS pick experiment with observe skills](https://www.youtube.com/watch?v=_1iaG1N6nmI)
* [Armadillo Gazebo, finer model, enhanced pick: video 1](https://youtu.be/9zy52vlDZOs)
* [Armadillo Gazebo, finer model, with all possible skills video 1](https://youtu.be/89PtHg0LpkI)
* [Armadillo Gazebo, finer model, with all possible skills video 2](https://youtu.be/c2UTeYKGSV4)
* [Armadillo Gazebo, finer model, with all possible skills video 3: nice](https://youtu.be/wPfAhS4Er7o)
* [Armadillo Gazebo, finer model, without observation video 1](https://youtu.be/B9e8b01Mm7Y)
* [Armadillo Gazebo, finer model, without observation video 2](https://youtu.be/TwL33YrbNz8)
* [Armadillo Gazebo, finer model, without observation video 3](https://youtu.be/bAQ-WKGmTlg)
* [Armadillo Gazebo, raw model, enhanced pick video 1](https://youtu.be/Fn4cvCr94OI)
* [Armadillo Gazebo, raw model, enhanced pick video 2: best](https://youtu.be/vpYgekGjoN0)
* [Armadillo Gazebo, raw model, enhanced pick video 3](https://youtu.be/AzBEtJbo2gY)
* [Armadillo Gazebo, raw model, enhanced pick video 4](https://youtu.be/_6GMCN3R5Us)
* [Armadillo Gazebo, raw model, with all possible skills video 1](https://youtu.be/EfJByOeWO_Q)
* [Armadillo Gazebo, raw model, with all possible skills video 2](https://youtu.be/wxVztzj9bzM)
* [Armadillo Gazebo, raw model, with all possible skills video 3](https://youtu.be/qYvAjKNVbpY)
* [Armadillo Gazebo, raw model, with all possible skills video 4](https://youtu.be/kh_7-Jr0dk4)
* [Armadillo Gazebo, raw model, with all possible skills video 5](https://youtu.be/DW5REAoKT80)
* [Armadillo Gazebo, raw model, with all possible skills video 6](https://youtu.be/mL_FgOCRN2E)
* [Armadillo Gazebo, raw model, with all possible skills video 7](https://youtu.be/RR4An1TQ8p0)
* [Armadillo Gazebo, raw model, without observation video 1](https://youtu.be/95bEE6jnJlw)
* [Armadillo Gazebo, raw model, without observation video 2](https://youtu.be/z6BrZroydBI)
* [Armadillo Gazebo, raw model, without observation video 3: best](https://youtu.be/4_g7Hcy5Ub4)

## Real Armadillo Robot experiments:
skills documentation can be found [at](https://github.com/orhaimwerthaim/AOS-experiments/tree/main/AdditionalExamples/Real%20Armadillo-%20Clean%20Lab).</br>
Next, we tested the integration of AOS with the real
Armadillo in a simple, clean-lab task. In our lab, there are six
workstations and two trash cans. There are two empty cups
somewhere in stations 1,2, or 3 and two in stations 4,5 or 6.
The robot can navigate to each of the stations and trash cans.
It can observe if an empty cup is placed on a nearby station;
it can pick a nearby cup and throw one into a nearby trash
can. The robot should collect all the cups and place them in
the trash cans as fast as possible. Navigation cost is relative
to the distance to the destination, so the robot would find the
shortest path to perform its task. We implemented each of these
skills, which are deterministic, and mapped our lab using ROS’
gmapping package [] so we can use ROS’s navigation stack.
We documented the skills, environment, and robot objective
and activated the robot that utilized its skills to clean our lab
as expected. As above, the only integration effort needed was
preparing the documentation files.</br>

###### Real Armadillo Robot videos:
* [Real Armadillo brings a cup experiment (speed x6) with two video angles at the end](https://youtu.be/CU-eUKAByPs)
* [Real Armadillo brings a cup experiment video 1](https://youtu.be/8NzD2Ea1Z2c)
* [Real Armadillo brings a cup experiment video 2](https://youtu.be/rbfNdPcF8so)
* [Real Armadillo brings a cup experiment video 2](https://youtu.be/JtQvxIHDFxo)

### turtleBot3 Gazebo experiments:
skills documentation can be found [at](https://github.com/orhaimwerthaim/AOS-experiments/tree/main/AdditionalExamples/turtleBotVisitLocations).</br>
This is a first integration experiment.</br>
The [video](https://youtu.be/fx6CXGMWWEM) shows:</br>
Starting the AOS.</br>
We are sending an HTTP request to integrate and operate the robot by the PLPs.
The robot simulation.</br>
Sending another HTTP request to see a) the sequence of actions sent by the solver with their details and response given by the code modules b) the belief state as maintained by the AOS during the process (we defined to see only one particle of the belief state but this is configurable).
This video demonstrates how the AOS finds the shortest path for the robot to visit seven critical points on the map. The user wrote a PLP describing his robot navigation skill and an environment file describing his goal: to visit all points while traveling the shortest distance. 
In this simple example, the initial state is known, and the outcomes of the actions are deterministic (navigation always succeeds). The solver planning time was set to 0.1 seconds per action.
</br> 
The AOS is automatically integrating the user code. The user only needs to send a request to the AOS Restful API (HTTP request), and the integration and execution are performed automatically.</br> </br>

##### turtleBot3 videos:</br>
* [video 1: AOS and turtleBot3 find the shortest path to visit each point twice](https://youtu.be/9Fyund5sjcU)
* [video 2: AOS and turtleBot3 integration in a simple shortest path navigation mission](https://youtu.be/2WQOsS4EikM)

## AOS Installtion
#### Requirements
* Ubuntu 20.04
#### Installtion Steps
* run bash script ["AOS_ubuntu20_install_v7.bash"](https://github.com/orhaimwerthaim/AOS-WebAPI/blob/master/AOS_ubuntu20_install_v7.bash) 
* (optional) run bash script ["ROS1_noetic_install.bash"](https://github.com/orhaimwerthaim/AOS-WebAPI/blob/master/ROS1_noetic_install.bash)

## AOS-ML-Server
* see [AOS-ML-Server github](https://github.com/orhaimwerthaim/AOS-ML-Server/tree/main)

## AOS GUI
The GUI allows users:
* To create new project documentation or edit existing ones (it only supports editing and creation of the JSON format)
* Debug and visualize their model correctness, robot execution, and the progress of the belief state during execution.
* [AOS-GUI github](https://github.com/orhaimwerthaim/AOS-GUI)
* [AOS-GUI project page](https://lankrys.wixsite.com/aosproject)
* [AOS-GUI video](https://www.youtube.com/watch?v=wsmd1NaOhPk)(less then three minutes)

# Tutorial:
* [AOS Tutorial: Introduction and Concepts-Hebrew(18:43 minutes)](https://youtu.be/5piEy69uNMQ)
* [AOS Tutorial: Documentation Example-Hebrew(57:50 minutes)](https://youtu.be/39bA1JRR-6g)
* [AOS Tutorial: Installing the AOS (5:51 minutes)](https://youtu.be/LtvghBdEWNg)
* [AOS Tutorial: Verifying the AOS installation(7:05 minutes)](https://youtu.be/Zm-KTZV180g)
* [AOS Tutorial: Verifying the model correctness using the GUI ((12:15 minutes))](https://youtu.be/wyLWg-b7Rww)
* [AOS Tutorial: Debug your documentation model using VS code (15:41 minutes)](https://youtu.be/FE91GuK-O4A)
* [AOS Tutorial: Highlight saved SDL words in my text editor (2:58 minutes)](https://youtu.be/1kv68lNGiP8)
