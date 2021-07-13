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
        private PLPsData plpsData; 
        private string projectNameWithCapitalFirstLetter;
        private static Configuration conf;
        static GenerateSolver()
        {
            conf = ConfigurationService.Get();
        }

        private string PomcpFilePath;
        private string EvaluatorFilePath;
        private string ProjectExamplePath;

        private string ProjectExamplePathSrc;
        private string ProjectModelPrimitivesPath;
        private string ProjectHeaderModelPrimitivesPath;
        public GenerateSolver(PLPsData data)
        {
            plpsData = data;
            projectNameWithCapitalFirstLetter = char.ToUpper(plpsData.ProjectName[0]) + plpsData.ProjectName.Substring(1);

            ProjectHeaderModelPrimitivesPath = conf.SolverPath + "/include/despot/model_primitives/" + plpsData.ProjectName;
            ProjectModelPrimitivesPath = conf.SolverPath + "/src/model_primitives/" + plpsData.ProjectName;
            ProjectExamplePathSrc = conf.SolverPath + "/examples/cpp_models/" + plpsData.ProjectName + "/src";
            ProjectExamplePath=conf.SolverPath + "/examples/cpp_models/" + plpsData.ProjectName;
            EvaluatorFilePath=conf.SolverPath + "/src/evaluator.cpp";
            PomcpFilePath=conf.SolverPath + "/src/solver/pomcp.cpp";

            CleanAndGenerateDirecotories();

            AddCMakeFiles();

            GenerateCodeFilesUtils.WriteTextFile(PomcpFilePath, SolverFileTemplate.GetPOMCP_File(conf.SolverGraphPDF_DirectoryPath, conf.SolverGraphPDF_Depth));
            GenerateCodeFilesUtils.WriteTextFile(EvaluatorFilePath, SolverFileTemplate.GetEvaluatorFile(plpsData.ProjectName));

        }

        private void DeleteAndCreateDirectory(string dirPath)
        {
            if (Directory.Exists(dirPath))
            {
                Directory.Delete(dirPath, true);
            }
            Directory.CreateDirectory(dirPath);
        }
        private void CleanAndGenerateDirecotories()
        {
            DeleteAndCreateDirectory(ProjectExamplePath);
            Directory.CreateDirectory(ProjectExamplePathSrc);
            DeleteAndCreateDirectory(ProjectModelPrimitivesPath);
            DeleteAndCreateDirectory(ProjectHeaderModelPrimitivesPath);

        }

        private void AddCMakeFiles()
        {
            GenerateCodeFilesUtils.WriteTextFile(ProjectExamplePath + "/Makefile", SolverFileTemplate.GetProjectExample_MakeFile());
            GenerateCodeFilesUtils.WriteTextFile(ProjectExamplePath + "/CMakeLists.txt", SolverFileTemplate.GetProjectExample_CMakeLists(plpsData.ProjectName));

            GenerateCodeFilesUtils.WriteTextFile(conf.SolverPath + "/CMakeLists.txt", SolverFileTemplate.GetBasePath_CMakeLists(plpsData.ProjectName));


        }
    }
}