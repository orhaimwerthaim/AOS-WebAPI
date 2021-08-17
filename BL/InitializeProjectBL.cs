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
        public Process AosSolverProcess = null;
        private static Configuration configuration { get; set; }
        enum PlpType { Environment, EnvironmentGlue, PLP, Glue }
        private static StringBuilder buildOutput = null;

        private static void BuildOutputHandler(object sendingProcess,
            DataReceivedEventArgs outLine)
        {
            if (!String.IsNullOrEmpty(outLine.Data))
            {
                // Add the text to the collected output.
                buildOutput.Append(Environment.NewLine + outLine.Data);
            }
        }

        static InitializeProjectBL()
        {
            configuration = ConfigurationService.Get();
        }

        private static string RunSolver()
        {
            ProcessStartInfo sInfo = new ProcessStartInfo()
            {
                WorkingDirectory = configuration.SolverPath + "/build",
                FileName = "bash",
                Arguments = "runSolverWrapper.sh"

            };
            Process process = new Process();
            process.StartInfo = sInfo;

            process.StartInfo.UseShellExecute = false;

            process.StartInfo.RedirectStandardOutput = true;
            buildOutput = new StringBuilder();

            // Set our event handler to asynchronously read the sort output.
            process.OutputDataReceived += BuildOutputHandler;
            // Redirect standard input as well.  This stream
            // is used synchronously.
            process.StartInfo.RedirectStandardInput = true;
            process.Start();

            process.BeginOutputReadLine();


            return buildOutput.ToString();
        }
        private static string BuildAosSolver()
        {
            ProcessStartInfo sInfo = new ProcessStartInfo()
            {
                WorkingDirectory = configuration.SolverPath,// "/home/.../AOS-Solver",
                FileName = "/opt/cmake-3.19.8-Linux-x86_64/bin/cmake",
                Arguments = "--build /home/or/Projects/AOS-Solver/build --config Debug --target all -j 14 --"
            };
            Process process = new Process();//Process.Start("echo", "asdasd"); 
            process.StartInfo = sInfo;
            // Set UseShellExecute to false for redirection.
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;

            buildOutput = new StringBuilder();

            // Set our event handler to asynchronously read the sort output.
            process.OutputDataReceived += BuildOutputHandler;
            // Redirect standard input as well.  This stream
            // is used synchronously.
            process.StartInfo.RedirectStandardInput = true;

            // Start the process.
            process.Start();

            process.BeginOutputReadLine();
            process.WaitForExit();
            int pid = process.Id;
            string output = buildOutput.ToString();
            process.Close();
            process.OutputDataReceived -= BuildOutputHandler;
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


        public static void InitializeProject(InitializeProject initProj, out List<String> errors, out string buildOutput, out string runOutput)
        {
            buildOutput = "";
            runOutput = "";
            errors = new List<string>();
            List<String> tempErrors;

            AosGeneralService.DeleteCollectionsBeforeProjectInitialization();
            errors.AddRange(LoadPLPs(initProj.PLPsDirectoryPath));
            PLPsData plpData = new PLPsData(out tempErrors);
            errors.AddRange(tempErrors);


            GenerateSolver generateSolver = new GenerateSolver(plpData, initProj);

            if (errors.Count > 0)
            {
                return;
            }
            //buildOutput = BuildAosSolver();

            //runOutput = RunSolver();


            new GenerateRosMiddleware(plpData, initProj);
        }
        private static List<String> LoadPLPs(string pLPsDirectoryPath)
        {
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
