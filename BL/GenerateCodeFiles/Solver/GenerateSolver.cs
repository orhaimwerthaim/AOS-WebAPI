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

        private string ProjectExamplePath;

        private string ProjectExamplePathSrc;
        private string ProjectModelPrimitivesPath;
        private string ProjectHeaderModelPrimitivesPath;

        

        public GenerateSolver(PLPsData data)
        {
            int totalNumberOfActionsInProject;
            plpsData = data;
            projectNameWithCapitalFirstLetter = char.ToUpper(plpsData.ProjectName[0]) + plpsData.ProjectName.Substring(1);

            ProjectHeaderModelPrimitivesPath = conf.SolverPath + "/include/despot/model_primitives/" + plpsData.ProjectName;
            ProjectModelPrimitivesPath = conf.SolverPath + "/src/model_primitives/" + plpsData.ProjectName;
            ProjectExamplePathSrc = conf.SolverPath + "/examples/cpp_models/" + plpsData.ProjectName + "/src";
            ProjectExamplePath = conf.SolverPath + "/examples/cpp_models/" + plpsData.ProjectName;



            CleanAndGenerateDirecotories();

            GenerateCodeFilesUtils.WriteTextFile(ProjectHeaderModelPrimitivesPath + "/actionManager.h", SolverFileTemplate.GetActionManagerHeaderFile(data));
            Dictionary<string,Dictionary<string, string>> enumMappingsForModuleResponseAndTempVar;
            GenerateCodeFilesUtils.WriteTextFile(ProjectHeaderModelPrimitivesPath + "/enum_map_" + data.ProjectName + ".h", SolverFileTemplate.GetEnumMapHeaderFile(data, out enumMappingsForModuleResponseAndTempVar));
            SolverFileTemplate.EnumMappingsForModuleResponseAndTempVar = enumMappingsForModuleResponseAndTempVar;
            GenerateCodeFilesUtils.WriteTextFile(ProjectHeaderModelPrimitivesPath + "/state_var_types.h", SolverFileTemplate.GetStateVarTypesHeaderFile(data));
            GenerateCodeFilesUtils.WriteTextFile(ProjectHeaderModelPrimitivesPath + "/state.h", SolverFileTemplate.GetStateHeaderFile(data));


            GenerateCodeFilesUtils.WriteTextFile(ProjectExamplePath + "/Makefile", SolverFileTemplate.GetProjectExample_MakeFile());
            GenerateCodeFilesUtils.WriteTextFile(ProjectExamplePath + "/CMakeLists.txt", SolverFileTemplate.GetProjectExample_CMakeLists(plpsData.ProjectName));
            GenerateCodeFilesUtils.WriteTextFile(conf.SolverPath + "/CMakeLists.txt", SolverFileTemplate.GetBasePath_CMakeLists(plpsData.ProjectName));


            GenerateCodeFilesUtils.WriteTextFile(conf.SolverPath + "/src/solver/pomcp.cpp", SolverFileTemplate.GetPOMCP_File(conf.SolverGraphPDF_DirectoryPath, conf.SolverGraphPDF_Depth));
            GenerateCodeFilesUtils.WriteTextFile(conf.SolverPath + "/src/evaluator.cpp", SolverFileTemplate.GetEvaluatorCppFile(data));
            GenerateCodeFilesUtils.WriteTextFile(conf.SolverPath + "/src/simple_tui.cpp", SolverFileTemplate.GetSimpleTuiCppFile(data));
            GenerateCodeFilesUtils.WriteTextFile(conf.SolverPath + "/include/despot/evaluator.h", SolverFileTemplate.GetEvaluatorHeaderFile(data));
 

            GenerateCodeFilesUtils.WriteTextFile(ProjectModelPrimitivesPath + "/actionManager.cpp", SolverFileTemplate.GetActionManagerCPpFile(data, out totalNumberOfActionsInProject));
            data.NumberOfActions = totalNumberOfActionsInProject;
            GenerateCodeFilesUtils.WriteTextFile(ProjectModelPrimitivesPath + "/enum_map_" + data.ProjectName + ".cpp", SolverFileTemplate.GetEnumMapCppFile(data));
            //GenerateCodeFilesUtils.WriteTextFile(ProjectModelPrimitivesPath + "/state_var_types.cpp", SolverFileTemplate.GetStateVarTypesCppFile(data));
            GenerateCodeFilesUtils.WriteTextFile(ProjectModelPrimitivesPath + "/state.cpp", SolverFileTemplate.GetStateCppFile(data));

            GenerateCodeFilesUtils.WriteTextFile(ProjectExamplePathSrc + "/globals.h", SolverFileTemplate.GetGlobalsFile(data, totalNumberOfActionsInProject));
            GenerateCodeFilesUtils.WriteTextFile(ProjectExamplePathSrc + "/main.cpp", SolverFileTemplate.GetMainFile(data));
            GenerateCodeFilesUtils.WriteTextFile(ProjectExamplePathSrc + "/" + data.ProjectName + ".cpp", SolverFileTemplate.GetModelCppFile(data));
            GenerateCodeFilesUtils.WriteTextFile(ProjectExamplePathSrc + "/" + data.ProjectName + ".h", SolverFileTemplate.GetModelHeaderFile(data));



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

    }
}