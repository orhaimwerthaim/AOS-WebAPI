#!/bin/bash
#https://matt.might.net/articles/bash-by-example/
#install VSCode from https://code.visualstudio.com/docs/?dv=linux64_deb



echo 'reinstall the AOS'
cd ~/AOS


yes|rm -r ~/AOS/AOS-WebAPI

echo 'Web API: Downloading code'
git clone https://github.com/orhaimwerthaim/AOS-WebAPI.git
echo 'Web API: Downloaded'

echo 'build the Web-API'
cd ~/AOS/AOS-WebAPI
dotnet build WebApiCSharp.csproj

 

dotnet build ~/AOS/AOS-WebAPI/WebApiCSharp.csproj /property:GenerateFullPaths=true /consoleloggerparameters:NoSummary

echo 'reinstall the AOS Solver'
cd ~/AOS
yes|rm -r ~/AOS/AOS-Solver
git clone https://github.com/orhaimwerthaim/AOS-Solver.git


