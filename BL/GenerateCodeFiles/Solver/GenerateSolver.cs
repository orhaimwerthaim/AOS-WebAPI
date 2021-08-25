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

        

        public GenerateSolver(PLPsData data, InitializeProject initProj)
        {
            int totalNumberOfActionsInProject;
            plpsData = data;
            projectNameWithCapitalFirstLetter = char.ToUpper(plpsData.ProjectName[0]) + plpsData.ProjectName.Substring(1);

            ProjectHeaderModelPrimitivesPath = conf.SolverPath + "/include/despot/model_primitives/" + plpsData.ProjectName;
            ProjectModelPrimitivesPath = conf.SolverPath + "/src/model_primitives/" + plpsData.ProjectName;
            ProjectExamplePathSrc = conf.SolverPath + "/examples/cpp_models/" + plpsData.ProjectName + "/src";
            ProjectExamplePath = conf.SolverPath + "/examples/cpp_models/" + plpsData.ProjectName;



            CleanAndGenerateDirecotories();

            GenerateFilesUtils.WriteTextFile(ProjectHeaderModelPrimitivesPath + "/actionManager.h", SolverFileTemplate.GetActionManagerHeaderFile(data));
            Dictionary<string,Dictionary<string, string>> enumMappingsForModuleResponseAndTempVar;
            GenerateFilesUtils.WriteTextFile(ProjectHeaderModelPrimitivesPath + "/enum_map_" + data.ProjectName + ".h", SolverFileTemplate.GetEnumMapHeaderFile(data, out enumMappingsForModuleResponseAndTempVar));
            SolverFileTemplate.EnumMappingsForModuleResponseAndTempVar = enumMappingsForModuleResponseAndTempVar;
            GenerateFilesUtils.WriteTextFile(ProjectHeaderModelPrimitivesPath + "/state_var_types.h", SolverFileTemplate.GetStateVarTypesHeaderFile(data));
            GenerateFilesUtils.WriteTextFile(ProjectHeaderModelPrimitivesPath + "/state.h", SolverFileTemplate.GetStateHeaderFile(data));
            GenerateFilesUtils.WriteTextFile(conf.SolverPath + "/include/despot/config.h", SolverFileTemplate.GetConfigHeaderFile(data, initProj));


            GenerateFilesUtils.WriteTextFile(ProjectExamplePath + "/Makefile", SolverFileTemplate.GetProjectExample_MakeFile());
            GenerateFilesUtils.WriteTextFile(ProjectExamplePath + "/CMakeLists.txt", SolverFileTemplate.GetProjectExample_CMakeLists(plpsData.ProjectName));
            GenerateFilesUtils.WriteTextFile(conf.SolverPath + "/CMakeLists.txt", SolverFileTemplate.GetBasePath_CMakeLists(plpsData.ProjectName));


            GenerateFilesUtils.WriteTextFile(conf.SolverPath + "/src/solver/pomcp.cpp", SolverFileTemplate.GetPOMCP_File(conf.SolverGraphPDF_DirectoryPath, conf.SolverGraphPDF_Depth, initProj));
            GenerateFilesUtils.WriteTextFile(conf.SolverPath + "/src/evaluator.cpp", SolverFileTemplate.GetEvaluatorCppFile(data,  initProj));
            GenerateFilesUtils.WriteTextFile(conf.SolverPath + "/src/simple_tui.cpp", SolverFileTemplate.GetSimpleTuiCppFile(data, initProj));
            GenerateFilesUtils.WriteTextFile(conf.SolverPath + "/include/despot/evaluator.h", SolverFileTemplate.GetEvaluatorHeaderFile(data));
 

            GenerateFilesUtils.WriteTextFile(ProjectModelPrimitivesPath + "/actionManager.cpp", SolverFileTemplate.GetActionManagerCPpFile(data, out totalNumberOfActionsInProject));
            data.NumberOfActions = totalNumberOfActionsInProject;
            GenerateFilesUtils.WriteTextFile(ProjectModelPrimitivesPath + "/enum_map_" + data.ProjectName + ".cpp", SolverFileTemplate.GetEnumMapCppFile(data));
            //GenerateFilesUtils.WriteTextFile(ProjectModelPrimitivesPath + "/state_var_types.cpp", SolverFileTemplate.GetStateVarTypesCppFile(data));
            GenerateFilesUtils.WriteTextFile(ProjectModelPrimitivesPath + "/state.cpp", SolverFileTemplate.GetStateCppFile(data));

            GenerateFilesUtils.WriteTextFile(ProjectExamplePathSrc + "/globals.h", SolverFileTemplate.GetGlobalsFile(data, totalNumberOfActionsInProject));
            GenerateFilesUtils.WriteTextFile(ProjectExamplePathSrc + "/main.cpp", SolverFileTemplate.GetMainFile(data));
            GenerateFilesUtils.WriteTextFile(ProjectExamplePathSrc + "/" + data.ProjectName + ".cpp", SolverFileTemplate.GetModelCppFile(data, initProj));
            GenerateFilesUtils.WriteTextFile(ProjectExamplePathSrc + "/" + data.ProjectName + ".h", SolverFileTemplate.GetModelHeaderFile(data));



        }
 
        private void CleanAndGenerateDirecotories()
        {
            GenerateFilesUtils.DeleteAndCreateDirectory(ProjectExamplePath);
            Directory.CreateDirectory(ProjectExamplePathSrc);
            GenerateFilesUtils.DeleteAndCreateDirectory(ProjectModelPrimitivesPath);
            GenerateFilesUtils.DeleteAndCreateDirectory(ProjectHeaderModelPrimitivesPath);

        }

    }
}