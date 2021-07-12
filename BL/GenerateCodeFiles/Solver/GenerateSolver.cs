using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using WebApiCSharp.Services;
using WebApiCSharp.Models;
using System.IO;
using System.Collections.Generic;
namespace WebApiCSharp.GenerateCodeFiles
{
    public class GenerateSolver
    {
        private static string projectName = "";
        private static Configuration conf;
        static GenerateSolver()
        {
            conf = ConfigurationService.Get(); 
        }

        private static string PomcpFilePath{
            get
            {
                return conf.SolverPath + "/src/solver/pomcp.cpp";
            }
            set { }
        }
        private static string ProjectExamplePath
        {
            get
            {
                return conf.SolverPath + "/examples/cpp_models/" + projectName;
            }
            set { }
        }

        private static string ProjectExamplePathSrc
        {
            get
            {
                return conf.SolverPath + "/examples/cpp_models/" + projectName + "/src";
            }
            set { }
        }
        private static string ProjectModelPrimitivesPath
        {
            get
            {
                return conf.SolverPath + "/src/model_primitives/" + projectName;
            }
            set { }
        }
        private static string ProjectHeaderModelPrimitivesPath
        {
            get
            {
                return conf.SolverPath + "/include/despot/model_primitives/" + projectName;
            }
            set { }
        }
        public static void Run()
        {
            projectName = "icaps2";
            CleanAndGenerateDirecotories();
            
            AddCMakeFiles();

            GenerateCodeFilesUtils.WriteTextFile(PomcpFilePath, SolverFileTemplate.GetPOMCP_File(conf.SolverGraphPDF_DirectoryPath,conf.SolverGraphPDF_Depth));
        }

        private static void DeleteAndCreateDirectory(string dirPath)
        {
            if (Directory.Exists(dirPath))
            {
                Directory.Delete(dirPath, true);
            }
            Directory.CreateDirectory(dirPath);
        }
        private static void CleanAndGenerateDirecotories()
        {
            DeleteAndCreateDirectory(ProjectExamplePath);
            Directory.CreateDirectory(ProjectExamplePathSrc);
            DeleteAndCreateDirectory(ProjectModelPrimitivesPath);
            DeleteAndCreateDirectory(ProjectHeaderModelPrimitivesPath);

        }

        private static void AddCMakeFiles()
        {
            GenerateCodeFilesUtils.WriteTextFile(ProjectExamplePath + "/Makefile", SolverFileTemplate.GetProjectExample_MakeFile());
            GenerateCodeFilesUtils.WriteTextFile(ProjectExamplePath + "/CMakeLists.txt", SolverFileTemplate.GetProjectExample_CMakeLists(projectName));

            GenerateCodeFilesUtils.WriteTextFile(conf.SolverPath + "/CMakeLists.txt", SolverFileTemplate.GetBasePath_CMakeLists(projectName));


        }
    }
}