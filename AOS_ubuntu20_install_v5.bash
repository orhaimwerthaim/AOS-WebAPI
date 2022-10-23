#!/bin/bash
#https://matt.might.net/articles/bash-by-example/
#install VSCode from https://code.visualstudio.com/docs/?dv=linux64_deb

#1. Open VS Code.
#2.Select the Extensions view icon on the Activity bar or use the keyboard shortcut (Ctrl+Shift+X).
#3. Search for 'C++'.
#4. Select Install.
#5. Search for 'cmake'.
#6. Select Install.
#7. Search for 'cmake tools'.
#8. Select Install.


echo 'update and upgrade using:'
sudo apt update && sudo apt upgrade -y

echo 'install c++'
yes| sudo apt install build-essential

echo 'install python3-dev'
yes| sudo apt-get install -y python3-dev



echo 'install cmake'
yes| sudo apt install cmake -y

#echo 'Install CMake'
#yes| sudo apt install cmake

echo 'Installing git:'
yes| sudo apt-get install git


echo 'install pip3'
yes| sudo apt install python3-pip

echo 'pybind11'
pip install pybind11


echo 'Install the AOS'
cd ~
mkdir AOS
cd ~/AOS


 

echo 'AI Solver: Downloading code'
git clone https://github.com/orhaimwerthaim/AOS-Solver.git
echo 'AI Solver: Downloaded'
echo 'Web API: Downloading code'
git clone https://github.com/orhaimwerthaim/AOS-WebAPI.git
echo 'Web API: Downloaded'

echo 'AOS-experiments: Downloading code'
git clone https://github.com/orhaimwerthaim/AOS-experiments.git
echo 'AOS-experiments: Downloaded'


echo 'Install MongoDB and C++ driver'
cd ~
mkdir mongoDB
cd ~/mongoDB
echo 'install libmongoc'
yes| sudo apt-get install libmongoc-1.0-0
echo 'Install libbson with a Package Manager'
#maybe already installed by other steps
#yes| sudo apt-get install libbson-1.0-0

yes| sudo apt-get install libssl-dev libsasl2-dev

#echo 'Preparing a build from a release tarball'
wget https://github.com/mongodb/mongo-c-driver/releases/download/1.23.1/mongo-c-driver-1.23.1.tar.gz
tar xzf mongo-c-driver-1.23.1.tar.gz
cd mongo-c-driver-1.23.1
mkdir cmake-build
cd cmake-build
 

cmake -DENABLE_AUTOMATIC_INIT_AND_CLEANUP=OFF ..
#cmake -DENABLE_AUTOMATIC_INIT_AND_CLEANUP=OFF -DENABLE_MONGOC=OFF ..

#each 'Preparing a build from a git repository clone'
#git clone https://github.com/mongodb/mongo-c-driver.git
#cd mongo-c-driver
#git checkout 1.23.0  # To build a particular release
#python3 build/calc_release_version.py > VERSION_CURRENT
#mkdir cmake-build
#cd cmake-build
#cmake -DENABLE_AUTOMATIC_INIT_AND_CLEANUP=OFF ..


#cmake -DENABLE_AUTOMATIC_INIT_AND_CLEANUP=OFF -DENABLE_MONGOC=OFF ..
echo 'Building libmongoc and libbson'
cmake --build .
sudo cmake --build . --target install


echo 'install curl'
yes| sudo apt install curl

echo 'Step 3: Download the latest version of the mongocxx driver.'
curl -OL https://github.com/mongodb/mongo-cxx-driver/releases/download/r3.7.0/mongo-cxx-driver-r3.7.0.tar.gz
tar -xzf mongo-cxx-driver-r3.7.0.tar.gz
cd mongo-cxx-driver-r3.7.0/build

echo 'Step 4: Configure the driver'
cmake ..                                \
    -DCMAKE_BUILD_TYPE=Release          \
    -DCMAKE_INSTALL_PREFIX=/usr/local



echo 'Step 5: Build and install the driver'
sudo cmake --build . --target EP_mnmlstc_core

echo 'build and install the driver:'
cmake --build .
sudo cmake --build . --target install

echo 'Install mongoDB server'
wget -qO - https://www.mongodb.org/static/pgp/server-6.0.asc | sudo apt-key add -

