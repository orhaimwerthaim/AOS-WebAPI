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
        public static string GetSingleFileModel(PLPsData data)
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

using namespace std;
typedef bool anyValue;

" + SolverFileTemplate.GetActionTypeEnum(data) + @"

" + SolverFileTemplate.GetResponseModuleAndTempEnumsList(data, out temp2, out temp1) + @"
" + SolverFileTemplate.GetGetStateVarTypesHeaderEnumTypes(data) + @"

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

" + SolverFileTemplate.GetActionManagerHeaderFile(data, true) + @"

" + SolverFileTemplate.GetActionManagerCPpFile(data, out totalNumberOfActionsInProject, true) + Environment.NewLine
+ SolverFileTemplate.GetPOMDPCPPFile(data, true);

            return file;
        }
    }
        
}