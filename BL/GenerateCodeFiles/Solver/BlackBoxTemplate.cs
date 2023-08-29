using System.Data.Common;
using System.Reflection;
using MongoDB.Bson;
using System;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using WebApiCSharp.Services;
using WebApiCSharp.Models;
using System.IO;
using System.Collections.Generic;
using System.Linq;
namespace WebApiCSharp.GenerateCodeFiles
{
    
    public class BlackBoxTemplate
    {
       
        public static string GetSingleFileModel(PLPsData data, bool forCppToPython = false)
        {
            Dictionary<string, Dictionary<string, string>>temp1;
            List<string> temp2;
            int totalNumberOfActionsInProject;
            string file = @"
#include <string>
#include <vector>
#include <tuple>
#include <map>


#include <cstdint>
#include <iostream>
#include <vector>
#include <sstream>
#include <unistd.h>
#include <random>";
if(forCppToPython)
{
    file += @"
#include <iostream>
#include <locale>
#include <sys/time.h>
#include <pybind11/pybind11.h>
#include <pybind11/stl.h>
#include <pybind11/complex.h>
#include <pybind11/chrono.h> 
#include <tuple>
#include <string>
namespace py = pybind11;";
}
file += @"
using namespace std;
namespace aos_model {
typedef bool anyValue;
typedef unsigned long int OBS_TYPE;
std::default_random_engine generator;
std::uniform_real_distribution<float> real_unfirom_dist(0.0,1.0); 

int get_hash(string str_)
    {
        const char *str = str_.c_str();
        unsigned long hash = 0;
        int c;

        while (c = *str++)
            hash = c + (hash << 6) + (hash << 16) - hash;

        return hash; 
    }

int SampleDiscrete(vector<float> weights)
{
    float rand = real_unfirom_dist(generator);
    float total = 0;
    for (int i = 0; i < weights.size();i++)
    {
        total += weights[i];
        if (rand < total)
            return i;
    }
    return -1;
}

int SampleDiscrete(vector<double> weights)
{
    float rand = real_unfirom_dist(generator);
    float total = 0;
    for (int i = 0; i < weights.size();i++)
    {
        total += weights[i];
        if (rand < total)
            return i;
    }
    return -1;
}

bool Bernoulli(double p)
{
    float rand = real_unfirom_dist(generator); 
	
	return rand <= p;
}

bool Bernoulli(float p)
{
    float rand = real_unfirom_dist(generator); 
	
	return rand <= p;
}

" + SolverFileTemplate.GetEnumMapHeaderFile(data, out temp1, true) + 
    SolverFileTemplate.GetEnumMapCppFile(data,true) + Environment.NewLine 
  + SolverFileTemplate.GetGetStateVarTypesHeaderEnumTypes(data) + @"

" + SolverFileTemplate.GetGetStateVarTypesHeaderCompoundTypes(data, true) + @"
class State {
public:
	int state_id;

	State();
	State(int _state_id);
	virtual ~State();

	friend std::ostream& operator<<(std::ostream& os, const State& state);
 

	static double Weight(const std::vector<State*>& particles);

	State* operator()(int state_id) {
		this->state_id = state_id; 
		return this;
	}

    bool __isTermianl = false;
" + SolverFileTemplate.GetVariableDeclarationsForStateHeaderFile(data, true) + @"

	public:
		static void SetAnyValueLinks(State *state);
};
typedef State " + data.ProjectNameWithCapitalLetter + @"State;

State::State(){}
State::~State() {}

State* CopyToNewState(State* state)
{
    State* s= new State();
"+SolverFileTemplate.GetDeepCopyState(data)+@"
    return s;
}

State* CopyToState(State* state, State* toState)
{
    *toState=*state;
    return toState;
}

State* prevState = new State();
State* afterExtState = new State();

" + SolverFileTemplate.GetActionManagerHeaderFile(data, true) + @"

" + SolverFileTemplate.GetActionManagerCPpFile(data, out totalNumberOfActionsInProject, true) + Environment.NewLine
+ SolverFileTemplate.GetPOMDPCPPFile(data, true) + Environment.NewLine + 
SolverFileTemplate.GetProcessSpecialStatesFunction(data, true) +
SolverFileTemplate.GetModelCppCreatStartStateFunction(data, null, true) +
SolverFileTemplate.GetCheckPreconditionsForModelCpp(data, true) + Environment.NewLine +
 SolverFileTemplate.GetComputePreferredActionValueForModelCpp(data, true) + Environment.NewLine +
 SolverFileTemplate.GetSampleModuleExecutionTimeFunction(data, true) + Environment.NewLine +
 SolverFileTemplate.GetExtrinsicChangesDynamicModelFunction(data, true) + Environment.NewLine +
 SolverFileTemplate.GetModuleDynamicModelFunction(data, true) + Environment.NewLine + 
 SolverFileTemplate.GetCPPStepFunction(data, true, forCppToPython) + Environment.NewLine + 
 @"
    State* InitEnv()
    {
        generator.seed(std::chrono::system_clock::now().time_since_epoch().count());
        InitMapEnumToString();
        State* state = CreateStartState();
        return state;
    }
 ";
 if(forCppToPython)
 {
 file +=@"

std::string EnvName()
{
    return """+data.ProjectName+@""";
}

std::string DocHash()
{
    return """+data.GetModelHash()+@""";
}

std::string Horizon()
{
    return """+data.Horizon+@""";
}

 vector<std::string> PrintActionsDescription()
{
    stringstream ss;
    vector<std::string> res;
    for(int i=0; i < ActionManager::actions.size();i++)
    {
        ss << Prints::PrintActionDescription(ActionManager::actions[i]) << endl;
        res.push_back(Prints::PrintActionDescription(ActionManager::actions[i]));
    }
    //return ss.str();
    return res;
}

PYBIND11_MODULE(aos_domain, m) {
    "+ GetPyBindForTypes(data)+@"
    py::class_<State>(m, ""State"")
//    .def(py::init()) 
"+GetStateVariableBindings(data)+@"
    .def(""__repr__"", &Prints::PrintState);
    
    
    m.def(""horizon"", &Horizon);
    m.def(""hash"", &DocHash);
    m.def(""name"", &EnvName);
    m.def(""copy"", &CopyToNewState);
    m.doc() = ""pybind11 aos_domain_for_python plugin""; // optional module docstring
    m.def(""init_env"", &InitEnv, ""A function that adds two numbers"");
    m.def(""create_state"", &CreateStartState, ""create a new state that is sampled from the initial belief state"");
    m.def(""step"", &Step, ""samples the next state given the current one"");
    m.def(""print_actions"", &PrintActionsDescription, ""print actions description"");
}
";
 }
file +=Environment.NewLine + "}";

if(forCppToPython)
{
    file=file.Replace("AOSUtils::","");
}
            return file;
        }
public static string GetStateVariableBindings(PLPsData data)
{
    string result = "" + Environment.NewLine;
    for(int i=0;i< data.GlobalVariableDeclarations.Count() ; i++)
    {
        GlobalVariableDeclaration d = data.GlobalVariableDeclarations[i];
        if(d.IsActionParameterValue)
        {
            continue;
        }
        if(data.GlobalEnumTypes.Where(x=> x.TypeName == d.Type).Count()>0)
        {
            result += "    .def_property(\""+d.Name+"\", &State::get_"+d.Name+", &State::set_"+d.Name+")";
        }
        else
        {
            result += "    .def_readwrite(\""+d.Name+"\", &State::"+d.Name+")";    
        }
        result += Environment.NewLine;
    }
    return result;
}
 public static string GetEnumTypeGetSetFunction(PLPsData data, string enumType, string fieldName)
    {
        string result="";
        for(int i=0; i < data.GlobalEnumTypes.Count();i++)
        {
          if(data.GlobalEnumTypes[i].TypeName == enumType)
          {
            result += "        void set_"+fieldName+"( int c_) { "+fieldName+" = ("+enumType+")c_; }" + Environment.NewLine;
            result += "        int get_"+fieldName+"()  { return (int)"+fieldName+"; }" + Environment.NewLine;
            break;
          }  
        } 
        return result;
    }

         private static string GetPyBindForTypes(PLPsData data)
    {
        string result = "";
        for(int i=0;i< data.GlobalCompoundTypes.Count();i++)
        {
            CompoundVarTypePLP t = data.GlobalCompoundTypes[i];
            result += "    py::class_<"+t.TypeName+">(m, \""+t.TypeName+"\")" + Environment.NewLine;
            for(int j=0;j< t.Variables.Count();j++)
            {
                if(data.GlobalEnumTypes.Where(x=> x.TypeName == t.Variables[j].Type).Count()>0)
                {
                    result += "    .def_property(\""+t.Variables[j].Name+"\", &"+t.TypeName+"::get_"+t.Variables[j].Name+", &"+t.TypeName+"::set_"+t.Variables[j].Name+")";
                }
                else
                {
                    result += "    .def_readwrite(\""+t.Variables[j].Name+"\", &"+t.TypeName+"::"+t.Variables[j].Name+")";
                }
                result += j == t.Variables.Count()-1 ? ";" : "";
                result += Environment.NewLine;
            }
        }
        return result;
    }

    }

   
        
}

