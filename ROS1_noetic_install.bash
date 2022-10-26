#!/bin/bash
sudo sh -c 'echo "deb http://packages.ros.org/ros/ubuntu $(lsb_release -sc) main" > /etc/apt/sources.list.d/ros-latest.list'
 
# if you haven't already installed curl
sudo apt install curl 
curl -s https://raw.githubusercontent.com/ros/rosdistro/master/ros.asc | sudo apt-key add -


sudo apt update


yes| sudo apt install ros-noetic-desktop-full


echo "source /opt/ros/noetic/setup.bash" >> ~/.bashrc
#echo "export ROS_DISTRO=noetic" >> ~/.bashrc
source ~/.bashrc


yes| sudo apt install python3-rosdep python3-rosinstall python3-rosinstall-generator python3-wstool build-essential


yes| sudo apt install python3-rosdep

sudo rosdep init
rosdep update

mkdir -p ~/catkin_ws/src
cd ~/catkin_ws/
source ~/.bashrc
catkin_make



echo "source ~/catkin_ws/devel/setup.bash" >> ~/.bashrc
source ~/.bashrc

#Install turtleBot3
yes| sudo apt-get install ros-noetic-joy ros-noetic-teleop-twist-joy \
  ros-noetic-teleop-twist-keyboard ros-noetic-laser-proc \
  ros-noetic-rgbd-launch ros-noetic-rosserial-arduino \
  ros-noetic-rosserial-python ros-noetic-rosserial-client \
  ros-noetic-rosserial-msgs ros-noetic-amcl ros-noetic-map-server \
  ros-noetic-move-base ros-noetic-urdf ros-noetic-xacro \
  ros-noetic-compressed-image-transport ros-noetic-rqt* ros-noetic-rviz \
  ros-noetic-gmapping ros-noetic-navigation ros-noetic-interactive-markers
  
  
yes| sudo apt install ros-noetic-dynamixel-sdk
yes| sudo apt install ros-noetic-turtlebot3-msgs
yes| sudo apt install ros-noetic-turtlebot3

#install simulation packages
cd ~/catkin_ws/src/

git clone -b noetic-devel https://github.com/ROBOTIS-GIT/turtlebot3_simulations.git

cd ~/catkin_ws && catkin_make


echo "export TURTLEBOT3_MODEL=waffle" >> ~/.bashrc
#echo "export TURTLEBOT3_MODEL=burger" >> ~/.bashrc
source ~/.bashrc

