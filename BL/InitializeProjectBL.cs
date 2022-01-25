using System;
using System.Collections.Generic;
using WebApiCSharp.Services;
using System.Text.Json;
using System.IO;
using System.Diagnostics;
using System.Text;
using MongoDB.Bson;
using WebApiCSharp.GenerateCodeFiles;
using WebApiCSharp.Models;
using System.Threading;

namespace WebApiCSharp.BL
{

    public class InitializeProjectBL
    {
        private static CountdownEvent countdownEvent;
        public Process AosSolverProcess = null;
        private static Configuration configuration { get; set; }
        enum PlpType { Environment, EnvironmentGlue, PLP, Glue }
        private static StringBuilder buildOutput = null;

        private static void BuildOutputHandler(object sendingProcess,
            DataReceivedEventArgs outLine)
        {
            if (outLine.Data != null)
            {
                // Add the text to the collected output.
                buildOutput.Append(Environment.NewLine + outLine.Data);
                //  Console.WriteLine("~~~~~~AAAAAAAAAAAAA~~~~~~~~~~~~~~~~~~ " + outLine.Data);
            }
            else
            {
                // end of stream
                if (!countdownEvent.IsSet)
                {
                    countdownEvent.Signal();
                }
            }
        }

        static InitializeProjectBL()
        {
            configuration = ConfigurationService.Get();
        }

        private static string GetRunSolverBashFile(PLPsData data)
        {
            string file = @"#!/bin/bash

cd /home/or/Projects/AOS-Solver/build/examples/cpp_models/" + data.ProjectName + @"
pwd
./despot_" + data.ProjectName;
            return file;
        }

        private static string GetBuildRosMiddlewareBashFile(InitializeProject initProj)
        {
            string file = @"#!/bin/bash

cd " + initProj.RosTarget.WorkspaceDirectortyPath + @"
catkin_make";
            return file;
        }

        private static string GetBuildSolverBashFile()
        {
            string file = @"#!/bin/bash

/opt/cmake-3.19.8-Linux-x86_64/bin/cmake --build /home/or/Projects/AOS-Solver/build --config Debug --target all -j 14 --
";
            return file;
        }
        private static void RunSolver(PLPsData data)
        {
            GenerateFilesUtils.WriteTextFile(configuration.SolverPath + "/build/runSolverWrapper.sh", GetRunSolverBashFile(data), false);
            ProcessStartInfo sInfo = new ProcessStartInfo()
            {
                //WorkingDirectory = configuration.SolverPath + "/build",
                //WorkingDirectory = ,
                FileName = configuration.SolverPath + "/build/examples/cpp_models/" + data.ProjectName + "/despot_" + data.ProjectName,
                //FileName = "bash",
                //Arguments = "runSolverWrapper.sh",
                UseShellExecute = false,
                RedirectStandardOutput = true

            };
            Process process = new Process();
            process.StartInfo = sInfo;


            // Redirect standard input as well.  This stream
            // is used synchronously. 
            process.Start();
        }
        private static string BuildAosSolver()
        {
            Directory.Delete(configuration.SolverPath + "/build/examples",true);//delete old builds
            countdownEvent = new CountdownEvent(1);
            GenerateFilesUtils.WriteTextFile(configuration.SolverPath + "/BuildSolverWrapper.sh", GetBuildSolverBashFile(), false);
            ProcessStartInfo sInfo = new ProcessStartInfo()
            {
                WorkingDirectory = configuration.SolverPath,// "/home/.../AOS-Solver",
                FileName = "bash",
                Arguments = "BuildSolverWrapper.sh",
                //FileName = "/opt/cmake-3.19.8-Linux-x86_64/bin/cmake",
                //Arguments = "--build /home/or/Projects/AOS-Solver/build --config Debug --target all -j 14 --",
                CreateNoWindow = true
            };
            Process process = new Process();//Process.Start("echo", "asdasd"); 
            process.StartInfo = sInfo;
            // Set UseShellExecute to false for redirection.
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;

            buildOutput = new StringBuilder();

            // Set our event handler to asynchronously read the sort output.
            process.OutputDataReceived += BuildOutputHandler;
            // process.ErrorDataReceived += BuildOutputHandler;
            // Redirect standard input as well.  This stream
            // is used synchronously.
            //process.StartInfo.RedirectStandardInput = true;

            // Start the process.
            process.Start();
            // while (!process.StandardOutput.EndOfStream)
            // {
            //     string line = process.StandardOutput.ReadLine();
            //     Console.WriteLine("AAAAAAAAAA   " + line);
            //     // do something with line
            // }


            process.BeginOutputReadLine();
            process.WaitForExit();
            countdownEvent.Wait();
            int pid = process.Id;
            string output = buildOutput.ToString();
            output = output.Contains("[100%] Built target") ? "[100%] Built target" : output;
            process.Close();
            //process.OutputDataReceived -= BuildOutputHandler;
            //return buildOutput.ToString();
            return output;
        }
        private static bool IsValidPLP(JsonDocument plp, out List<String> errorMessages)
        {
            errorMessages = new List<string>();
            JsonElement root = plp.RootElement;
            JsonElement studentsElement;
            if (!root.TryGetProperty("PlpMain", out studentsElement))
            {
                errorMessages.Add("no 'PlpMain' element");
            }

            return errorMessages.Count == 0;
        }


