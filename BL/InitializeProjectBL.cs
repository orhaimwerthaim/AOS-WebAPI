using System;
using System.Collections.Generic;
using WebApiCSharp.Services;
using System.Text.Json;
using System.IO;
using System.Diagnostics;
using System.Text;
using MongoDB.Bson;
using WebApiCSharp.GenerateCodeFiles;

namespace WebApiCSharp.BL
{

    public class InitializeProjectBL
    {
        enum PlpType { Environment, EnvironmentGlue, PLP, Glue }
        private static StringBuilder buildOutput = null;
        private static StringBuilder buildErrors = null;

        private static void BuildOutputHandler(object sendingProcess,
            DataReceivedEventArgs outLine)
        {
            // Collect the sort command output.
            if (!String.IsNullOrEmpty(outLine.Data))
            {
                // Add the text to the collected output.
                buildOutput.Append(Environment.NewLine + outLine.Data);
                if (outLine.Data.ToLower().Contains("error"))
                    buildErrors.Append(Environment.NewLine + outLine.Data);
            }
        }
        private static void BuildAosSolver()
        {
            // Get the path that stores favorite links.
            ProcessStartInfo sInfo = new ProcessStartInfo()
            {
                WorkingDirectory = "/home/or/Projects/AOS-Solver",
                FileName = "/opt/cmake-3.19.8-Linux-x86_64/bin/cmake",
                Arguments = "--build /home/or/Projects/AOS-Solver/build --config Debug --target all -j 14 --"
            };
            Process process = Process.Start(sInfo);//Process.Start("echo", "asdasd"); 

            // Set UseShellExecute to false for redirection.
            process.StartInfo.UseShellExecute = false;

            process.StartInfo.RedirectStandardOutput = true;



            buildOutput = new StringBuilder();
            buildErrors = new StringBuilder();

            // Set our event handler to asynchronously read the sort output.
            process.OutputDataReceived += BuildOutputHandler;
            // Redirect standard input as well.  This stream
            // is used synchronously.
            process.StartInfo.RedirectStandardInput = true;

            // Start the process.
            process.Start();

            process.BeginOutputReadLine();
            process.WaitForExit();

            Console.WriteLine("Build output:");
            Console.WriteLine(buildOutput);
            Console.WriteLine("Build Errors:");
            Console.WriteLine(buildErrors);

            process.Close();
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


        public static List<String> InitializeProject(string pLPsDirectoryPath)
        {
            List<String> errors = LoadPLPs(pLPsDirectoryPath);
            if (errors.Count > 0) return errors;

            PLPsData plpData = new PLPsData(out errors);
            if (errors.Count > 0) return errors;

            GenerateSolver generateSolver = new GenerateSolver(plpData);


            return errors;
        }
        public static List<String> LoadPLPs(string pLPsDirectoryPath)
        {
            PLPsService.DeleteAll();
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
