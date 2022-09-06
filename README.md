## Experiments and Videos
### tic-tac-toe experiments
#### Basic Experiment
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

*[tic-tac-toe Video 1: first expriment robot starts, we are letting it win :)](https://www.youtube.com/watch?v=45Vaf3C-Pco)
*[tic-tac-toe Video 2: robot starts and plays as expected](https://www.youtube.com/watch?v=45Vaf3C-Pco)
*[tic-tac-toe Video 3: robot starts](https://www.youtube.com/watch?v=Jlp6ddtmVz8)
*[tic-tac-toe Video 4: human starts](https://www.youtube.com/watch?v=-rqX1sG5m9Y)
*[tic-tac-toe Video 5: human starts](https://www.youtube.com/watch?v=fpwuh_1mInU)
*[tic-tac-toe Video 6: human starts](https://www.youtube.com/watch?v=ZA5Wg_8KkMM)
*[tic-tac-toe Video 7: human starts robot losses because it counts on the random opponent mistake.](https://www.youtube.com/watch?v=R4dBrP7SLe8)

### Probabilistic tic-tac-toe
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
* [Probabilistic tic-tac-toe video 1](https://www.youtube.com/watch?v=wdVrZBAW9k8)
* [Probabilistic tic-tac-toe video 2](https://www.youtube.com/watch?v=gO49ZPzf6eE)
* [Probabilistic tic-tac-toe video 3](https://www.youtube.com/watch?v=ZypCr8SoJr4)

### Changing the rules of the game
Our next experiment considered the case of a new task:
players get a score for marking positions adjacent to corner
squares they marked before. This is a different game played
with the same skills. To play it, the AOS requires minimal
user effort of modifying the reward function. The resulting
behavior, however, is quite different. For this, a
completely new script would have to be written and would
require figuring out good strategies for this game. Classical
planners would not be able to model this objective well.</br> </br>

* [Altered tic-tac-tow video 1](https://www.youtube.com/watch?v=llIDKMD1yCA)
* [Altered tic-tac-tow video 2](https://www.youtube.com/watch?v=HhgTrNxjLzE)
* [Altered tic-tac-tow video 3](https://www.youtube.com/watch?v=XLj1i3W5c2c)

### Initial state unknown

In this experiment, the robot starts with a board that was already played for three moves. We documented the initial belief, such that any legal three moves are possible. 
An autonomous robot must be able to start from different possible states of the environment.
And again, this is not a manually written script for tic-tac-toe. It is a general-purpose algorithm to operate an autonomous robot to maximize its objectives in a partially observable stochastic environment.</br></br>

* [tic-tac-toe with unknown initial state video 1](https://www.youtube.com/watch?v=Q86ZCDcSAGk)
* [tic-tac-toe with unknown initial state video 2](https://www.youtube.com/watch?v=O5zKoOIBL-U)
* [tic-tac-toe with unknown initial state video 1](https://www.youtube.com/watch?v=Q86ZCDcSAGk)
* [tic-tac-toe with unknown initial state video 1](https://www.youtube.com/watch?v=Q86ZCDcSAGk)
* [tic-tac-toe with unknown initial state video 1](https://www.youtube.com/watch?v=Q86ZCDcSAGk)



## AOS Installtion
#### Requirements
* Ubuntu 16.04
#### Install dependencies
* [MongoDB](https://docs.mongodb.com/manual/tutorial/install-mongodb-on-ubuntu/) 
* [Postman](https://www.postman.com/downloads/)
* Load DB (run this command from the AOS.archive directory)</br>
`mongorestore -v --nsFrom "AOS.*" --nsTo "AOS.*" --uri="mongodb://localhost:27017/" --archive="AOS.archive"`

#### Download code:
* Download the AOS Web API </br>
`git clone https://github.com/orhaimwerthaim/AOS-WebAPI` 

* Download the Planning Engine base into your <_planning engine directory_> </br>
`git clone https://github.com/orhaimwerthaim/AOS-Solver`

...