        public static void InitializeProject(InitializeProject initProj, out List<String> errors, out List<String> remarks, out string buildOutput, out string buildRosMiddlewareOutput)
        {
            remarks = new List<string>();
            errors = new List<string>();
            buildOutput = "";
            buildRosMiddlewareOutput = "";
            try
            {
                int solverId = -1;
                List<String> tempErrors;
                List<String> tempRemarks;
                if (initProj.RunWithoutRebuild.Value)//stop solver if it is running
                {
                    solverId = SolversService.GetNextNewSolverId() - 1;
                    SolversService.StopOrStartSolver(solverId, true, initProj.SolverConfiguration.PlanningTimePerMoveInSeconds);
                }
                AosGeneralService.DeleteCollectionsBeforeProjectInitialization(); 
                errors.AddRange(LoadPLPs(initProj.PLPsDirectoryPath, out tempRemarks));
                remarks.AddRange(tempRemarks);

                PLPsData plpData = new PLPsData(out tempErrors);
                errors.AddRange(tempErrors);

                if(initProj.RunWithoutRebuild.Value)
                {
                    SolversService.StopOrStartSolver(solverId, false, initProj.SolverConfiguration.PlanningTimePerMoveInSeconds);//set solver to start mode at db

                    if (!initProj.OnlyGenerateCode.Value)
                    {
                        RunSolver(plpData);//start solver process
                        RunRosMiddleware(initProj);
                        buildOutput = "request included 'RunWithoutRebuild'";
                        buildRosMiddlewareOutput = "request included 'RunWithoutRebuild'";
                    }
                    return;
                }
                else
                {
                    if (errors.Count > 0)
                    {
                        return;
                    }
                    int nextSolverId = SolversService.RemoveAllSolversAndGetNextSolverID();
                    Solver solver = new Solver() { ProjectName = plpData.ProjectName, SolverId = nextSolverId, ServerGeneratedSolverDateTime = DateTime.UtcNow };
                    solver = SolversService.Add(solver);
                    GenerateSolver generateSolver = new GenerateSolver(plpData, initProj, solver);
                    new GenerateRosMiddleware(plpData, initProj);

                    if (!initProj.OnlyGenerateCode.Value)
                    {
                        buildOutput = BuildAosSolver();



                        buildRosMiddlewareOutput = initProj.SolverConfiguration.IsInternalSimulation ? "InternalSimulation" : BuildRosMiddleware(initProj);
                        RunSolver(plpData);
                        if (!initProj.SolverConfiguration.IsInternalSimulation)
                        {
                            RunRosMiddleware(initProj);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                errors.Add(e.Message);
            }
        }

        private static void RunRosMiddleware(InitializeProject initProg)
        {
            //countdownEvent = new CountdownEvent(1);
            //         GenerateFilesUtils.WriteTextFile(initProg.RosTarget.WorkspaceDirectortyPath + "/buildRosMiddlewareWrapper.sh", GetBuildRosMiddlewareBashFile(initProg), false);
            ProcessStartInfo sInfo = new ProcessStartInfo()
            {
                //               WorkingDirectory = initProg.RosTarget.WorkspaceDirectortyPath,
                FileName = "killall",//rosnode
                Arguments = "-9 rosmaster",//kill --all
                UseShellExecute = true,
                // RedirectStandardOutput = true
            };
            Process process = new Process();
            process.StartInfo = sInfo;
            process.Start();

            // sInfo = new ProcessStartInfo()
            // {
            //     //               WorkingDirectory = initProg.RosTarget.WorkspaceDirectortyPath,
            //     FileName = "roscore",
            //     Arguments = "",
            //     UseShellExecute = true,
            //     // RedirectStandardOutput = true

            // };
            //  process = new Process();
            // process.StartInfo = sInfo;
            // process.Start();




            sInfo = new ProcessStartInfo()
            {
                WorkingDirectory = initProg.RosTarget.WorkspaceDirectortyPath,
                FileName = "roslaunch",
                Arguments = initProg.RosTarget.TargetProjectLaunchFile,
                UseShellExecute = true,
                // RedirectStandardOutput = true

            };
            process = new Process();
            process.StartInfo = sInfo;
            process.Start();

            Thread.Sleep(Convert.ToInt32(initProg.RosTarget.TargetProjectInitializationTimeInSeconds.Value * 1000));
            sInfo = new ProcessStartInfo()
            {
                FileName = "rosrun",
                Arguments = "aos_ros_middleware_auto aos_ros_middleware_auto_node.py",
            };
            process = new Process();
            process.StartInfo = sInfo;
            process.Start();
            
        }
        private static string BuildRosMiddleware(InitializeProject initProg)
        {
            countdownEvent = new CountdownEvent(1);
            GenerateFilesUtils.WriteTextFile(initProg.RosTarget.WorkspaceDirectortyPath + "/buildRosMiddlewareWrapper.sh", GetBuildRosMiddlewareBashFile(initProg), false);
            ProcessStartInfo sInfo = new ProcessStartInfo()
            {
                WorkingDirectory = initProg.RosTarget.WorkspaceDirectortyPath,
                FileName = "bash",
                Arguments = "buildRosMiddlewareWrapper.sh",
                UseShellExecute = false,
                RedirectStandardOutput = true

            };
            Process process = new Process();
            process.StartInfo = sInfo;
            buildOutput = new StringBuilder();

            // Set our event handler to asynchronously read the sort output.
            process.OutputDataReceived += BuildOutputHandler;

            // Redirect standard input as well.  This stream
            // is used synchronously. 
            process.Start();

            process.BeginOutputReadLine();
            // process.WaitForExit();
            countdownEvent.Wait();
            string buildOutputRos = buildOutput.ToString();
            buildOutputRos = buildOutputRos.Contains("[100%] Built target") ? "[100%] Built target" : buildOutputRos;
            return buildOutputRos;
        }
        private static List<String> LoadPLPs(string pLPsDirectoryPath, out List<string> remarks)
        {
            remarks = new List<string>();
            List<String> errorMessages = new List<string>();

            if (!Directory.Exists(pLPsDirectoryPath))
            {
                errorMessages.Add("The PLPs directory '" + pLPsDirectoryPath + "', is not a Directory!");
                return errorMessages;
            }
            string[] fileEntries = Directory.GetFiles(pLPsDirectoryPath);
            if (fileEntries.Length == 0)
            {
                errorMessages.Add("The PLPs directory '" + pLPsDirectoryPath + "', is empty!");
                return errorMessages;
            }

            foreach (string filePath in fileEntries)
            {
                try
                {
                    FileInfo fi = new FileInfo(filePath);
                    if (filePath.ToLower().EndsWith(".json"))
                    {
                        string fileContent = System.IO.File.ReadAllText(filePath);
                        using (JsonDocument plp = JsonDocument.Parse(fileContent))
                        {
                            List<String> plpParseErrors;
                            if (!IsValidPLP(plp, out plpParseErrors))
                            {
                                errorMessages.Add("File '" + filePath + "' is not a valid PLP");
                                errorMessages.AddRange(plpParseErrors);
                                return errorMessages;
                            }
                            BsonDocument bPLP = PLPsService.Add(plp);
                        }
                        remarks.Add("File '" + filePath + "' was successfully loaded");
                    }
                    else
                    {
                        remarks.Add("File '" + filePath + "' was not loaded since it's extension is not '.json'");
                    }
                }
                catch (Exception e)
                {
                    errorMessages.Add("PLP file '" + filePath + "' is not in a valid JSON!");
                    errorMessages.Add(e.ToString());
                    return errorMessages;
                }
            }
            return errorMessages;
        }
    }
}
