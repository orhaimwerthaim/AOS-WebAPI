using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using WebApiCSharp.Services;
using WebApiCSharp.Models;
using System.IO;
using System.Collections.Generic;
namespace WebApiCSharp.GenerateCodeFiles
{
    public class GenerateBlackBox
    {
        private PLPsData plpsData;
        private string projectNameWithCapitalFirstLetter;
        private static Configuration conf;
        static GenerateBlackBox()
        {
            conf = ConfigurationService.Get();
        }

        private string ProjectExamplePath;

        private string ProjectExamplePathSrc;
        private string ProjectModelPrimitivesPath;
        private string ProjectHeaderModelPrimitivesPath;

        private string ProjectHeaderModelPrimitivesBasePath;

        private string ProjectModelPrimitivesBasePath;



        public GenerateBlackBox(PLPsData data, InitializeProject initProj, Solver solverData)
        {
            int totalNumberOfActionsInProject;
            plpsData = data;
            projectNameWithCapitalFirstLetter = char.ToUpper(plpsData.ProjectName[0]) + plpsData.ProjectName.Substring(1);
            
            ProjectHeaderModelPrimitivesBasePath = conf.SolverPath + "/include/despot/model_primitives";
            ProjectHeaderModelPrimitivesPath = ProjectHeaderModelPrimitivesBasePath + "/" + plpsData.ProjectName;
            string HeaderPathCore = conf.SolverPath + "/include/despot/core";
            string CppCorePath = conf.SolverPath + "/src/core";
            ProjectModelPrimitivesBasePath = conf.SolverPath + "/src/model_primitives";
            ProjectModelPrimitivesPath = ProjectModelPrimitivesBasePath + "/" + plpsData.ProjectName;


            ProjectExamplePathSrc = conf.SolverPath + "/examples/cpp_models/" + plpsData.ProjectName + "/src";
            ProjectExamplePath = conf.SolverPath + "/examples/cpp_models/" + plpsData.ProjectName;



            CleanAndGenerateDirecotories();
            GenerateFilesUtils.WriteTextFile(HeaderPathCore + "/pomdp.h", SolverFileTemplate.GetPOMDPHeaderFile(data));
            GenerateFilesUtils.WriteTextFile(CppCorePath + "/pomdp.cpp", SolverFileTemplate.GetPOMDPCPPFile(data));


            GenerateFilesUtils.WriteTextFile(ProjectHeaderModelPrimitivesPath + "/closed_model_policy.h", SolverFileTemplate.GetClosedModelPolicyHeaderFile(data));
            GenerateFilesUtils.WriteTextFile(ProjectHeaderModelPrimitivesPath + "/actionManager.h", SolverFileTemplate.GetActionManagerHeaderFile(data));
            Dictionary<string, Dictionary<string, string>> enumMappingsForModuleResponseAndTempVar;
            GenerateFilesUtils.WriteTextFile(ProjectHeaderModelPrimitivesPath + "/enum_map_" + data.ProjectName + ".h", SolverFileTemplate.GetEnumMapHeaderFile(data, out enumMappingsForModuleResponseAndTempVar));
            SolverFileTemplate.EnumMappingsForModuleResponseAndTempVar = enumMappingsForModuleResponseAndTempVar;
            GenerateFilesUtils.WriteTextFile(ProjectHeaderModelPrimitivesPath + "/state_var_types.h", SolverFileTemplate.GetStateVarTypesHeaderFile(data));
            //GenerateFilesUtils.WriteTextFile(ProjectHeaderModelPrimitivesPath + "/state.h", SolverFileTemplate.GetStateHeaderFile(data));
            GenerateFilesUtils.WriteTextFile(conf.SolverPath + "/include/despot/config.h", SolverFileTemplate.GetConfigHeaderFile(data, initProj, solverData));
//

            GenerateFilesUtils.WriteTextFile(ProjectExamplePath + "/Makefile", SolverFileTemplate.GetProjectExample_MakeFile());
            GenerateFilesUtils.WriteTextFile(ProjectExamplePath + "/CMakeLists.txt", SolverFileTemplate.GetProjectExample_CMakeLists(plpsData.ProjectName, conf, initProj));
            GenerateFilesUtils.WriteTextFile(conf.SolverPath + "/CMakeLists.txt", SolverFileTemplate.GetBasePath_CMakeLists(plpsData.ProjectName, initProj, conf));

GenerateFilesUtils.WriteTextFile(conf.SolverPath + "/include/despot/solver/pomcp.h", SolverFileTemplate.GetPomcpHeaderFile(data));
            
            GenerateFilesUtils.WriteTextFile(conf.SolverPath + "/src/evaluator.cpp", SolverFileTemplate.GetEvaluatorCppFile(data, initProj));
            GenerateFilesUtils.WriteTextFile(conf.SolverPath + "/src/simple_tui.cpp", SolverFileTemplate.GetSimpleTuiCppFile(data, initProj));
            GenerateFilesUtils.WriteTextFile(conf.SolverPath + "/include/despot/evaluator.h", SolverFileTemplate.GetEvaluatorHeaderFile(data, initProj));

            GenerateFilesUtils.WriteTextFile(ProjectModelPrimitivesPath + "/closed_model_policy.cpp", SolverFileTemplate.GetClosedModelPolicyCPpFile(data));

            GenerateFilesUtils.WriteTextFile(ProjectModelPrimitivesPath + "/actionManager.cpp", SolverFileTemplate.GetActionManagerCPpFile(data, out totalNumberOfActionsInProject));
         
            GenerateFilesUtils.WriteTextFile(ProjectModelPrimitivesPath + "/enum_map_" + data.ProjectName + ".cpp", SolverFileTemplate.GetEnumMapCppFile(data));
            //GenerateFilesUtils.WriteTextFile(ProjectModelPrimitivesPath + "/state_var_types.cpp", SolverFileTemplate.GetStateVarTypesCppFile(data));
          //  GenerateFilesUtils.WriteTextFile(ProjectModelPrimitivesPath + "/state.cpp", SolverFileTemplate.GetStateCppFile(data));

            GenerateFilesUtils.WriteTextFile(ProjectExamplePathSrc + "/globals.h", SolverFileTemplate.GetGlobalsFile(data, totalNumberOfActionsInProject));
            GenerateFilesUtils.WriteTextFile(ProjectExamplePathSrc + "/main.cpp", SolverFileTemplate.GetMainFile(data));
            GenerateFilesUtils.WriteTextFile(ProjectExamplePathSrc + "/" + data.ProjectName + ".cpp", SolverFileTemplate.GetModelCppFile(data, initProj));
            GenerateFilesUtils.WriteTextFile(ProjectExamplePathSrc + "/" + data.ProjectName + ".h", SolverFileTemplate.GetModelHeaderFile(data, initProj));

            GenerateFilesUtils.WriteTextFile(ProjectExamplePathSrc + "/closed_model.h", SolverFileTemplate.GetClosedModelHeaderFile(data, initProj));
            GenerateFilesUtils.WriteTextFile(ProjectExamplePathSrc + "/closed_model.cpp", SolverFileTemplate.GetClosedModelCppFile(data, initProj));
    
            GenerateFilesUtils.WriteTextFile(conf.SolverPath + "/src/solver/pomcp.cpp", SolverFileTemplate.GetPOMCP_File(conf.SolverGraphPDF_DirectoryPath, initProj.SolverConfiguration.PolicyGraphDepth, initProj, data));


            GenerateFilesUtils.WriteTextFile(conf.SolverPath + "/src/util/mongoDB_Bridge.cpp", SolverFileTemplate.GetMongoBridgeCppFile(data));

        }

        private void CleanAndGenerateDirecotories()
        {
            GenerateFilesUtils.DeleteAndCreateDirectory(ProjectHeaderModelPrimitivesBasePath);
            GenerateFilesUtils.DeleteAndCreateDirectory(ProjectModelPrimitivesBasePath);
            GenerateFilesUtils.DeleteAndCreateDirectory(ProjectExamplePath);
            Directory.CreateDirectory(ProjectExamplePathSrc);
            GenerateFilesUtils.DeleteAndCreateDirectory(ProjectModelPrimitivesPath);
            GenerateFilesUtils.DeleteAndCreateDirectory(ProjectHeaderModelPrimitivesPath);




            GenerateFilesUtils.DeleteAndCreateDirectory(ProjectHeaderModelPrimitivesPath);
 
        }

    }
}