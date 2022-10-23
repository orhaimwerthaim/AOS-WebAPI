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

bool SampleBernoulli(double p)
{
    float rand = real_unfirom_dist(generator); 
	
	return rand <= p;
}

" + SolverFileTemplate.GetEnumMapHeaderFile(data, out temp1, true) + 
    SolverFileTemplate.GetEnumMapCppFile(data,true) + Environment.NewLine 
  + SolverFileTemplate.GetGetStateVarTypesHeaderEnumTypes(data) + @"

" + SolverFileTemplate.GetGetStateVarTypesHeaderCompoundTypes(data) + @"
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
" + SolverFileTemplate.GetVariableDeclarationsForStateHeaderFile(data) + @"

	public:
		static void SetAnyValueLinks(State *state);
};
typedef State " + data.ProjectNameWithCapitalLetter + @"State;

State::State(){}
State::~State() {}

State* CopyToNewState(State* state)
{
    State* s= new State();
    *s=*state;
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
        InitMapEnumToString();
        State* state = CreateStartState();
        return state;
    }
 ";
 if(forCppToPython)
 {
 file +=@"
PYBIND11_MODULE(aos_domain, m) {
    
    py::class_<State>(m, ""State"")
//    .def(py::init()) 
    .def(""__repr__"", &Prints::PrintState);
    
    m.def(""copy"", &CopyToNewState);
    m.doc() = ""pybind11 aos_domain_for_python plugin""; // optional module docstring
    m.def(""init_env"", &InitEnv, ""A function that adds two numbers"");
    m.def(""create_state"", &CreateStartState, ""create a new state that is sampled from the initial belief state"");
    m.def(""step"", &Step, ""samples the next state given the current one"");
}
";
 }
file +=Environment.NewLine + "}";

            return file;
        }
    }
        
}