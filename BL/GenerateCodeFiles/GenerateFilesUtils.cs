
using MongoDB.Bson;
using System;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using WebApiCSharp.Services;
using WebApiCSharp.Models;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace WebApiCSharp.GenerateCodeFiles
{
    public class GenerateFilesUtils
    {
        public static string RunBashCommand(string cmd, bool waitForExit = true, string workingDir = null)
        {
            var escapedArgs = cmd.Replace("\"", "\\\"");

            var process = new Process()
            {
            StartInfo = new ProcessStartInfo
            {
            FileName = "/bin/bash",
            Arguments = $"-c \"{escapedArgs}\"",
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            }
            };
            if(workingDir != null)
            {
              process =   new Process(){
            StartInfo = new ProcessStartInfo
            {
            FileName = "/bin/bash",
            Arguments = $"-c \"{escapedArgs}\"",
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory=workingDir
            }
            };
            }
            
            process.Start();
            if(waitForExit)
            {
            string result = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            return result;
            }
            return "";
        }
        public static void RunApplicationUntilEnd(string appFilePath, string workingDir = null, string arguments = null)
        {
            ProcessStartInfo sInfo = new ProcessStartInfo()
            {
                FileName = appFilePath,
            };
            Process process = new Process();
            process.StartInfo = sInfo;
            if (workingDir != null)
            {
                process.StartInfo.WorkingDirectory = workingDir;
            }

            if (arguments != null)
            {
                process.StartInfo.Arguments = arguments;
            }


            process.StartInfo.UseShellExecute = true;
            process.Start();
            process.WaitForExit();
            process.Close();

        }
        public static string AppendPath(string basePath, string pathEnd)
        {
            basePath = basePath.EndsWith("/") ? basePath.TrimEnd('/') : basePath;
            pathEnd = pathEnd.StartsWith("/") ? pathEnd.TrimStart('/') : pathEnd;

            string resPath = basePath + "/" + pathEnd;
            return resPath;
        }

        public static bool IsPrimitiveType(string type, bool IncludeIsAnyValue = false)
        {
            return PLPsData.PRIMITIVE_TYPES.Any(x => x.Equals(type)) || (IncludeIsAnyValue && type.Equals(PLPsData.ANY_VALUE_TYPE_NAME)) ;
        }

        public static string ToUpperFirstLetter(string str)
        {
            return char.ToUpper(str[0]) + str.Substring(1);
        }


        private static string GetIdent(int numOfIndentations, int indentSize)
        {
            return (new string(' ', numOfIndentations * indentSize));
        }
        public static string GetIndentationStr(int numOfIndentations, int indentSize = 4, string str = "", bool withNewLine = true, bool isPythonCode = false)
        {
            string result = GetIdent(numOfIndentations, indentSize) + str;
            result = !isPythonCode ? result : 
                result
                .Replace("\n", Environment.NewLine + GetIdent(numOfIndentations, indentSize))
                .Replace("\t", GetIdent(1, indentSize));
            return result + (withNewLine ? Environment.NewLine : "");
        }


        public static void WriteTextFile(string path, string content, bool setExecuteable = false)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            // Create a file to write to.
            using (StreamWriter sw = File.CreateText(path))
            {
                sw.Write(content);
            }
            if (setExecuteable)
            {
                GenerateFilesUtils.RunBashCommand("chmod +x " + path);
                //RunApplicationUntilEnd("chmod", null, "+x " + path);
            }
        }

        public static void DeleteAndCreateDirectory(string dirPath, bool createDirectory = true)
        {
            if (Directory.Exists(dirPath))
            {
                Directory.Delete(dirPath, true);
            }
            if (createDirectory)
            {
                Directory.CreateDirectory(dirPath);
            }
        }
    }
}