echo "deb [ arch=amd64,arm64 ] https://repo.mongodb.org/apt/ubuntu focal/mongodb-org/6.0 multiverse" | sudo tee /etc/apt/sources.list.d/mongodb-org-6.0.list

yes| sudo apt-get update

yes| sudo apt-get install -y mongodb-org

echo "mongodb-org hold" | sudo dpkg --set-selections
echo "mongodb-org-database hold" | sudo dpkg --set-selections
echo "mongodb-org-server hold" | sudo dpkg --set-selections
echo "mongodb-mongosh hold" | sudo dpkg --set-selections
echo "mongodb-org-mongos hold" | sudo dpkg --set-selections
echo "mongodb-org-tools hold" | sudo dpkg --set-selections


echo 'Starting mongoDB server'
sudo systemctl enable mongod.service
sudo systemctl start mongod


echo 'Restore initial DB data'
cp ~/AOS/AOS-WebAPI/AOS.archive .
#mongorestore -v --nsFrom "~/AOS/AOS-WebAPI/AOS.*" --nsTo "~/AOS/AOS-WebAPI/AOS.*" --uri="mongodb://localhost:27017/" --archive="AOS.archive"
#/home/aos/AOS/AOS-WebAPI
mongorestore -v --nsFrom "AOS.*" --nsTo "AOS.*" --uri="mongodb://localhost:27017/" --archive="AOS.archive"

echo 'Install pymongo'
pip3 install pymongo

#Configure AOS-Solver cmake
cd ~/AOS/AOS-Solver
cmake --no-warn-unused-cli -DCMAKE_EXPORT_COMPILE_COMMANDS:BOOL=TRUE -DCMAKE_BUILD_TYPE:STRING=Debug -DCMAKE_C_COMPILER:FILEPATH=/usr/bin/gcc -DCMAKE_CXX_COMPILER:FILEPATH=/usr/bin/g++ -S~/AOS/AOS-Solver -B~/AOS/AOS-Solver/build -G "Unix Makefiles"

echo 'Install VSCode'
cd ~
mkdir VSCode
cd ~/VSCode

yes| sudo apt-get install wget gpg
wget -qO- https://packages.microsoft.com/keys/microsoft.asc | gpg --dearmor > packages.microsoft.gpg
yes| sudo install -D -o root -g root -m 644 packages.microsoft.gpg /etc/apt/keyrings/packages.microsoft.gpg
yes| sudo sh -c 'echo "deb [arch=amd64,arm64,armhf signed-by=/etc/apt/keyrings/packages.microsoft.gpg] https://packages.microsoft.com/repos/code stable main" > /etc/apt/sources.list.d/vscode.list'
rm -f packages.microsoft.gpg


yes| sudo apt install apt-transport-https
yes| sudo apt update
yes| sudo apt install code 


echo 'Install Postman takes a few minutes'
yes| sudo snap install postman


echo 'Install .NET framework'
wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb

echo 'Install .NET SDK'
sudo apt-get update && \
  sudo apt-get install -y dotnet-sdk-5.0
  
 echo 'Install .NET runtime'
sudo apt-get update && \
  sudo apt-get install -y aspnetcore-runtime-5.0


echo 'build the Web-API'
cd ~/AOS/AOS-WebAPI
dotnet build WebApiCSharp.csproj

cd ~/AOS/AOS-Solver

dotnet build ~/AOS/AOS-WebAPI/WebApiCSharp.csproj /property:GenerateFullPaths=true /consoleloggerparameters:NoSummary
 
echo 'You should manually:'
echo '    1) type "code" and then Install VSCode extensions "C/C++ for Visual Studio Code", "CMake Tools"'
echo '"CMake Tools"'
echo 'Open VSCode click ctl+shift+p and write "cmake:configure", click it and select debuger'
echo '    2)POSTMAN from https://www.postman.com/downloads/ '
echo '    3)optional (300MB): download and install "Download Studio 3T" (to see DB content) from https://studio3t.com/download-thank-you/?OS=x64   '
echo 'to run the AOS use the command "cd ~/AOS/AOS-WebAPI/bin/Debug/net5.0 && ./WebApiCSharp"'



