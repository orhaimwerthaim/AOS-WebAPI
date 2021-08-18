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
    public class SolverFileTemplate
    {
        public static Dictionary<string, Dictionary<string, string>> EnumMappingsForModuleResponseAndTempVar = null;
        #region cmake
        public static string GetProjectExample_MakeFile()
        {
            return @"# -----------------------
# Compiler/linker options
# -----------------------

CXX = g++
LDFLAGS = -O3 -Wall -Wno-sign-compare $(GPROF)

# -----------
# Directories
# -----------

DESPOTHOME = ../../..
SRCDIR = src
INCDIR = $(DESPOTHOME)/include
LIBDIR = $(DESPOTHOME)/build

# -----
# Files
# -----

SOURCES = $(shell find -L $(SRCDIR) -name '*.cpp')
BIN = pocman 

# -------
# Targets
# -------

.PHONY: all clean 

all:
	@cd $(DESPOTHOME) && make
	$(CXX) $(LDFLAGS) $(SOURCES) -I $(INCDIR) $(LIBDIR)/*.o -o $(BIN)

light:
	@cd $(DESPOTHOME) && make
	$(CXX) $(LDFLAGS) $(SOURCES) -I $(INCDIR) -L $(LIBDIR) -ldespot -o $(BIN)

clean:
	rm -f $(BIN)
";
        }

        public static string GetProjectExample_CMakeLists(string projectName)
        {
            string file = @"cmake_minimum_required(VERSION 2.8.3)

add_executable(""${PROJECT_NAME}_" + projectName + @"""
#src/state_var_types.cpp   
src/" + projectName + @".cpp
src/main.cpp
#src/mongoDB_Bridge.cpp
)



##############
#added for mongo DB client
find_package(libmongocxx REQUIRED)
find_package(libbsoncxx REQUIRED)
include_directories(${LIBMONGOCXX_INCLUDE_DIR})
include_directories(${LIBBSONCXX_INCLUDE_DIR})
include_directories(""/usr/local/include/mongocxx/v_noabi"")
include_directories(""/usr/local/include/bsoncxx/v_noabi"")
include_directories(""/usr/local/include/libmongoc-1.0"")
include_directories(""/usr/local/include/libbson-1.0"")
include_directories(""/usr/local/lib"")
 

target_link_libraries(""${PROJECT_NAME}_" + projectName + @""" ${LIBMONGOCXX_LIBRARIES})
target_link_libraries(""${PROJECT_NAME}_" + projectName + @""" ${LIBBSONCXX_LIBRARIES})
##############


target_link_libraries(""${PROJECT_NAME}_" + projectName + @"""
  ""${PROJECT_NAME}""
)
install(TARGETS ""${PROJECT_NAME}_" + projectName + @"""
  RUNTIME DESTINATION ""${BINARY_INSTALL_PATH}""
)";
            return file;
        }

        public static string GetBasePath_CMakeLists(string projectName)
        {
            string file = @"cmake_minimum_required(VERSION 2.8.3)
project(despot)

########################################
#to work with cpp 17
set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
set(CMAKE_CXX_EXTENSIONS OFF)
#########################################


set(BINARY_INSTALL_PATH ""bin"" CACHE PATH ""Binary install path"")
set(LIBRARY_INSTALL_PATH ""lib"" CACHE PATH ""Library install path"")
set(INCLUDE_INSTALL_PATH ""include"" CACHE PATH ""Include install path"")
set(CONFIG_INSTALL_PATH ""${LIBRARY_INSTALL_PATH}/${PROJECT_NAME}/cmake"")

set(DESPOT_BUILD_EXAMPLES ON CACHE BOOL ""Build C++ model examples"")
set(DESPOT_BUILD_POMDPX ON CACHE BOOL ""Build POMDPX example"")

set(CMAKE_CXX_FLAGS ""${CMAKE_CXX_FLAGS} -msse2 -mfpmath=sse"")
set(CMAKE_MODULE_PATH ${CMAKE_PREFIX_PATH} ""${PROJECT_SOURCE_DIR}/cmake"")

include_directories(include)

##################################
#adding json
set(JSON_BuildTests OFF CACHE INTERNAL """")
add_subdirectory(nlohmann_json) 
#add_library(""${PROJECT_NAME}"" SHARED
#src/model_primitives/" + projectName + @"/actionManager.cpp #added
#)
#target_link_libraries(""${PROJECT_NAME}"" PRIVATE nlohmann_json::nlohmann_json)
##################################



add_library(""${PROJECT_NAME}"" SHARED
  src/model_primitives/" + projectName + @"/state.cpp
  src/model_primitives/" + projectName + @"/enum_map_" + projectName + @".cpp #added
  src/model_primitives/" + projectName + @"/actionManager.cpp #added

  src/core/belief.cpp
  src/core/globals.cpp
  src/core/lower_bound.cpp
  src/core/mdp.cpp
  src/core/node.cpp
  src/core/policy.cpp
  src/core/pomdp.cpp
  src/core/solver.cpp
  src/core/upper_bound.cpp
  
  src/evaluator.cpp
  src/pomdpx/parser/function.cpp
  src/pomdpx/parser/parser.cpp
  src/pomdpx/parser/variable.cpp
  src/pomdpx/pomdpx.cpp
  src/random_streams.cpp
  src/simple_tui.cpp
  src/solver/aems.cpp
  src/solver/despot.cpp
  src/solver/pomcp.cpp
  src/util/coord.cpp
  src/util/mongoDB_Bridge.cpp
  src/util/dirichlet.cpp
  src/util/exec_tracker.cpp
  src/util/floor.cpp
  src/util/gamma.cpp
  src/util/logging.cpp
  src/util/random.cpp
  src/util/seeds.cpp
  src/util/util.cpp
  src/util/tinyxml/tinystr.cpp
  src/util/tinyxml/tinyxml.cpp
  src/util/tinyxml/tinyxmlerror.cpp
  src/util/tinyxml/tinyxmlparser.cpp
)
target_link_libraries(""${PROJECT_NAME}""
  ${TinyXML_LIBRARIES}
  PRIVATE nlohmann_json::nlohmann_json #adding json
)


##############
#added for mongo DB client
find_package(libmongocxx REQUIRED)
find_package(libbsoncxx REQUIRED)
include_directories(${LIBMONGOCXX_INCLUDE_DIR})
include_directories(${LIBBSONCXX_INCLUDE_DIR})
include_directories(""/usr/local/include/mongocxx/v_noabi"")
include_directories(""/usr/local/include/bsoncxx/v_noabi"")
include_directories(""/usr/local/include/libmongoc-1.0"")
include_directories(""/usr/local/include/libbson-1.0"")
include_directories(""/usr/local/lib"")
 

target_link_libraries(""${PROJECT_NAME}"" ${LIBMONGOCXX_LIBRARIES})
target_link_libraries(""${PROJECT_NAME}"" ${LIBBSONCXX_LIBRARIES})
##############

# Build example files
if(DESPOT_BUILD_EXAMPLES)
  # add_subdirectory(examples/cpp_models/adventurer)
  # add_subdirectory(examples/cpp_models/bridge)
  # add_subdirectory(examples/cpp_models/chain)
  # add_subdirectory(examples/cpp_models/navigation)
  # add_subdirectory(examples/cpp_models/pocman)
  # add_subdirectory(examples/cpp_models/robocup_cleanroom)
  # add_subdirectory(examples/cpp_models/robocup_findmates)
  # add_subdirectory(examples/cpp_models/temp)
  # add_subdirectory(examples/cpp_models/reg_demo)
  # add_subdirectory(examples/cpp_models/rock_sample)
  # add_subdirectory(examples/cpp_models/simple_rock_sample)
  # add_subdirectory(examples/cpp_models/tag)
  # add_subdirectory(examples/cpp_models/tiger)


  add_subdirectory(examples/cpp_models/" + projectName + @")
 # add_subdirectory(examples/cpp_models/" + projectName + @"_working_toy)
endif()

if(DESPOT_BUILD_POMDPX)
  add_subdirectory(examples/pomdpx_models)
endif()

install(TARGETS ""${PROJECT_NAME}""
  EXPORT ""DespotTargets""
  ARCHIVE DESTINATION ""${LIBRARY_INSTALL_PATH}""
  LIBRARY DESTINATION ""${LIBRARY_INSTALL_PATH}""
  RUNTIME DESTINATION ""${BINARY_INSTALL_PATH}""
)
install(DIRECTORY ""include/${PROJECT_NAME}/""
  DESTINATION ""${INCLUDE_INSTALL_PATH}/${PROJECT_NAME}""
)

# Install a DespotConfig.cmake file so CMake can find_package(Despot).
include(CMakePackageConfigHelpers)
configure_package_config_file(""cmake/DespotConfig.cmake.in""
  ""${CMAKE_CURRENT_BINARY_DIR}/DespotConfig.cmake""
  INSTALL_DESTINATION ""${CONFIG_INSTALL_PATH}""
  PATH_VARS INCLUDE_INSTALL_PATH
)

install(EXPORT ""DespotTargets""
  FILE ""DespotTargets.cmake""
  DESTINATION ""${CONFIG_INSTALL_PATH}""
)
install(FILES ""${CMAKE_CURRENT_BINARY_DIR}/DespotConfig.cmake""
  DESTINATION ""${CONFIG_INSTALL_PATH}""
)

";
            return file;
        }
        #endregion


        public static string GetConfigHeaderFile(PLPsData data, InitializeProject initProj)
        {
            string file = @"#ifndef CONFIG_H
#define CONFIG_H

#include <string>

namespace despot {

struct Config {
	bool internalSimulation; 
	int search_depth;
	double discount;
	unsigned int root_seed;
	double time_per_move;  // CPU time available to construct the search tree
	int num_scenarios;
	double pruning_constant;
	double xi; // xi * gap(root) is the target uncertainty at the root.
	int sim_len; // Number of steps to run the simulation for.
  std::string default_action;
	int max_policy_sim_len; // Maximum number of steps for simulating the default policy
	double noise;
	bool silence;

	Config() :
		search_depth(" + data.Horizon + @"),
		discount(" + data.Horizon + @"),
		root_seed(42),
		time_per_move(" + initProj.SolverConfiguration.PlanningTimePerMoveInSeconds + @"),
		num_scenarios(500),
		pruning_constant(0),
		xi(0.95),
		sim_len(90),
		default_action(""""),
		max_policy_sim_len(10),
		noise(0.1),
		silence(false),
		internalSimulation(" + initProj.SolverConfiguration.IsInternalSimulation.ToString().ToLower() + @")
		{
		
	}
};

} // namespace despot

#endif
";
            return file;
        }


        public static string GetPOMCP_File(string debugPDF_Path, int debugPDF_Depth)
        {
            string file = @"#include <despot/solver/pomcp.h>
#include <despot/util/logging.h>
#include <iostream>
#include <fstream>

using namespace std;

using namespace std;

namespace despot {

/* =============================================================================
 * POMCPPrior class
 * =============================================================================*/

POMCPPrior::POMCPPrior(const DSPOMDP* model) :
	model_(model) {
	exploration_constant_ = (model->GetMaxReward()
		- model->GetMinRewardAction().value);
}

POMCPPrior::~POMCPPrior() {
}

const vector<int>& POMCPPrior::preferred_actions() const {
	return preferred_actions_;
}

const vector<int>& POMCPPrior::legal_actions() const {
	return legal_actions_;
}

int POMCPPrior::GetAction(const State& state) {
	ComputePreference(state);

	if (preferred_actions_.size() != 0)
		return Random::RANDOM.NextElement(preferred_actions_);

	if (legal_actions_.size() != 0)
		return Random::RANDOM.NextElement(legal_actions_);

	return Random::RANDOM.NextInt(model_->NumActions());
}

/* =============================================================================
 * UniformPOMCPPrior class
 * =============================================================================*/

UniformPOMCPPrior::UniformPOMCPPrior(const DSPOMDP* model) :
	POMCPPrior(model) {
}

UniformPOMCPPrior::~UniformPOMCPPrior() {
}

void UniformPOMCPPrior::ComputePreference(const State& state) {
}

/* =============================================================================
 * POMCP class
 * =============================================================================*/

POMCP::POMCP(const DSPOMDP* model, POMCPPrior* prior, Belief* belief) :
	Solver(model, belief),
	root_(NULL) {
	reuse_ = false;
	prior_ = prior;
	assert(prior_ != NULL);
}

void POMCP::reuse(bool r) {
	reuse_ = r;
}

ValuedAction POMCP::Search(double timeout) {
	double start_cpu = clock(), start_real = get_time_second();

	if (root_ == NULL) {
		State* state = belief_->Sample(1)[0];
		root_ = CreateVNode(0, state, prior_, model_);
		model_->Free(state);
	}
	 
	static const int actArr[] = {};
	//static const int actArr[] = {0,4,1,5};//when an action sequence is required to simulate
	
	std::vector<int> actionSeq (actArr, actArr + sizeof(actArr) / sizeof(actArr[0]) );
	 
	std::vector<int>*	simulatedActionSequence = actionSeq.size() > 0 ? &actionSeq : NULL;	 
	

	int hist_size = history_.Size();
	bool done = false;
	int num_sims = 0;
	while (true) {
		vector<State*> particles = belief_->Sample(1000);
		for (int i = 0; i < particles.size(); i++) {
			State* particle = particles[i];
			logd << ""[POMCP::Search] Starting simulation "" << num_sims << endl;

			Simulate(particle, root_, model_, prior_, simulatedActionSequence);
 
			num_sims++;
			logd << ""[POMCP::Search] "" << num_sims << "" simulations done"" << endl;
			history_.Truncate(hist_size);

			if ((clock() - start_cpu) / CLOCKS_PER_SEC >= timeout) {
				done = true;
				break;
			}
		}

		for (int i = 0; i < particles.size(); i++) {
			model_->Free(particles[i]);
		}

		if (done)
			break;
	}

	ValuedAction astar = OptimalAction(root_);
	
	
	//TODO::remove only for debug
	double explore_constant = prior_->exploration_constant();
	std::cout << ""--------------------------------------------------------------SEARCH-ACTION--END---------------------------------------------------------------"" << endl;
	//model_->PrintState(*belief_->Sample(1)[0]);
	int action = UpperBoundAction(root_, explore_constant, model_, belief_);
	//untill here
	
	
	logi << ""[POMCP::Search] Search statistics"" << endl
		<< ""OptimalAction = "" << astar << endl 
		<< ""# Simulations = "" << root_->count() << endl
		<< ""Time: CPU / Real = "" << ((clock() - start_cpu) / CLOCKS_PER_SEC) << "" / "" << (get_time_second() - start_real) << endl
		<< ""# active particles = "" << model_->NumActiveParticles() << endl
		<< ""Tree size = "" << root_->Size() << endl;

	if (astar.action == -1) {
		for (int action = 0; action < model_->NumActions(); action++) {
			cout << ""action "" << action << "": "" << root_->Child(action)->count()
				<< "" "" << root_->Child(action)->value() << endl;
		}
	}

	std::string dot = POMCP::GenerateDotGraph(root_," + debugPDF_Depth + @", model_);
	// delete root_;
	return astar;
}



ValuedAction POMCP::Search() {
	return Search(Globals::config.time_per_move);
}

void POMCP::belief(Belief* b) {
	belief_ = b;
	history_.Truncate(0);
  prior_->PopAll();
	delete root_;
	root_ = NULL;
}


void POMCP::Update(int action, OBS_TYPE obs, std::map<std::string, bool> updatesFromAction)
{
	double start = get_time_second();

	if (reuse_) {
		VNode* node = root_->Child(action)->Child(obs);
		root_->Child(action)->children().erase(obs);
		delete root_;

		root_ = node;
		if (root_ != NULL) {
			root_->parent(NULL);
		}
	} else {
		delete root_;
		root_ = NULL;
	}

	prior_->Add(action, obs);
	history_.Add(action, obs);
	belief_->Update(action, obs, updatesFromAction);

	logi << ""[POMCP::Update] Updated belief, history and root with action ""
		<< action << "", observation "" << obs
		<< "" in "" << (get_time_second() - start) << ""s"" << endl;
}
void POMCP::Update(int action, OBS_TYPE obs) {
	std::map<std::string, bool> updatesFromAction;
	POMCP::Update(action, obs, updatesFromAction);
}

int POMCP::UpperBoundAction(const VNode* vnode, double explore_constant)
{
	return UpperBoundAction(vnode, explore_constant, NULL, NULL);
}

int POMCP::UpperBoundAction(const VNode* vnode, double explore_constant, const DSPOMDP* model, Belief* belief) {
	const vector<QNode*>& qnodes = vnode->children();
	double best_ub = Globals::NEG_INFTY;
	int best_action = -1;

	/*
	 int total = 0;
	 for (int action = 0; action < qnodes.size(); action ++) {
	 total += qnodes[action]->count();
	 double ub = qnodes[action]->value() + explore_constant * sqrt(log(vnode->count() + 1) / qnodes[action]->count());
	 cout << action << "" "" << ub << "" "" << qnodes[action]->value() << "" "" << qnodes[action]->count() << "" "" << vnode->count() << endl;
	 }
	 */
	//TODO:: activate line below only on debug mode:
	if(model)
		logi << model->PrintStateStr(*belief->Sample(1)[0]);

	for (int action = 0; action < qnodes.size(); action++) {
		if (qnodes[action]->count() == 0)
			return action;
		
		//TODO::remove 
		//std::string s= model->GetActionDescription(action);
		
		
		double ub = qnodes[action]->value()
			+ explore_constant
				* sqrt(log(vnode->count() + 1) / qnodes[action]->count());

		if (ub > best_ub) {
			best_ub = ub;
			best_action = action;
		}
		//logi << ""[POMCP::UpperBoundAction]:Depth:"" << vnode->depth() << ""Action:""<< action <<"",N:"" << vnode->count() << "",V:"" << vnode->value() << endl;
		if (vnode->depth() < 1 && model)
			logi << ""[POMCP::UpperBoundAction]:Depth:""<< vnode->depth() <<"",N:"" << vnode->count() <<"",V:"" << vnode->value() << model->GetActionDescription(action) << "",UCB:""<< ub<< endl;   

			// if(model)
			// logd << ""[POMCP::UpperBoundAction]: Best Action is: ""<< model->GetActionDescription(best_action) << ""|With value:""<<best_ub <<endl;	
	}
	
	assert(best_action != -1);
	if(model)
		logi << ""[POMCP::UpperBoundAction]:Selected Action:""<< model->GetActionDescription(best_action) <<endl;
	return best_action;
}

ValuedAction POMCP::OptimalAction(const VNode* vnode) {
	const vector<QNode*>& qnodes = vnode->children();
	ValuedAction astar(-1, Globals::NEG_INFTY);
	for (int action = 0; action < qnodes.size(); action++) {
		// cout << action << "" "" << qnodes[action]->value() << "" "" << qnodes[action]->count() << "" "" << vnode->count() << endl;
		if (qnodes[action]->value() > astar.value) {
			astar = ValuedAction(action, qnodes[action]->value());
		}
	}
	// assert(atar.action != -1);
	return astar;
}

int POMCP::Count(const VNode* vnode) {
	int count = 0;
	for (int action = 0; action < vnode->children().size(); action++)
		count += vnode->Child(action)->count();
	return count;
}

VNode* POMCP::CreateVNode(int depth, const State* state, POMCPPrior* prior,
	const DSPOMDP* model) {
	VNode* vnode = new VNode(0, 0.0, depth);

	prior->ComputePreference(*state);

	const vector<int>& preferred_actions = prior->preferred_actions();
	const vector<int>& legal_actions = prior->legal_actions();

	int large_count = 1000000;
	double neg_infty = -1e10;

	if (legal_actions.size() == 0) { // no prior knowledge, all actions are equal
		for (int action = 0; action < model->NumActions(); action++) {
			QNode* qnode = new QNode(vnode, action);
			qnode->count(0);
			qnode->value(0);

			vnode->children().push_back(qnode);
		}
	} else {
		for (int action = 0; action < model->NumActions(); action++) {
			QNode* qnode = new QNode(vnode, action);
			qnode->count(large_count);
			qnode->value(neg_infty);

			vnode->children().push_back(qnode);
		}

		for (int a = 0; a < legal_actions.size(); a++) {
			QNode* qnode = vnode->Child(legal_actions[a]);
			qnode->count(0);
			qnode->value(0);
		}

		for (int a = 0; a < preferred_actions.size(); a++) {
			int action = preferred_actions[a];
			QNode* qnode = vnode->Child(action);
			qnode->count(prior->SmartCount(action));
			qnode->value(prior->SmartValue(action));
		}
	}

	return vnode;
}

double POMCP::Simulate(State* particle, RandomStreams& streams, VNode* vnode,
	const DSPOMDP* model, POMCPPrior* prior) {
	if (streams.Exhausted())
		return 0;

	double explore_constant = prior->exploration_constant();

	int action = POMCP::UpperBoundAction(vnode, explore_constant);
	logd << *particle << endl;
	logd << ""depth = "" << vnode->depth() << ""; action = "" << action << ""; ""
		<< particle->scenario_id << endl;

	double reward;
	OBS_TYPE obs;
	bool terminal = model->Step(*particle, streams.Entry(particle->scenario_id),
		action, reward, obs);

	QNode* qnode = vnode->Child(action);
	if (!terminal) {
		prior->Add(action, obs);
		streams.Advance();
		map<OBS_TYPE, VNode*>& vnodes = qnode->children();
		if (vnodes[obs] != NULL) {
			reward += Globals::Discount()
				* Simulate(particle, streams, vnodes[obs], model, prior);
		} else { // Rollout upon encountering a node not in curren tree, then add the node
			reward += Globals::Discount() 
        * Rollout(particle, streams, vnode->depth() + 1, model, prior);
			vnodes[obs] = CreateVNode(vnode->depth() + 1, particle, prior,
				model);
		}
		streams.Back();
		prior->PopLast();
	}

	qnode->Add(reward);
	vnode->Add(reward);

	return reward;
}

// static
double POMCP::Simulate(State* particle, VNode* vnode, const DSPOMDP* model,
	POMCPPrior* prior, std::vector<int>* simulateActionSequence) {
	assert(vnode != NULL);
	if (vnode->depth() >= Globals::config.search_depth)
		return 0;

	double explore_constant = prior->exploration_constant();

	int action = simulateActionSequence && simulateActionSequence->size() > vnode->depth() ? (*simulateActionSequence)[vnode->depth()] : UpperBoundAction(vnode, explore_constant);
			
	double reward;
	OBS_TYPE obs;
	bool terminal = model->Step(*particle, action, reward, obs);

	QNode* qnode = vnode->Child(action);
	if (!terminal) {
		prior->Add(action, obs);
		map<OBS_TYPE, VNode*>& vnodes = qnode->children();
		if (vnodes[obs] != NULL) {
			reward += Globals::Discount()
				* Simulate(particle, vnodes[obs], model, prior,simulateActionSequence);
		} else { // Rollout upon encountering a node not in curren tree, then add the node
			vnodes[obs] = CreateVNode(vnode->depth() + 1, particle, prior,
				model);
			reward += Globals::Discount()
				* Rollout(particle, vnode->depth() + 1, model, prior,simulateActionSequence);
		}
		prior->PopLast();
	}

	qnode->Add(reward);
	vnode->Add(reward);

	return reward;
}

// static
double POMCP::Rollout(State* particle, RandomStreams& streams, int depth,
	const DSPOMDP* model, POMCPPrior* prior) {
	if (streams.Exhausted()) {
		return 0;
	}

	int action = prior->GetAction(*particle);

	logd << *particle << endl;
	logd << ""depth = "" << depth << ""; action = "" << action << endl;

	double reward;
	OBS_TYPE obs;
	bool terminal = model->Step(*particle, streams.Entry(particle->scenario_id),
		action, reward, obs);
	if (!terminal) {
		prior->Add(action, obs);
		streams.Advance();
		reward += Globals::Discount()
			* Rollout(particle, streams, depth + 1, model, prior);
		streams.Back();
		prior->PopLast();
	}

	return reward;
}

// static
double POMCP::Rollout(State* particle, int depth, const DSPOMDP* model,
	POMCPPrior* prior, std::vector<int>* simulateActionSequence) {
	if (depth >= Globals::config.search_depth) {
		return 0;
	}

	//int action = prior->GetAction(*particle);
	int action = simulateActionSequence && simulateActionSequence->size() > depth ? (*simulateActionSequence)[depth] : prior->GetAction(*particle);
	
	double reward;
	OBS_TYPE obs;
	bool terminal = model->Step(*particle, action, reward, obs);
	if (!terminal) {
		prior->Add(action, obs);
		reward += Globals::Discount() * Rollout(particle, depth + 1, model, prior,simulateActionSequence);
		prior->PopLast();
	}

	return reward;
}

ValuedAction POMCP::Evaluate(VNode* root, vector<State*>& particles,
	RandomStreams& streams, const DSPOMDP* model, POMCPPrior* prior) {
	double value = 0;

	for (int i = 0; i < particles.size(); i++)
		particles[i]->scenario_id = i;

	for (int i = 0; i < particles.size(); i++) {
		State* particle = particles[i];
		VNode* cur = root;
		State* copy = model->Copy(particle);
		double discount = 1.0;
		double val = 0;
		int steps = 0;

		// Simulate until all random numbers are consumed
		while (!streams.Exhausted()) {
			int action =
				(cur != NULL) ?
					UpperBoundAction(cur, 0) : prior->GetAction(*particle);

			double reward;
			OBS_TYPE obs;
			bool terminal = model->Step(*copy, streams.Entry(copy->scenario_id),
				action, reward, obs);

			val += discount * reward;
			discount *= Globals::Discount();

			if (!terminal) {
				prior->Add(action, obs);
				streams.Advance();
				steps++;

				if (cur != NULL) {
					QNode* qnode = cur->Child(action);
					map<OBS_TYPE, VNode*>& vnodes = qnode->children();
					cur = vnodes.find(obs) != vnodes.end() ? vnodes[obs] : NULL;
				}
			} else {
				break;
			}
		}

		// Reset random streams and prior
		for (int i = 0; i < steps; i++) {
			streams.Back();
			prior->PopLast();
		}

		model->Free(copy);

		value += val;
	}

	return ValuedAction(UpperBoundAction(root, 0), value / particles.size());
}

/* =============================================================================
 * DPOMCP class
 * =============================================================================*/

DPOMCP::DPOMCP(const DSPOMDP* model, POMCPPrior* prior, Belief* belief) :
	POMCP(model, prior, belief) {
	reuse_ = false;
}

void DPOMCP::belief(Belief* b) {
	belief_ = b;
	history_.Truncate(0);
  prior_->PopAll();
}

ValuedAction DPOMCP::Search(double timeout) {
	double start_cpu = clock(), start_real = get_time_second();

	vector<State*> particles = belief_->Sample(Globals::config.num_scenarios);

	RandomStreams streams(Globals::config.num_scenarios,
		Globals::config.search_depth);

	root_ = ConstructTree(particles, streams, model_, prior_, history_,
		timeout);

	for (int i = 0; i < particles.size(); i++)
		model_->Free(particles[i]);

	logi << ""[DPOMCP::Search] Time: CPU / Real = ""
		<< ((clock() - start_cpu) / CLOCKS_PER_SEC) << "" / ""
		<< (get_time_second() - start_real) << endl << ""Tree size = ""
		<< root_->Size() << endl;

	ValuedAction astar = OptimalAction(root_);
	if (astar.action == -1) {
		for (int action = 0; action < model_->NumActions(); action++) {
			cout << ""action "" << action << "": "" << root_->Child(action)->count()
				<< "" "" << root_->Child(action)->value() << endl;
		}
	}

	delete root_;
	return astar;
}

// static
VNode* DPOMCP::ConstructTree(vector<State*>& particles, RandomStreams& streams,
	const DSPOMDP* model, POMCPPrior* prior, History& history, double timeout) {
	prior->history(history);
	VNode* root = CreateVNode(0, particles[0], prior, model);

	for (int i = 0; i < particles.size(); i++)
		particles[i]->scenario_id = i;

	logi << ""[DPOMCP::ConstructTree] # active particles before search = ""
		<< model->NumActiveParticles() << endl;
	double start = clock();
	int num_sims = 0;
	while (true) {
		logd << ""Simulation "" << num_sims << endl;

		int index = Random::RANDOM.NextInt(particles.size());
		State* particle = model->Copy(particles[index]);
		Simulate(particle, streams, root, model, prior);
		num_sims++;
		model->Free(particle);

		if ((clock() - start) / CLOCKS_PER_SEC >= timeout) {
			break;
		}
	}

	logi << ""[DPOMCP::ConstructTree] OptimalAction = "" << OptimalAction(root)
		<< endl << ""# Simulations = "" << root->count() << endl
		<< ""# active particles after search = "" << model->NumActiveParticles()
		<< endl;

	return root;
}

void DPOMCP::Update(int action, OBS_TYPE obs) {
	double start = get_time_second();

	history_.Add(action, obs);
	belief_->Update(action, obs);

	logi << ""[DPOMCP::Update] Updated belief, history and root with action ""
		<< action << "", observation "" << obs
		<< "" in "" << (get_time_second() - start) << ""s"" << endl;
}

std::string POMCP::GenerateDotGraph(VNode* root, int depthLimit, const DSPOMDP* model)
{
	stringstream ssNodes;
	stringstream ssEdges; 
	
	ssNodes << ""digraph plan {"" << endl;
	ssEdges << """";
	int currentNodeID = 0;
	POMCP::GenerateDotGraphVnode(root, currentNodeID, ssNodes, ssEdges, depthLimit, model_);

	ssNodes << ssEdges.str() << ""}"" << endl;

	ofstream MyFile(""" + debugPDF_Path + @"/debug.dot"");
	MyFile << ssNodes.str();
	MyFile.close();
	//run: ""dot -Tpdf  debug.dot > debug.pdf""
	system(""(cd " + debugPDF_Path + @";dot -Tpdf  debug.dot > debug.pdf)"");
	return ssNodes.str();
}

void POMCP::GenerateDotGraphVnode(VNode* vnode, int& currentNodeID, stringstream &ssNodes, stringstream &ssEdges, int depthLimit, const DSPOMDP* model)
{ 
	
	int _nodeID = currentNodeID;

	std::string stateDesc = """";
	try 
	{
		if(vnode->belief() != NULL)
			std::vector<State *> vs = vnode->belief()->Sample(1);
		//stateDesc = model_->PrintStateStr(*());
	} 
	catch (std::exception& e)
	{ 
		stateDesc = """";
		
		/* */ }
	// if(vnode->particles().size() == 0)
	// 	stateDesc = """";
	// else 
	// 	stateDesc = model_->PrintStateStr(*(vnode->particles()[0]));
	// ssNodes << _nodeID << ""[label=\""belief[0]:"" << stateDesc << ""\"",style=filled,fillcolor=black,fontcolor=white];"" << endl;

	if(depthLimit >= 0 && vnode->depth() >= depthLimit)
	{
		return;
	}

	for (int i = 0; i < vnode->children().size(); i++)
	{
		QNode *child = vnode->children()[i];
		int childId = ++currentNodeID;
		int N = child->count();
		std::string childNodeColor = N < 10000 ? ""blue"" : ""purple"";
		double V = child->value();
		int action = i;
		ssNodes << childId << ""[label=\""action:"" << model->GetActionDescription(action) << "",""
				<< ""N:"" << N << "", V:"" << V << ""\"",style=filled,fillcolor=""<< childNodeColor <<"",fontcolor=white];"" << endl;
		ssEdges << ""\"""" << _nodeID << ""\"" -> \"""" << childId << ""\"" [ label=\""\"" , penwidth=2, color=\""black\""]"" << endl;

		for (std::map<OBS_TYPE, VNode *>::iterator it = child->children().begin(); it != child->children().end(); ++it)
		{
			OBS_TYPE obs = it->first;
			VNode *vnodeChild = it->second;
			int vnodeChildId = ++currentNodeID;
			ssEdges << ""\"""" << childId << ""\"" -> \"""" << vnodeChildId << ""\"" [ label=\""""
					<< ""Observation:"" << model_->PrintObs(action, obs) << ""\"" , penwidth=2, color=\""black\""]"" << endl;
			POMCP::GenerateDotGraphVnode(vnodeChild, currentNodeID, ssNodes, ssEdges, depthLimit, model);
		}
	}
}
 
} // namespace despot
";
            return file;
        }


        public static string GetSimpleTuiCppFile(PLPsData data)
        {
            string file = @"#include <despot/simple_tui.h>
#include <despot/model_primitives/" + data.ProjectName + @"/enum_map_" + data.ProjectName + @".h> 
#include <despot/model_primitives/" + data.ProjectName + @"/actionManager.h> 
using namespace std;

namespace despot {

void disableBufferedIO(void) {
  setbuf(stdout, NULL);
  setbuf(stdin, NULL);
  setbuf(stderr, NULL);
  setvbuf(stdout, NULL, _IONBF, 0);
  setvbuf(stdin, NULL, _IONBF, 0);
  setvbuf(stderr, NULL, _IONBF, 0);
}

option::Descriptor* buildUsage(string lower_bounds_str, 
		string base_lower_bounds_str, 
		string upper_bounds_str, 
		string base_upper_bounds_str)
{
	static string lb_str = ""-l <arg>  \t--lbtype <arg>  \tLower bound strategy ("" + lower_bounds_str + "")."";
	static string blb_str = ""  \t--blbtype <arg>  \tBase lower bound ("" + base_lower_bounds_str + "")."";
	static string ub_str = ""-u <arg>  \t--ubtype <arg>  \tUpper bound strategy ("" + upper_bounds_str + "")."";
	static string bub_str = ""  \t--bubtype <arg>  \tBase upper bound ("" + base_upper_bounds_str + "")."";
	// option::Arg::Required is a misnomer. The program won't complain if these
	// are absent, and required flags must be checked manually.
	static option::Descriptor usage[] = {
		{ E_HELP, 0, """", ""help"", option::Arg::None,
			""  \t--help\tPrint usage and exit."" },
		{ E_PARAMS_FILE, 0, ""m"", ""model-params"", option::Arg::Required,
			""-m <arg>  \t--model-params <arg>  \tPath to model-parameters file, if ""
			""any."" },
		{ E_SIZE, 0, """", ""size"", option::Arg::Required,
			""  \t--size <arg>  \tSize of a problem (problem specific)."" },
		{ E_NUMBER, 0, """", ""number"", option::Arg::Required,
			""  \t--number <arg>  \tNumber of elements of a problem (problem ""
			""specific)."" },
		{ E_DEPTH, 0, ""d"", ""depth"", option::Arg::Required,
			""-d <arg>  \t--depth <arg>  \tMaximum depth of search tree (default 90)."" },
		{ E_DISCOUNT, 0, ""g"", ""discount"", option::Arg::Required,
			""-g <arg>  \t--discount <arg>  \tDiscount factor (default 0.95)."" },
		{ E_TIMEOUT, 0, ""t"", ""timeout"", option::Arg::Required,
			""-t <arg>  \t--timeout <arg>  \tSearch time per move, in seconds (default ""
			""1)."" },
		{ E_NUMPARTICLES, 0, ""n"", ""nparticles"", option::Arg::Required,
			""-n <arg>  \t--nparticles <arg>  \tNumber of particles (default 500)."" },
		{ E_PRUNE, 0, ""p"", ""prune"", option::Arg::Required,
			""-p <arg>  \t--prune <arg>  \tPruning constant (default no pruning)."" },
		{ E_GAP, 0, """", ""xi"", option::Arg::Required,
			""  \t--xi <arg>  \tGap constant (default to 0.95)."" },
		{ E_MAX_POLICY_SIM_LEN, 0, """", ""max-policy-simlen"", option::Arg::Required,
			""  \t--max-policy-simlen <arg>  \tDepth to simulate the default policy ""
			""until. (default 90)."" },

		{ E_SEED, 0, ""r"", ""seed"", option::Arg::Required,
			""-r <arg>  \t--seed <arg>  \tRandom number seed (default is random)."" },
		{ E_SIM_LEN, 0, ""s"", ""simlen"", option::Arg::Required,
			""-s <arg>  \t--simlen <arg>  \tNumber of steps to simulate. (default 90; 0 ""
			""= infinite)."" },
		{ E_RUNS, 0, """", ""runs"", option::Arg::Required,
			""  \t--runs <arg>  \tNumber of runs. (default 1)."" },
		{ E_LBTYPE, 0, ""l"", ""lbtype"", option::Arg::Required, lb_str.c_str()},
		{ E_BLBTYPE, 0, """", ""blbtype"", option::Arg::Required, blb_str.c_str()},	
		{ E_UBTYPE, 0, ""u"", ""ubtype"", option::Arg::Required, ub_str.c_str()},
		{ E_BUBTYPE, 0, """", ""bubtype"", option::Arg::Required, bub_str.c_str()},
		{ E_BELIEF, 0, ""b"", ""belief"", option::Arg::Required,
			""-b <arg>  \t--belief <arg>  \tBelief update strategy, if applicable."" },
		{ E_NOISE, 0, """", ""noise"", option::Arg::Required,
			""  \t--noise <arg>  \tNoise level for transition in POMDPX belief ""
			""update."" },

		{ E_VERBOSITY, 0, ""v"", ""verbosity"", option::Arg::Required,
			""-v <arg>  \t--verbosity <arg>  \tVerbosity level."" },
		{ E_SILENCE, 0, """", ""silence"", option::Arg::None,
			""  \t--silence  \tReduce default output to minimal."" },
		{ E_SOLVER, 0, """", ""solver"", option::Arg::Required,
			""  \t--solver <arg>  \t"" },
		{ E_PRIOR, 0, """", ""prior"", option::Arg::Required, 
			""  \t--prior <arg>  \tPOMCP prior."" },
		{ 0, 0, 0, 0, 0, 0 }
	};
	return usage;
}

SimpleTUI::SimpleTUI(string lower_bounds_str,
		string base_lower_bounds_str,
		string upper_bounds_str,
		string base_upper_bounds_str) {
	usage = buildUsage(lower_bounds_str, base_lower_bounds_str,
			upper_bounds_str, base_upper_bounds_str);
}

SimpleTUI::~SimpleTUI() {}

Solver *SimpleTUI::InitializeSolver(DSPOMDP *model, string solver_type,
                                    option::Option *options) {
  Solver *solver = NULL;
  // DESPOT or its default policy
  if (solver_type == ""DESPOT"" ||
      solver_type == ""PLB"") // PLB: particle lower bound
  {
    string blbtype = options[E_BLBTYPE] ? options[E_BLBTYPE].arg : ""DEFAULT"";
    string lbtype = options[E_LBTYPE] ? options[E_LBTYPE].arg : ""DEFAULT"";
    ScenarioLowerBound *lower_bound =
        model->CreateScenarioLowerBound(lbtype, blbtype);

    logi << ""Created lower bound "" << typeid(*lower_bound).name() << endl;

    if (solver_type == ""DESPOT"") {
      string bubtype = options[E_BUBTYPE] ? options[E_BUBTYPE].arg : ""DEFAULT"";
      string ubtype = options[E_UBTYPE] ? options[E_UBTYPE].arg : ""DEFAULT"";
      ScenarioUpperBound *upper_bound =
          model->CreateScenarioUpperBound(ubtype, bubtype);

      logi << ""Created upper bound "" << typeid(*upper_bound).name() << endl;

      solver = new DESPOT(model, lower_bound, upper_bound);
    } else
      solver = lower_bound;
  } // AEMS or its default policy
  else if (solver_type == ""AEMS"" || solver_type == ""BLB"") {
    string lbtype = options[E_LBTYPE] ? options[E_LBTYPE].arg : ""DEFAULT"";
    BeliefLowerBound *lower_bound =
        static_cast<BeliefMDP *>(model)->CreateBeliefLowerBound(lbtype);

    logi << ""Created lower bound "" << typeid(*lower_bound).name() << endl;

    if (solver_type == ""AEMS"") {
      string ubtype = options[E_UBTYPE] ? options[E_UBTYPE].arg : ""DEFAULT"";
      BeliefUpperBound *upper_bound =
          static_cast<BeliefMDP *>(model)->CreateBeliefUpperBound(ubtype);

      logi << ""Created upper bound "" << typeid(*upper_bound).name() << endl;

      solver = new AEMS(model, lower_bound, upper_bound);
    } else
      solver = lower_bound;
  } // POMCP or DPOMCP
  else if (solver_type == ""POMCP"" || solver_type == ""DPOMCP"") {
    string ptype = options[E_PRIOR] ? options[E_PRIOR].arg : ""DEFAULT"";
    POMCPPrior *prior = model->CreatePOMCPPrior(ptype);

    logi << ""Created POMCP prior "" << typeid(*prior).name() << endl;

    if (options[E_PRUNE]) {
      prior->exploration_constant(Globals::config.pruning_constant);
    }

    if (solver_type == ""POMCP"")
      solver = new POMCP(model, prior);
    else
      solver = new DPOMCP(model, prior);
  } else { // Unsupported solver
    cerr << ""ERROR: Unsupported solver type: "" << solver_type << endl;
    exit(1);
  }
  return solver;
}

void SimpleTUI::OptionParse(option::Option *options, int &num_runs,
                            string &simulator_type, string &belief_type,
                            int &time_limit, string &solver_type,
                            bool &search_solver) {
  if (options[E_SILENCE])
    Globals::config.silence = true;

  if (options[E_DEPTH])
    Globals::config.search_depth = atoi(options[E_DEPTH].arg);

  if (options[E_DISCOUNT])
    Globals::config.discount = atof(options[E_DISCOUNT].arg);

  if (options[E_SEED])
    Globals::config.root_seed = atoi(options[E_SEED].arg);
  else { // last 9 digits of current time in milli second
    long millis = (long)get_time_second() * 1000;
    long range = (long)pow((double)10, (int)9);
    Globals::config.root_seed =
        (unsigned int)(millis - (millis / range) * range);
  }

  if (options[E_TIMEOUT])
    Globals::config.time_per_move = atof(options[E_TIMEOUT].arg);

  if (options[E_NUMPARTICLES])
    Globals::config.num_scenarios = atoi(options[E_NUMPARTICLES].arg);

  if (options[E_PRUNE])
    Globals::config.pruning_constant = atof(options[E_PRUNE].arg);

  if (options[E_GAP])
    Globals::config.xi = atof(options[E_GAP].arg);

  if (options[E_SIM_LEN])
    Globals::config.sim_len = atoi(options[E_SIM_LEN].arg);

  if (options[E_EVALUATOR])
    simulator_type = options[E_EVALUATOR].arg;

  if (options[E_MAX_POLICY_SIM_LEN])
    Globals::config.max_policy_sim_len =
        atoi(options[E_MAX_POLICY_SIM_LEN].arg);

  if (options[E_DEFAULT_ACTION])
    Globals::config.default_action = options[E_DEFAULT_ACTION].arg;

  if (options[E_RUNS])
    num_runs = atoi(options[E_RUNS].arg);

  if (options[E_BELIEF])
    belief_type = options[E_BELIEF].arg;

  if (options[E_TIME_LIMIT])
    time_limit = atoi(options[E_TIME_LIMIT].arg);

  if (options[E_NOISE])
    Globals::config.noise = atof(options[E_NOISE].arg);

  search_solver = options[E_SEARCH_SOLVER];

  if (options[E_SOLVER])
    solver_type = options[E_SOLVER].arg;

//  int verbosity = 0;
  int verbosity = 3;//TODO:: remove debug log
  if (options[E_VERBOSITY])
    verbosity = atoi(options[E_VERBOSITY].arg);
  logging::level(verbosity);
}

void SimpleTUI::InitializeEvaluator(Evaluator *&simulator,
                                    option::Option *options, DSPOMDP *model,
                                    Solver *solver, int num_runs,
                                    clock_t main_clock_start,
                                    string simulator_type, string belief_type,
                                    int time_limit, string solver_type) {

  if (time_limit != -1) {
    simulator =
        new POMDPEvaluator(model, belief_type, solver, main_clock_start, &cout,
                           EvalLog::curr_inst_start_time + time_limit,
                           num_runs * Globals::config.sim_len);
  } else {
    simulator =
        new POMDPEvaluator(model, belief_type, solver, main_clock_start, &cout);
  }
}

void SimpleTUI::DisplayParameters(option::Option *options, DSPOMDP *model) {

  string lbtype = options[E_LBTYPE] ? options[E_LBTYPE].arg : ""DEFAULT"";
  string ubtype = options[E_UBTYPE] ? options[E_UBTYPE].arg : ""DEFAULT"";
  default_out << ""Model = "" << typeid(*model).name() << endl
              << ""Random root seed = "" << Globals::config.root_seed << endl
              << ""Search depth = "" << Globals::config.search_depth << endl
              << ""Discount = "" << Globals::config.discount << endl
              << ""Simulation steps = "" << Globals::config.sim_len << endl
              << ""Number of scenarios = "" << Globals::config.num_scenarios
              << endl
              << ""Search time per step = "" << Globals::config.time_per_move
              << endl
              << ""Regularization constant = ""
              << Globals::config.pruning_constant << endl
              << ""Lower bound = "" << lbtype << endl
              << ""Upper bound = "" << ubtype << endl
              << ""Policy simulation depth = ""
              << Globals::config.max_policy_sim_len << endl
              << ""Target gap ratio = "" << Globals::config.xi << endl;
  // << ""Solver = "" << typeid(*solver).name() << endl << endl;
}

void SimpleTUI::RunEvaluator(DSPOMDP *model, Evaluator *simulator,
                             option::Option *options, int num_runs,
                             bool search_solver, Solver *&solver,
                             string simulator_type, clock_t main_clock_start,
                             int start_run) {
  // Run num_runs simulations
  vector<double> round_rewards(num_runs);
  for (int round = start_run; round < start_run + num_runs; round++) {
    default_out << endl
                << ""####################################### Round "" << round
                << "" #######################################"" << endl;

    if (search_solver) {
      if (round == 0) {
        solver = InitializeSolver(model, ""DESPOT"", options);
        default_out << ""Solver: "" << typeid(*solver).name() << endl;

        simulator->solver(solver);
      } else if (round == 5) {
        solver = InitializeSolver(model, ""POMCP"", options);
        default_out << ""Solver: "" << typeid(*solver).name() << endl;

        simulator->solver(solver);
      } else if (round == 10) {
        double sum1 = 0, sum2 = 0;
        for (int i = 0; i < 5; i++)
          sum1 += round_rewards[i];
        for (int i = 5; i < 10; i++)
          sum2 += round_rewards[i];
        if (sum1 < sum2)
          solver = InitializeSolver(model, ""POMCP"", options);
        else
          solver = InitializeSolver(model, ""DESPOT"", options);
        default_out << ""Solver: "" << typeid(*solver).name()
                    << "" DESPOT:"" << sum1 << "" POMCP:"" << sum2 << endl;
      }

      simulator->solver(solver);
    }

    simulator->InitRound();

    for (int i = 0; i < Globals::config.sim_len; i++) {
      /*
      default_out << ""-----------------------------------Round "" << round
                  << "" Step "" << i << ""-----------------------------------""
                  << endl;*/
      double step_start_t = get_time_second();

      bool terminal = simulator->RunStep(i, round);

      if (terminal)
        break;

      double step_end_t = get_time_second();
      logi << ""[main] Time for step: actual / allocated = ""
           << (step_end_t - step_start_t) << "" / "" << EvalLog::allocated_time
           << endl;
      simulator->UpdateTimePerMove(step_end_t - step_start_t);
      logi << ""[main] Time per move set to "" << Globals::config.time_per_move
           << endl;
      logi << ""[main] Plan time ratio set to "" << EvalLog::plan_time_ratio
           << endl;
    //  default_out << endl;
    }

    default_out << ""Simulation terminated in "" << simulator->step() << "" steps""
                << endl;
    double round_reward = simulator->EndRound();
    round_rewards[round] = round_reward;
  }

  if (simulator_type == ""ippc"" && num_runs != 30) {
    cout << ""Exit without receiving reward."" << endl
         << ""Total time: Real / CPU = ""
         << (get_time_second() - EvalLog::curr_inst_start_time) << "" / ""
         << (double(clock() - main_clock_start) / CLOCKS_PER_SEC) << ""s""
         << endl;
    exit(0);
  }
}

void SimpleTUI::PrintResult(int num_runs, Evaluator *simulator,
                            clock_t main_clock_start) {

  cout << ""\nCompleted "" << num_runs << "" run(s)."" << endl;
  cout << ""Average total discounted reward (stderr) = ""
       << simulator->AverageDiscountedRoundReward() << "" (""
       << simulator->StderrDiscountedRoundReward() << "")"" << endl;
  cout << ""Average total undiscounted reward (stderr) = ""
       << simulator->AverageUndiscountedRoundReward() << "" (""
       << simulator->StderrUndiscountedRoundReward() << "")"" << endl;
  cout << ""Total time: Real / CPU = ""
       << (get_time_second() - EvalLog::curr_inst_start_time) << "" / ""
       << (double(clock() - main_clock_start) / CLOCKS_PER_SEC) << ""s"" << endl;
}

int SimpleTUI::run(int argc, char *argv[]) {
  enum_map_" + data.ProjectName + @"::Init();
  
  clock_t main_clock_start = clock();
  EvalLog::curr_inst_start_time = get_time_second();

  const char *program = (argc > 0) ? argv[0] : ""despot"";

  argc -= (argc > 0);
  argv += (argc > 0); // skip program name argv[0] if present

  option::Stats stats(usage, argc, argv);
  option::Option *options = new option::Option[stats.options_max];
  option::Option *buffer = new option::Option[stats.buffer_max];
  option::Parser parse(usage, argc, argv, options, buffer);

  string solver_type = ""DESPOT"";
  solver_type = ""POMCP"";//TODO::DELETE
  bool search_solver;

  /* =========================
   * Parse required parameters
   * =========================*/
  int num_runs = 1;
  string simulator_type = ""pomdp"";
  string belief_type = ""DEFAULT"";
  int time_limit = -1;

  /* =========================================
   * Problem specific default parameter values
*=========================================*/
  InitializeDefaultParameters();

  /* =========================
   * Parse optional parameters
   * =========================*/
  if (options[E_HELP]) {
    cout << ""Usage: "" << program << "" [options]"" << endl;
    option::printUsage(std::cout, usage);
    return 0;
  }
  OptionParse(options, num_runs, simulator_type, belief_type, time_limit,
              solver_type, search_solver);

  /* =========================
   * Global random generator
   * =========================*/
  Seeds::root_seed(Globals::config.root_seed);
  unsigned world_seed = Seeds::Next();
  unsigned seed = Seeds::Next();
  Random::RANDOM = Random(seed);

  /* =========================
   * initialize model
   * =========================*/
  DSPOMDP *model = InitializeModel(options);

  /* =========================
   * initialize solver
   * =========================*/
  Solver *solver = InitializeSolver(model, solver_type, options);
  assert(solver != NULL);

  /* =========================
   * initialize simulator
   * =========================*/
  Evaluator *simulator = NULL;
  InitializeEvaluator(simulator, options, model, solver, num_runs,
                      main_clock_start, simulator_type, belief_type, time_limit,
                      solver_type);
  simulator->world_seed(world_seed);

  int start_run = 0;

  /* =========================
   * Display parameters
   * =========================*/
  DisplayParameters(options, model);

  /* =========================
   * run simulator
   * =========================*/
  RunEvaluator(model, simulator, options, num_runs, search_solver, solver,
               simulator_type, main_clock_start, start_run);

  simulator->End();

  PrintResult(num_runs, simulator, main_clock_start);

  return 0;
}

} // namespace despot
";
            return file;
        }

        public static string GetEvaluatorHeaderFile(PLPsData data)
        {
            string file = @"#ifndef SIMULATOR_H
#define SIMULATOR_H

#include <despot/core/globals.h>
#include <despot/core/pomdp.h>
#include <despot/pomdpx/pomdpx.h>
#include <despot/util/util.h>
#include <despot/model_primitives/" + data.ProjectName + @"/enum_map_" + data.ProjectName + @".h>
namespace despot {

/* =============================================================================
 * EvalLog class
 * =============================================================================*/

class EvalLog {
private:
  std::vector<std::string> runned_instances;
	std::vector<int> num_of_completed_runs;
	std::string log_file_; 
public:
	static time_t start_time;

	static double curr_inst_start_time;
	static double curr_inst_target_time; // Targetted amount of time used for each step
	static double curr_inst_budget; // Total time in seconds given for current instance
	static double curr_inst_remaining_budget; // Remaining time in seconds for current instance
	static int curr_inst_steps;
	static int curr_inst_remaining_steps;
	static double allocated_time;
	static double plan_time_ratio;

	EvalLog(std::string log_file);

	void Save();
	void IncNumOfCompletedRuns(std::string problem);
	int GetNumCompletedRuns() const;
	int GetNumRemainingRuns() const;
	int GetNumCompletedRuns(std::string instance) const;
	int GetNumRemainingRuns(std::string instance) const;
	double GetUsedTimeInSeconds() const;
	double GetRemainingTimeInSeconds() const;

	// Pre-condition: curr_inst_start_time is initialized
	void SetInitialBudget(std::string instance);
	double GetRemainingBudget(std::string instance) const;
};

/* =============================================================================
 * Evaluator class
 * =============================================================================*/

/** Interface for evaluating a solver's performance by simulating how it runs
 * in a real.
 */
class Evaluator {
	private:
	std::vector<int> action_sequence_to_sim;
protected:
	DSPOMDP* model_;
	std::string belief_type_;
	Solver* solver_;
	clock_t start_clockt_;
	State* state_;
	int step_;
	double target_finish_time_;
	std::ostream* out_;

	std::vector<double> discounted_round_rewards_;
	std::vector<double> undiscounted_round_rewards_;
	double reward_;
	double total_discounted_reward_;
	double total_undiscounted_reward_;

public:
	Evaluator(DSPOMDP* model, std::string belief_type, Solver* solver,
		clock_t start_clockt, std::ostream* out);
	virtual ~Evaluator();

	inline void out(std::ostream* o) {
		out_ = o;
	}

	inline void rewards(std::vector<double> rewards) {
		undiscounted_round_rewards_ = rewards;
	}

	inline std::vector<double> rewards() {
		return undiscounted_round_rewards_;
	}

	inline int step() {
		return step_;
	}
	inline double target_finish_time() {
		return target_finish_time_;
	}
	inline void target_finish_time(double t) {
		target_finish_time_ = t;
	}
	inline Solver* solver() {
		return solver_;
	}
	inline void solver(Solver* s) {
		solver_ = s;
	}
	inline DSPOMDP* model() {
		return model_;
	}
	inline void model(DSPOMDP* m) {
		model_ = m;
	}

	virtual inline void world_seed(unsigned seed) {
	}

	virtual int Handshake(std::string instance) = 0; // Initialize simulator and return number of runs.
	virtual void InitRound() = 0;

	bool RunStep(int step, int round);

	virtual double EndRound() = 0; // Return total undiscounted reward for this round.
	virtual bool ExecuteAction(int action, double& reward, OBS_TYPE& obs, std::map<std::string, bool>& updates) = 0;
	//IcapsResponseModuleAndTempEnums CalculateModuleResponse(std::string moduleName);
	virtual void ReportStepReward();
	virtual double End() = 0; // Free resources and return total reward collected

	virtual void UpdateTimePerMove(double step_time) = 0;

	double AverageUndiscountedRoundReward() const;
	double StderrUndiscountedRoundReward() const;
	double AverageDiscountedRoundReward() const;
	double StderrDiscountedRoundReward() const;
};

/* =============================================================================
 * POMDPEvaluator class
 * =============================================================================*/

/** Evaluation by simulating using a DSPOMDP model.*/
class POMDPEvaluator: public Evaluator {
protected:
	Random random_;

public:
	POMDPEvaluator(DSPOMDP* model, std::string belief_type, Solver* solver,
		clock_t start_clockt, std::ostream* out, double target_finish_time = -1,
		int num_steps = -1);
	~POMDPEvaluator();

	virtual inline void world_seed(unsigned seed) {
		random_ = Random(seed);
	}

	int Handshake(std::string instance);
	void InitRound();
	double EndRound();
	bool ExecuteAction(int action, double& reward, OBS_TYPE& obs, std::map<std::string, bool>& updates);
	double End();
	void UpdateTimePerMove(double step_time);
};

} // namespace despot

#endif
";
            return file;
        }
        public static string GetEvaluatorCppFile(PLPsData data, InitializeProject initProj)
        {
            string file = @"#include <despot/evaluator.h>
#include <despot/util/mongoDB_Bridge.h>
#include <despot/model_primitives/" + data.ProjectName + @"/enum_map_" + data.ProjectName + @".h>
#include <despot/model_primitives/" + data.ProjectName + @"/actionManager.h>
#include <despot/model_primitives/" + data.ProjectName + @"/state.h>
#include <nlohmann/json.hpp>
using namespace std;


namespace despot {

/* =============================================================================
 * EvalLog class
 * =============================================================================*/

time_t EvalLog::start_time = 0;
double EvalLog::curr_inst_start_time = 0;
double EvalLog::curr_inst_target_time = 0;
double EvalLog::curr_inst_budget = 0;
double EvalLog::curr_inst_remaining_budget = 0;
int EvalLog::curr_inst_steps = 0;
int EvalLog::curr_inst_remaining_steps = 0;
double EvalLog::allocated_time = 1.0;
double EvalLog::plan_time_ratio = 1.0;

EvalLog::EvalLog(string log_file) :
	log_file_(log_file) {
	ifstream fin(log_file_.c_str(), ifstream::in);
	if (!fin.good() || fin.peek() == ifstream::traits_type::eof()) {
		time(&start_time);
	} else {
		fin >> start_time;

		int num_instances;
		fin >> num_instances;
		for (int i = 0; i < num_instances; i++) {
			string name;
			int num_runs;
			fin >> name >> num_runs;
			runned_instances.push_back(name);
			num_of_completed_runs.push_back(num_runs);
		}
	}
	fin.close();
}

void EvalLog::Save() {
	ofstream fout(log_file_.c_str(), ofstream::out);
	fout << start_time << endl;
	fout << runned_instances.size() << endl;
	for (int i = 0; i < runned_instances.size(); i++)
		fout << runned_instances[i] << "" "" << num_of_completed_runs[i] << endl;
	fout.close();
}

void EvalLog::IncNumOfCompletedRuns(string problem) {
	bool seen = false;
	for (int i = 0; i < runned_instances.size(); i++) {
		if (runned_instances[i] == problem) {
			num_of_completed_runs[i]++;
			seen = true;
		}
	}

	if (!seen) {
		runned_instances.push_back(problem);
		num_of_completed_runs.push_back(1);
	}
}

int EvalLog::GetNumCompletedRuns() const {
	int num = 0;
	for (int i = 0; i < num_of_completed_runs.size(); i++)
		num += num_of_completed_runs[i];
	return num;
}

int EvalLog::GetNumRemainingRuns() const {
	return 80 * 30 - GetNumCompletedRuns();
}

int EvalLog::GetNumCompletedRuns(string instance) const {
	for (int i = 0; i < runned_instances.size(); i++) {
		if (runned_instances[i] == instance)
			return num_of_completed_runs[i];
	}
	return 0;
}

int EvalLog::GetNumRemainingRuns(string instance) const {
	return 30 - GetNumCompletedRuns(instance);
}

double EvalLog::GetUsedTimeInSeconds() const {
	time_t curr;
	time(&curr);
	return (double) (curr - start_time);
}

double EvalLog::GetRemainingTimeInSeconds() const {
	return 24 * 3600 - GetUsedTimeInSeconds();
}

// Pre-condition: curr_inst_start_time is initialized
void EvalLog::SetInitialBudget(string instance) {
	curr_inst_budget = 0;
	if (GetNumRemainingRuns() != 0 && GetNumRemainingRuns(instance) != 0) {
		cout << ""Num of remaining runs: curr / total = ""
			<< GetNumRemainingRuns(instance) << "" / "" << GetNumRemainingRuns()
			<< endl;
		curr_inst_budget = (24 * 3600 - (curr_inst_start_time - start_time))
			/ GetNumRemainingRuns() * GetNumRemainingRuns(instance);
		if (curr_inst_budget < 0)
			curr_inst_budget = 0;
		if (curr_inst_budget > 18 * 60)
			curr_inst_budget = 18 * 60;
	}
}

double EvalLog::GetRemainingBudget(string instance) const {
	return curr_inst_budget
		- (get_time_second() - EvalLog::curr_inst_start_time);
}

/* =============================================================================
 * Evaluator class
 * =============================================================================*/

Evaluator::Evaluator(DSPOMDP* model, string belief_type, Solver* solver,
	clock_t start_clockt, ostream* out) :
	model_(model),
	belief_type_(belief_type),
	solver_(solver),
	start_clockt_(start_clockt),
	target_finish_time_(-1),
	out_(out) {
        " + (initProj.SolverConfiguration.ActionsToSimulate.Count > 0 ?
        (String.Join("        " + Environment.NewLine,
            initProj.SolverConfiguration.ActionsToSimulate.Select(x => "action_sequence_to_sim.push_back(" + x + ");").ToList()))
        : "") + @"
}

Evaluator::~Evaluator() {
}


bool Evaluator::RunStep(int step, int round) {
	if (target_finish_time_ != -1 && get_time_second() > target_finish_time_) {
		if (!Globals::config.silence && out_)
			*out_ << ""Exit. (Total time ""
				<< (get_time_second() - EvalLog::curr_inst_start_time)
				<< ""s exceeded time limit of ""
				<< (target_finish_time_ - EvalLog::curr_inst_start_time) << ""s)""
				<< endl
				<< ""Total time: Real / CPU = ""
				<< (get_time_second() - EvalLog::curr_inst_start_time) << "" / ""
				<< (double(clock() - start_clockt_) / CLOCKS_PER_SEC) << ""s""
				<< endl;
		exit(1);
	}

	double step_start_t = get_time_second();
    double start_t = get_time_second();
	int action = -1;

	if(action_sequence_to_sim.size() == 0)
	{
        action = solver_->Search().action;
	}
	else
	{
		action = action_sequence_to_sim[0];
		action_sequence_to_sim.erase(action_sequence_to_sim.begin());
		if(action_sequence_to_sim.size() == 0)
		{
			action_sequence_to_sim.push_back(-1);
		}
	}

	double end_t = get_time_second();
	logi << ""[RunStep] Time spent in "" << typeid(*solver_).name()
		<< ""::Search(): "" << (end_t - start_t) << endl;

	double reward;
	OBS_TYPE obs;
	start_t = get_time_second();

	//TODO:: remove prints
	logi << ""--------------------------------------EXECUTED---------------------------------------------------------------------------"" << endl;
	model_->PrintState(*state_);
	std::map<std::string, bool> updatesFromAction;
	bool terminal = ExecuteAction(action, reward, obs, updatesFromAction);
	model_->PrintState(*state_);
	logi << ""action:"" << action << "", reward:""
		 << "", reward:"" << reward << "", observation:"" << enum_map_" + data.ProjectName + @"::vecResponseEnumToString[(" + data.ProjectNameWithCapitalLetter + @"ResponseModuleAndTempEnums)obs] << endl;
	end_t = get_time_second();
	logi << ""[RunStep] Time spent in ExecuteAction(): "" << (end_t - start_t)
		<< endl;
	logi << ""-------------------------------------END-EXECUTED---------------------------------------------------------------------------"" << endl;
	start_t = get_time_second();
	*out_ << ""-----------------------------------Round "" << round
				<< "" Step "" << step << ""-----------------------------------""
				<< endl;
	if (!Globals::config.silence && out_) {
		*out_ << ""- Action = "";
		model_->PrintAction(action, *out_);
	}

	if (state_ != NULL) {
		if (!Globals::config.silence && out_) {
			*out_ << ""- State:\n"";
			model_->PrintState(*state_, *out_);
		}
	}

	if (!Globals::config.silence && out_) {
		*out_ << endl << ""- Observation = "" << enum_map_" + data.ProjectName + @"::vecResponseEnumToString[(" + data.ProjectNameWithCapitalLetter + @"ResponseModuleAndTempEnums)obs];
	}

	if (state_ != NULL) {
		if (!Globals::config.silence && out_)
			*out_ << ""- ObsProb = "" << model_->ObsProb(obs, *state_, action)
				<< endl;
	}

	ReportStepReward();
	end_t = get_time_second();

	double step_end_t;
	if (terminal) {
		step_end_t = get_time_second();
		logi << ""[RunStep] Time for step: actual / allocated = ""
			<< (step_end_t - step_start_t) << "" / "" << EvalLog::allocated_time
			<< endl;
		if (!Globals::config.silence && out_)
			*out_ << endl;
		step_++;
		return true;
	}

	*out_<<endl;

	start_t = get_time_second();
	if(action_sequence_to_sim.size() == 0)
	{
		solver_->Update(action, obs, updatesFromAction);
	}
	end_t = get_time_second();
	logi << ""[RunStep] Time spent in Update(): "" << (end_t - start_t) << endl;

	step_++;
	return false;
}

double Evaluator::AverageUndiscountedRoundReward() const {
	double sum = 0;
	for (int i = 0; i < undiscounted_round_rewards_.size(); i++) {
		double reward = undiscounted_round_rewards_[i];
		sum += reward;
	}
	return undiscounted_round_rewards_.size() > 0 ? (sum / undiscounted_round_rewards_.size()) : 0.0;
}

double Evaluator::StderrUndiscountedRoundReward() const {
	double sum = 0, sum2 = 0;
	for (int i = 0; i < undiscounted_round_rewards_.size(); i++) {
		double reward = undiscounted_round_rewards_[i];
		sum += reward;
		sum2 += reward * reward;
	}
	int n = undiscounted_round_rewards_.size();
	return n > 0 ? sqrt(sum2 / n / n - sum * sum / n / n / n) : 0.0;
}


double Evaluator::AverageDiscountedRoundReward() const {
	double sum = 0;
	for (int i = 0; i < discounted_round_rewards_.size(); i++) {
		double reward = discounted_round_rewards_[i];
		sum += reward;
	}
	return discounted_round_rewards_.size() > 0 ? (sum / discounted_round_rewards_.size()) : 0.0;
}

double Evaluator::StderrDiscountedRoundReward() const {
	double sum = 0, sum2 = 0;
	for (int i = 0; i < discounted_round_rewards_.size(); i++) {
		double reward = discounted_round_rewards_[i];
		sum += reward;
		sum2 += reward * reward;
	}
	int n = discounted_round_rewards_.size();
	return n > 0 ? sqrt(sum2 / n / n - sum * sum / n / n / n) : 0.0;
}

void Evaluator::ReportStepReward() {
	if (!Globals::config.silence && out_)
		*out_ << ""- Reward = "" << reward_ << endl
			<< ""- Current rewards:"" << endl
			<< ""  discounted / undiscounted = "" << total_discounted_reward_
			<< "" / "" << total_undiscounted_reward_ << endl;
}

/* =============================================================================
 * POMDPEvaluator class
 * =============================================================================*/

POMDPEvaluator::POMDPEvaluator(DSPOMDP* model, string belief_type,
	Solver* solver, clock_t start_clockt, ostream* out,
	double target_finish_time, int num_steps) :
	Evaluator(model, belief_type, solver, start_clockt, out),
	random_((unsigned) 0) {
	target_finish_time_ = target_finish_time;

	if (target_finish_time_ != -1) {
		EvalLog::allocated_time = (target_finish_time_ - get_time_second())
			/ num_steps;
		Globals::config.time_per_move = EvalLog::allocated_time;
		EvalLog::curr_inst_remaining_steps = num_steps;
	}
}

POMDPEvaluator::~POMDPEvaluator() {
}

int POMDPEvaluator::Handshake(string instance) {
	return -1; // Not to be used
}

void POMDPEvaluator::InitRound() {
	step_ = 0;

	double start_t, end_t;
	// Initial state
	state_ = model_->CreateStartState();
	logi << ""[POMDPEvaluator::InitRound] Created start state."" << endl;
	if (!Globals::config.silence && out_) {
		*out_ << ""Initial state: "" << endl;
		model_->PrintState(*state_, *out_);
		*out_ << endl;
	}

	// Initial belief
	start_t = get_time_second();
	delete solver_->belief();
	end_t = get_time_second();
	logi << ""[POMDPEvaluator::InitRound] Deleted old belief in ""
		<< (end_t - start_t) << ""s"" << endl;

	start_t = get_time_second();
	Belief* belief = model_->InitialBelief(state_, belief_type_);
	end_t = get_time_second();
	logi << ""[POMDPEvaluator::InitRound] Created intial belief ""
		<< typeid(*belief).name() << "" in "" << (end_t - start_t) << ""s"" << endl;

	solver_->belief(belief);

	total_discounted_reward_ = 0;
	total_undiscounted_reward_ = 0;
}

double POMDPEvaluator::EndRound() {
	if (!Globals::config.silence && out_) {
		*out_ << ""Total discounted reward = "" << total_discounted_reward_ << endl
			<< ""Total undiscounted reward = "" << total_undiscounted_reward_ << endl;
	}

	discounted_round_rewards_.push_back(total_discounted_reward_);
	undiscounted_round_rewards_.push_back(total_undiscounted_reward_);

	return total_undiscounted_reward_;
}

bool POMDPEvaluator::ExecuteAction(int action, double& reward, OBS_TYPE& obs, std::map<std::string, bool>& updates) {
	ActionDescription &actDesc = *ActionManager::actions[action];
	double random_num = random_.NextDouble();
	if(Globals::IsInternalSimulation())
	{
		
		bool terminal = model_->Step(*state_, random_num, action, reward, obs);

		reward_ = reward;
		total_discounted_reward_ += Globals::Discount(step_) * reward;
		total_undiscounted_reward_ += reward;

		return terminal;
	}
	else
	{
		ActionType acType(actDesc.actionType);
		std::string actionParameters = actDesc.GetActionParametersJson_ForActionExecution();
		
		std::string actionName = enum_map_" + data.ProjectName + @"::vecActionTypeEnumToString[acType];
	  
		bsoncxx::oid actionId = MongoDB_Bridge::SendActionToExecution(actDesc.actionId, actionName, actionParameters);

		std::string obsStr = """";
		updates = MongoDB_Bridge::WaitForActionResponse(actionId, obsStr);

		obs = enum_map_" + data.ProjectName + @"::vecStringToResponseEnum[obsStr];
		return false;
	}
}

double POMDPEvaluator::End() {
	return 0; // Not to be used
}

void POMDPEvaluator::UpdateTimePerMove(double step_time) {
	if (target_finish_time_ != -1) {
		if (step_time < 0.99 * EvalLog::allocated_time) {
			if (EvalLog::plan_time_ratio < 1.0)
				EvalLog::plan_time_ratio += 0.01;
			if (EvalLog::plan_time_ratio > 1.0)
				EvalLog::plan_time_ratio = 1.0;
		} else if (step_time > EvalLog::allocated_time) {
			double delta = (step_time - EvalLog::allocated_time)
				/ (EvalLog::allocated_time + 1E-6);
			if (delta < 0.02)
				delta = 0.02; // Minimum reduction per step
			if (delta > 0.05)
				delta = 0.05; // Maximum reduction per step
			EvalLog::plan_time_ratio -= delta;
			// if (EvalLog::plan_time_ratio < 0)
			// EvalLog::plan_time_ratio = 0;
		}

		EvalLog::curr_inst_remaining_budget = target_finish_time_
			- get_time_second();
		EvalLog::curr_inst_remaining_steps--;

		if (EvalLog::curr_inst_remaining_steps <= 0) {
			EvalLog::allocated_time = 0;
		} else {
			EvalLog::allocated_time =
				(EvalLog::curr_inst_remaining_budget - 2.0)
					/ EvalLog::curr_inst_remaining_steps;

			if (EvalLog::allocated_time > 5.0)
				EvalLog::allocated_time = 5.0;
		}

		Globals::config.time_per_move = EvalLog::plan_time_ratio
			* EvalLog::allocated_time;
	}
}

} // namespace despot
";
            return file;
        }

        public static string GetGlobalsFile(PLPsData data, int totalNumberOfActionsInProject)
        {
            string file = @"struct globals
{
    static constexpr double MAX_IMMEDIATE_REWARD = " + data.MaxReward + @";
    static constexpr double MIN_IMMEDIATE_REWARD = " + data.MinReward + @";
    static constexpr int NUM_OF_ACTIONS = " + totalNumberOfActionsInProject + @"; 
};
";
            return file;
        }

        public static string GetMainFile(PLPsData data)
        {
            string file = @"#include <despot/simple_tui.h>
#include """ + data.ProjectName + @".h""

using namespace despot;

class TUI: public SimpleTUI {
public:
  TUI() {
  }

  DSPOMDP* InitializeModel(option::Option* options) {
    DSPOMDP* model = new " + data.ProjectNameWithCapitalLetter + @"();
    return model;
   }

  void InitializeDefaultParameters() {
     Globals::config.num_scenarios = 100;
  }
};

int main(int argc, char* argv[]) {
  return TUI().run(argc, argv);
}
";
            return file;
        }

        private static string GetModelHeaderFileDistributionVariableDefinition(PLPsData data)
        {
            string result = "";
            foreach (DistributionSample dist in data.DistributionSamples.Values)
            {
                switch (dist.Type)
                {
                    case DistributionType.Normal:
                        result += "    static std::normal_distribution<> " + dist.C_VariableName + "; //" + dist.FunctionDescription + Environment.NewLine;
                        break;
                    case DistributionType.Discrete:
                        result += "    static std::discrete_distribution<> " + dist.C_VariableName + "; //" + dist.FunctionDescription + Environment.NewLine;
                        break;
                    case DistributionType.Uniform:
                        throw new NotImplementedException("Uniform distribution is not supported yet, remove '" + dist.FunctionDescription + "'!");
                }
            }
            return result;
        }

        private static string GetPrintEnvironmentEnumPrintFunctionDeclarations(PLPsData data)
        {
            string result = "";
            foreach (EnumVarTypePLP eType in data.GlobalEnumTypes)
            {
                result += GenerateFilesUtils.GetIndentationStr(1, 4, "static std::string Print" + eType.TypeName + "(" + eType.TypeName + ");");
            }
            return result;
        }
        public static string GetModelHeaderFile(PLPsData data)
        {
            string file = @"
#include ""globals.h""
#include <despot/core/pomdp.h>
#include <despot/solver/pomcp.h> 
#include <random>
#include <string>
#include <despot/model_primitives/" + data.ProjectName + @"/enum_map_" + data.ProjectName + @".h> 
#include <despot/model_primitives/" + data.ProjectName + @"/state.h> 
namespace despot {

/* ==============================================================================
 * " + data.ProjectNameWithCapitalLetter + @"State class
 * ==============================================================================*/

class " + data.ProjectNameWithCapitalLetter + @"State;
class AOSUtils
{
	public:
	static bool Bernoulli(double);
};

class ActionDescription; 

class Prints
{
	public:
" + GetPrintEnvironmentEnumPrintFunctionDeclarations(data) + @"
	static std::string PrintActionDescription(ActionDescription*);
	static std::string PrintActionType(ActionType);
	static std::string PrintState(" + data.ProjectNameWithCapitalLetter + @"State state);
	static std::string PrintObs(int action, int obs);
};



/* ==============================================================================
 * " + data.ProjectNameWithCapitalLetter + @" and PocmanBelief class
 * ==============================================================================*/
class " + data.ProjectNameWithCapitalLetter + @";
class " + data.ProjectNameWithCapitalLetter + @"Belief: public ParticleBelief {
protected:
	const " + data.ProjectNameWithCapitalLetter + @"* " + data.ProjectName + @"_;
public:
	static int num_particles; 
	" + data.ProjectNameWithCapitalLetter + @"Belief(std::vector<State*> particles, const DSPOMDP* model, Belief* prior =
		NULL);
	void Update(int actionId, OBS_TYPE obs, std::map<std::string,bool> updates);
};

/* ==============================================================================
 * " + data.ProjectNameWithCapitalLetter + @" 
 * ==============================================================================*/
/**
 * The implementation is adapted from that included in the POMCP software.
 */

class " + data.ProjectNameWithCapitalLetter + @": public DSPOMDP {
public:
	virtual std::string PrintObs(int action, OBS_TYPE obs) const;
	virtual std::string PrintStateStr(const State &state) const;
	virtual std::string GetActionDescription(int) const;
	void UpdateStateByRealModuleObservation(State &state, int actionId, OBS_TYPE &observation) const;
	virtual bool Step(State &state, double rand_num, int actionId, double &reward,
					  OBS_TYPE &observation) const;
	int NumActions() const;
	virtual double ObsProb(OBS_TYPE obs, const State& state, int actionId) const;

	virtual State* CreateStartState(std::string type = ""DEFAULT"") const;
	virtual Belief* InitialBelief(const State* start,
		std::string type = ""PARTICLE"") const;

	inline double GetMaxReward() const {
		return globals::MAX_IMMEDIATE_REWARD;
	}
	 

	inline ValuedAction GetMinRewardAction() const {
		return ValuedAction(0, globals::MIN_IMMEDIATE_REWARD);
	}
	 
	POMCPPrior* CreatePOMCPPrior(std::string name = ""DEFAULT"") const;

	virtual void PrintState(const State& state, std::ostream& out = std::cout) const;
	

	
	virtual void PrintObs(const State& state, OBS_TYPE observation,
		std::ostream& out = std::cout) const;
	void PrintBelief(const Belief& belief, std::ostream& out = std::cout) const;
	virtual void PrintAction(int actionId, std::ostream& out = std::cout) const;

	State* Allocate(int state_id, double weight) const;
	virtual State* Copy(const State* particle) const;
	virtual void Free(State* particle) const;
	int NumActiveParticles() const;


public:
	" + data.ProjectNameWithCapitalLetter + @"(); 

private:
	void CheckPreconditions(const " + data.ProjectNameWithCapitalLetter + @"State& farstate, double &reward, bool &meetPrecondition, int actionId) const;
	void SampleModuleExecutionTime(const " + data.ProjectNameWithCapitalLetter + @"State& state, double rand_num, int actionId, int &moduleExecutionTime) const;
	void ExtrinsicChangesDynamicModel(const " + data.ProjectNameWithCapitalLetter + @"State& initState, " + data.ProjectNameWithCapitalLetter + @"State& afterExState, double rand_num, int actionId, double& reward,
		const int &moduleExecutionTime) const;
	void ModuleDynamicModel(const " + data.ProjectNameWithCapitalLetter + @"State &initState, const " + data.ProjectNameWithCapitalLetter + @"State &afterExState, " + data.ProjectNameWithCapitalLetter + @"State &nextState, double rand_num, int actionId, double &reward,
								 OBS_TYPE &observation, const int &moduleExecutionTime) const;
	bool ProcessSpecialStates(const " + data.ProjectNameWithCapitalLetter + @"State &state, double &reward) const;

	mutable MemoryPool<" + data.ProjectNameWithCapitalLetter + @"State> memory_pool_;
	static std::default_random_engine generator;
" + GetModelHeaderFileDistributionVariableDefinition(data) + @"
};
} // namespace despot
 ";
            return file;
        }





        private static string GetClassesGetActionManagerHeader(PLPsData data)
        {
            string result = "";

            foreach (string plpName in data.PLPs.Keys)
            {
                PLP plp = data.PLPs[plpName];
                if (plp.GlobalVariableModuleParameters.Count > 0)
                {
                    result += @"class " + GenerateFilesUtils.ToUpperFirstLetter(plpName) + @"ActionDescription: public ActionDescription
{
    public:
";
                    foreach (GlobalVariableModuleParameter param in plp.GlobalVariableModuleParameters)
                    {
                        result += "        " + param.Type + " " + param.Name + ";" + Environment.NewLine;
                    }
                    foreach (GlobalVariableModuleParameter param in plp.GlobalVariableModuleParameters)
                    {
                        result += "        std::string strLink_" + param.Name + ";" + Environment.NewLine;
                    }

                    result += "        " + GenerateFilesUtils.ToUpperFirstLetter(plpName) + "ActionDescription(";
                    string parameters = "";
                    foreach (GlobalVariableModuleParameter param in plp.GlobalVariableModuleParameters)
                    {
                        parameters += (parameters.Length == 0) ? "" : ", ";
                        parameters += "int _" + param.Name + "_Index";
                    }
                    result += parameters + ");" + Environment.NewLine;

                    result += @"        virtual void SetActionParametersByState(" + data.ProjectNameWithCapitalLetter + @"State *state, std::vector<std::string> indexes);
        virtual std::string GetActionParametersJson_ForActionExecution();
        virtual std::string GetActionParametersJson_ForActionRegistration();
        " + GenerateFilesUtils.ToUpperFirstLetter(plpName) + @"ActionDescription(){};
};

";
                }
            }

            return result;
        }

        public static string GetActionManagerHeaderFile(PLPsData data)
        {
            string file = @"#ifndef ACTION_MANAGER_H
#define ACTION_MANAGER_H

#include ""state.h""
#include <despot/model_primitives/" + data.ProjectName + @"/enum_map_" + data.ProjectName + @".h> 
#include <vector>
#include <utility>
#include <string>
namespace despot { 


    class ActionDescription
    {
    public:
        int actionId;
        ActionType actionType;
        virtual void SetActionParametersByState(" + data.ProjectNameWithCapitalLetter + @"State *state, std::vector<std::string> indexes);
        virtual std::string GetActionParametersJson_ForActionExecution() { return """"; };
        virtual std::string GetActionParametersJson_ForActionRegistration() { return """"; };
        
    };

" + GetClassesGetActionManagerHeader(data) + @"

class ActionManager {
public:
	static std::vector<ActionDescription*> actions;
    static void Init(" + data.ProjectNameWithCapitalLetter + @"State* state);
};
}
#endif //ACTION_MANAGER_H
";
            return file;
        }

        private static string GetResponseModuleAndTempEnumsList(PLPsData data, out List<string> responseModuleAndTempEnums, out Dictionary<string, Dictionary<string, string>> enumMappingsForModuleResponseAndTempVar)
        {
            enumMappingsForModuleResponseAndTempVar = new Dictionary<string, Dictionary<string, string>>();
            responseModuleAndTempEnums = new List<string>();
            string result = @"
  enum " + data.ProjectNameWithCapitalLetter + @"ResponseModuleAndTempEnums
  {
";
            string sFullEnumName = "";
            foreach (PLP plp in data.PLPs.Values)
            {
                enumMappingsForModuleResponseAndTempVar.Add(plp.Name, new Dictionary<string, string>());
                sFullEnumName = plp.Name + "_moduleResponse";
                result += "	  " + sFullEnumName + "," + Environment.NewLine;
                foreach (string sEnumResponse in plp.EnumResponse)
                {
                    sFullEnumName = plp.Name + "_" + sEnumResponse;
                    enumMappingsForModuleResponseAndTempVar[plp.Name].Add(sEnumResponse, sFullEnumName);
                    responseModuleAndTempEnums.Add(sFullEnumName);
                    result += "	  " + sFullEnumName + "," + Environment.NewLine;
                }

                foreach (var assign in plp.DynamicModel_VariableAssignments)
                {
                    if (assign.TempVariable.Type == PLPsData.ENUM_VARIABLE_TYPE_NAME)
                    {
                        sFullEnumName = plp.Name + "_" + assign.TempVariable.EnumName;
                        enumMappingsForModuleResponseAndTempVar[plp.Name].Add(assign.TempVariable.EnumName, sFullEnumName);
                        result += "	  " + sFullEnumName + "," + Environment.NewLine;
                        foreach (string sEnumVal in assign.TempVariable.EnumValues)
                        {
                            sFullEnumName = plp.Name + "_" + sEnumVal;
                            enumMappingsForModuleResponseAndTempVar[plp.Name].Add(sEnumVal, sFullEnumName);
                            result += "	  " + plp.Name + "_" + sEnumVal + "," + Environment.NewLine;
                            responseModuleAndTempEnums.Add(sFullEnumName);
                        }
                    }
                }
            }

            result += @"
	  illegalActionObs = 100000
  };
";

            return result;
        }

        private static string GetActionTypeEnum(PLPsData data)
        {
            string result = @"
  enum ActionType
{
";
            int i = 0;
            foreach (string plpName in data.PLPs.Keys)
            {
                result += "    " + plpName + "Action" + (i == data.PLPs.Count - 1 ? "" : ",") + Environment.NewLine;
                i++;
            }

            result += @"	
};
";
            return result;
        }

        private static string GetCreateMapResponseEnumToString(PLPsData data, List<string> responseModuleAndTempEnums)
        {
            string result = @"	  static map<" + data.ProjectNameWithCapitalLetter + @"ResponseModuleAndTempEnums,std::string> CreateMapResponseEnumToString()
	  {
          map<" + data.ProjectNameWithCapitalLetter + @"ResponseModuleAndTempEnums,std::string> m;" + Environment.NewLine;
            foreach (string sEnum in responseModuleAndTempEnums)
            {
                result += "          m[" + sEnum + "] = \"" + sEnum + "\";" + Environment.NewLine;
            }
            result += "          m[illegalActionObs] = \"IllegalActionObs\";" + Environment.NewLine;
            result += @"          return m;
        }
";

            return result;
        }

        private static string GetCreateMapActionTypeEnumToString(PLPsData data)
        {
            string result = @"
		static map<ActionType,std::string> CreateMapActionTypeEnumToString()
	  {
          map<ActionType,std::string> m;" + Environment.NewLine;

            foreach (string plpName in data.PLPs.Keys)
            {
                result += "          m[" + plpName + "Action] = \"" + plpName + "\";" + Environment.NewLine;
            }
            result += @"
          return m;
        }
";
            return result;
        }


        public static string GetEnumMapHeaderFile(PLPsData data, out Dictionary<string, Dictionary<string, string>> enumMappingsForModuleResponseAndTempVar)
        {
            List<string> _responseModuleAndTempEnums;
            string file = @"#ifndef ENUM_MAP_" + data.ProjectName.ToUpper() + @"_H
#define ENUM_MAP_" + data.ProjectName.ToUpper() + @"_H

#include <map>

#include ""state.h""
#include <vector>
#include <utility>
#include <string>
using namespace std;
namespace despot
{
" + GetActionTypeEnum(data) + @"

" + GetResponseModuleAndTempEnumsList(data, out _responseModuleAndTempEnums, out enumMappingsForModuleResponseAndTempVar) + @"

  struct enum_map_" + data.ProjectName + @"
  {
" + GetCreateMapResponseEnumToString(data, _responseModuleAndTempEnums) + @"
		static map<std::string, " + data.ProjectNameWithCapitalLetter + @"ResponseModuleAndTempEnums> CreateMapStringToEnum(map<" + data.ProjectNameWithCapitalLetter + @"ResponseModuleAndTempEnums,std::string> vecResponseEnumToString)
	  {
          map<std::string, " + data.ProjectNameWithCapitalLetter + @"ResponseModuleAndTempEnums> m;
		  map<" + data.ProjectNameWithCapitalLetter + @"ResponseModuleAndTempEnums,std::string>::iterator it;
		  for (it = vecResponseEnumToString.begin(); it != vecResponseEnumToString.end(); it++)
			{
				m[it->second] = it->first;
			}

          return m;
        }

" + GetCreateMapActionTypeEnumToString(data) + @"
    static map<" + data.ProjectNameWithCapitalLetter + @"ResponseModuleAndTempEnums,std::string> vecResponseEnumToString;
	static map<std::string, " + data.ProjectNameWithCapitalLetter + @"ResponseModuleAndTempEnums> vecStringToResponseEnum;
	static map<ActionType,std::string> vecActionTypeEnumToString;
	static void Init();
  };

} // namespace despot
#endif /* ENUM_MAP_" + data.ProjectName.ToUpper() + @"_H */

 
";
            return file;
        }


        private static string GetGetStateVarTypesHeaderCompoundTypes(PLPsData data)
        {
            string result = "";
            foreach (CompoundVarTypePLP complType in data.GlobalCompoundTypes)
            {
                result += Environment.NewLine;
                result += @"
	struct " + complType.TypeName + @"
	{
";
                for (int i = 0; i < complType.Variables.Count; i++)
                {
                    CompoundVarTypePLP_Variable oComVar = complType.Variables[i];
                    result += "		" + oComVar.Type + " " + oComVar.Name + ";" + Environment.NewLine;
                }
                result += @"		" + complType.TypeName + @"(); 
	};

";
            }
            return result;
        }
        private static string GetGetStateVarTypesHeaderEnumTypes(PLPsData data)
        {
            string result = "";
            foreach (EnumVarTypePLP enumlVar in data.GlobalEnumTypes)
            {
                result += Environment.NewLine;
                result += @"
	enum " + enumlVar.TypeName + @"
	{
";
                for (int i = 0; i < enumlVar.Values.Count; i++)
                {
                    result += "		" + enumlVar.Values[i] + (i == enumlVar.Values.Count - 1 ? "" : ",") + Environment.NewLine;
                }
                result += @"	};
";
            }
            return result;
        }
        public static string GetStateVarTypesHeaderFile(PLPsData data)
        {
            string file = @"#ifndef VAR_TYPES_H
#define VAR_TYPES_H

#include <string>
#include <iostream> 

namespace despot
{
	typedef bool anyValue;

" + GetGetStateVarTypesHeaderEnumTypes(data) + @"

" + GetGetStateVarTypesHeaderCompoundTypes(data) + @"

 
} // namespace despot
#endif //VAR_TYPES_H";
            return file;
        }

        private static string GetVariableDeclarationsForStateHeaderFile(PLPsData data)
        {
            string result = "";
            foreach (EnumVarTypePLP oEnumType in data.GlobalEnumTypes)
            {
                result += "    std::vector<" + oEnumType.TypeName + "> " + oEnumType.TypeName + "Objects;" + Environment.NewLine;
            }

            HashSet<string> paramTypes = new HashSet<string>();
            foreach (GlobalVariableDeclaration oVarDec in data.GlobalVariableDeclarations)
            {

                if (oVarDec.IsActionParameterValue)
                {
                    paramTypes.Add(oVarDec.Type);
                }
            }
            foreach (string paramType in paramTypes)
            {
                result += "    std::map<std::string, " + paramType + "> " + paramType + "ObjectsForActions;" + Environment.NewLine;
            }

            bool HasAnyValue = false;
            foreach (GlobalVariableDeclaration oVarDec in data.GlobalVariableDeclarations)
            {
                HasAnyValue = oVarDec.Type == PLPsData.ANY_VALUE_TYPE_NAME ? true : HasAnyValue;
                result += "    " + oVarDec.Type + " " + oVarDec.Name + ";" + Environment.NewLine;
            }
            result += "    std::map<std::string, anyValue*> anyValueUpdateDic;" + Environment.NewLine;
            return result;
        }

        public static string GetStateHeaderFile(PLPsData data)
        {
            string file = @"#ifndef STATE_H
#define STATE_H
#include <vector>
#include <despot/core/pomdp.h> 
#include ""state_var_types.h""
namespace despot
{


class " + data.ProjectNameWithCapitalLetter + @"State : public State {
public:
" + GetVariableDeclarationsForStateHeaderFile(data) + @"

	public:
		static void SetAnyValueLinks(" + data.ProjectNameWithCapitalLetter + @"State *state);
		
};
}
#endif //STATE_H";
            return file;
        }




        private static string GetClassesFunctionDefinitionForActionManagerCPP(PLPsData data)
        {
            string result = "";


            foreach (string plpName in data.PLPs.Keys)
            {
                PLP plp = data.PLPs[plpName];
                if (plp.GlobalVariableModuleParameters.Count > 0)
                {
                    //PLPActionDescription::SetActionParametersByState()
                    result += @"void " + GenerateFilesUtils.ToUpperFirstLetter(plpName) + @"ActionDescription::SetActionParametersByState(" + data.ProjectNameWithCapitalLetter + @"State *state, std::vector<std::string> indexes)
{
";
                    for (int i = 0; plp.GlobalVariableModuleParameters.Count > i; i++)
                    {
                        GlobalVariableModuleParameter param = plp.GlobalVariableModuleParameters[i];
                        result += "    strLink_" + param.Name + " = indexes[" + i + "];" + Environment.NewLine;
                        result += "    " + param.Name + " = (state->" + param.Type + "ObjectsForActions[indexes[" + i + "]]);" + Environment.NewLine;
                    }
                    result += "}" + Environment.NewLine;
                    //------------------------------------------------------------------------------------------------------------------------------------

                    //PLPActionDescription::GetActionParametersJson_ForActionExecution()
                    result += @"std::string " + GenerateFilesUtils.ToUpperFirstLetter(plpName) + @"ActionDescription::GetActionParametersJson_ForActionExecution()
{  
    json j;
";
                    for (int i = 0; plp.GlobalVariableModuleParameters.Count > i; i++)
                    {
                        GlobalVariableModuleParameter param = plp.GlobalVariableModuleParameters[i];
                        result += "    j[\"ParameterLinks\"][\"" + param.Name + "\"] = strLink_" + param.Name + ";" + Environment.NewLine;

                        List<CompoundVarTypePLP> lTemp = data.GlobalCompoundTypes.Where(x => x.TypeName == param.Type).ToList();
                        bool IsNotCompund = lTemp.Count == 0;
                        if (IsNotCompund)
                        {
                            result += "    j[\"ParameterValues\"][\"" + param.Name + "\"] = " + param.Name + ";" + Environment.NewLine;
                        }
                        else
                        {
                            CompoundVarTypePLP oType = lTemp[0];
                            foreach (var oVar in oType.Variables)
                            {
                                result += "    j[\"ParameterValues\"][\"" + param.Name + "\"][\"" + oVar.Name + "\"] = " + param.Name + "." + oVar.Name + ";" + Environment.NewLine;
                            }
                        }
                    }

                    result += @"
    std::string str(j.dump().c_str());
    return str;
}" + Environment.NewLine;
                    //------------------------------------------------------------------------------------------------------------------------------------

                    //PLPActionDescription::GetActionParametersJson_ForActionRegistration()
                    result += @"std::string " + GenerateFilesUtils.ToUpperFirstLetter(plpName) + @"ActionDescription::GetActionParametersJson_ForActionRegistration()
{
    json j;" + Environment.NewLine;

                    for (int i = 0; plp.GlobalVariableModuleParameters.Count > i; i++)
                    {
                        GlobalVariableModuleParameter param = plp.GlobalVariableModuleParameters[i];

                        List<CompoundVarTypePLP> lTemp = data.GlobalCompoundTypes.Where(x => x.TypeName == param.Type).ToList();
                        bool IsNotCompund = lTemp.Count == 0;
                        if (IsNotCompund)
                        {
                            result += "    j[\"" + param.Name + "\"] = " + param.Name + ";" + Environment.NewLine;
                        }
                        else
                        {
                            CompoundVarTypePLP oType = lTemp[0];
                            foreach (var oVar in oType.Variables)
                            {
                                result += "    j[\"" + param.Name + "->" + oVar.Name + "\"] = " + param.Name + "." + oVar.Name + ";" + Environment.NewLine;

                            }
                        }
                    }

                    result += @"
    std::string str(j.dump().c_str());
    return str;
}";


                }
            }


            return result;
        }
        public static string GetActionManagerCPpFile(PLPsData data, out int totalNumberOfActionsInProject)
        {
            string file = @"
#include <despot/model_primitives/" + data.ProjectName + @"/actionManager.h>
#include <despot/util/mongoDB_Bridge.h>
#include <nlohmann/json.hpp> 

// for convenience
using json = nlohmann::json;
//#include ""actionManager.h""
#include <vector>
#include <utility>
#include <string>
namespace despot { 
    void ActionDescription::SetActionParametersByState(" + data.ProjectNameWithCapitalLetter + @"State *state, std::vector<std::string> indexes){}
    std::vector<ActionDescription*> ActionManager::actions;


" + GetClassesFunctionDefinitionForActionManagerCPP(data) + @"

void ActionManager::Init(" + data.ProjectNameWithCapitalLetter + @"State* state)
{
	
	int id = 0;
" + GetAddingActionForActionManagerCPP(data, out totalNumberOfActionsInProject) + @"

	for(int j=0;j< ActionManager::actions.size();j++)
	{
        MongoDB_Bridge::RegisterAction(ActionManager::actions[j]->actionId, enum_map_" + data.ProjectName + @"::vecActionTypeEnumToString[ActionManager::actions[j]->actionType], ActionManager::actions[j]->GetActionParametersJson_ForActionRegistration());
    }
}
}";
            return file;
        }

        private static string GetAddingActionForActionManagerCPP(PLPsData data, out int totalNumberOfActionsInProject)
        {
            totalNumberOfActionsInProject = 0;
            string result = "";
            foreach (PLP plp in data.PLPs.Values)
            {
                if (plp.GlobalVariableModuleParameters.Count == 0)
                {
                    totalNumberOfActionsInProject += 1;
                    result += "    ActionDescription *" + plp.Name + " = new ActionDescription;" + Environment.NewLine;
                    result += "    " + plp.Name + "->actionType = " + plp.Name + "Action;" + Environment.NewLine;
                    result += "    " + plp.Name + "->actionId = id++;" + Environment.NewLine;

                    result += "    ActionManager::actions.push_back(" + plp.Name + ");" + Environment.NewLine + Environment.NewLine;
                }
                else
                {
                    int totalActionNumberForPLP = 1;
                    KeyValuePair<GlobalVariableModuleParameter, int>[] parameters = new KeyValuePair<GlobalVariableModuleParameter, int>[plp.GlobalVariableModuleParameters.Count];
                    for (int i = 0; plp.GlobalVariableModuleParameters.Count > i; i++)
                    {
                        GlobalVariableModuleParameter oParam = plp.GlobalVariableModuleParameters[i];
                        int cardinality = data.GlobalVariableDeclarations.Where(x => x.IsActionParameterValue && x.Type.Equals(oParam.Type)).Count();
                        totalActionNumberForPLP *= cardinality;
                        parameters[i] = new KeyValuePair<GlobalVariableModuleParameter, int>(oParam, cardinality);

                    }
                    totalNumberOfActionsInProject += totalActionNumberForPLP;
                    result += "    " + GenerateFilesUtils.ToUpperFirstLetter(plp.Name) + "ActionDescription* " + plp.Name + "Actions = new " + GenerateFilesUtils.ToUpperFirstLetter(plp.Name) + "ActionDescription[" + totalActionNumberForPLP + "];" + Environment.NewLine;
                    result += "    std::vector<std::string> indexes;" + Environment.NewLine;
                    result += "    int i = 0;" + Environment.NewLine;
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        GlobalVariableModuleParameter oPar = parameters[i].Key;
                        string it = "it" + (i + 1).ToString();
                        result += GenerateFilesUtils.GetIndentationStr(i + 1, 4, "map<std::string, " + oPar.Type + ">::iterator " + it + ";");
                        result += GenerateFilesUtils.GetIndentationStr(i + 1, 4, "for (" + it + " = state->" + oPar.Type + "ObjectsForActions.begin(); " + it + " != state->" + oPar.Type + "ObjectsForActions.end(); " + it + "++)");
                        result += GenerateFilesUtils.GetIndentationStr(i + 1, 4, "{");
                        result += GenerateFilesUtils.GetIndentationStr(i + 2, 4, "indexes.push_back(" + it + "->first);");

                        if (i == parameters.Length - 1)
                        {
                            string actionVarName = "o" + GenerateFilesUtils.ToUpperFirstLetter(plp.Name) + "Action";
                            result += GenerateFilesUtils.GetIndentationStr(i + 2, 4, GenerateFilesUtils.ToUpperFirstLetter(plp.Name) + "ActionDescription &" + actionVarName + " = " + plp.Name + "Actions[i];");
                            result += GenerateFilesUtils.GetIndentationStr(i + 2, 4, actionVarName + ".SetActionParametersByState(state, indexes);");
                            result += GenerateFilesUtils.GetIndentationStr(i + 2, 4, actionVarName + ".actionId = id++;");
                            result += GenerateFilesUtils.GetIndentationStr(i + 2, 4, actionVarName + ".actionType = " + plp.Name + "Action;");

                            result += GenerateFilesUtils.GetIndentationStr(i + 2, 4, "ActionManager::actions.push_back(&" + actionVarName + ");");
                            result += GenerateFilesUtils.GetIndentationStr(i + 2, 4, "i++;");
                            result += GenerateFilesUtils.GetIndentationStr(i + 2, 4, "indexes.pop_back();");
                            result += GenerateFilesUtils.GetIndentationStr(i + 1, 4, "}");
                        }
                    }
                }
            }
            return result;

        }

        public static string GetEnumMapCppFile(PLPsData data)
        {
            string file = @"
#include <despot/model_primitives/" + data.ProjectName + @"/enum_map_" + data.ProjectName + @".h> 
using namespace std;
namespace despot
{ 
	map<" + data.ProjectNameWithCapitalLetter + @"ResponseModuleAndTempEnums, std::string> enum_map_" + data.ProjectName + @"::vecResponseEnumToString;
	map<std::string, " + data.ProjectNameWithCapitalLetter + @"ResponseModuleAndTempEnums> enum_map_" + data.ProjectName + @"::vecStringToResponseEnum ;
	map<ActionType,std::string> enum_map_" + data.ProjectName + @"::vecActionTypeEnumToString;

	void enum_map_" + data.ProjectName + @"::Init()
	{
		if(enum_map_" + data.ProjectName + @"::vecResponseEnumToString.size() > 0)
			return; 

		enum_map_" + data.ProjectName + @"::vecResponseEnumToString = enum_map_" + data.ProjectName + @"::CreateMapResponseEnumToString();
	 enum_map_" + data.ProjectName + @"::vecStringToResponseEnum = enum_map_" + data.ProjectName + @"::CreateMapStringToEnum(enum_map_" + data.ProjectName + @"::vecResponseEnumToString);
	enum_map_" + data.ProjectName + @"::vecActionTypeEnumToString = enum_map_" + data.ProjectName + @"::CreateMapActionTypeEnumToString();
	}
} // namespace despot
";
            return file;
        }

        private static string GetStateVarTypesCppConstructors(PLPsData data)
        {
            string result = "";

            foreach (CompoundVarTypePLP oCompType in data.GlobalCompoundTypes)
            {
                result += GenerateFilesUtils.GetIndentationStr(1, 4, oCompType.TypeName + "::" + oCompType.TypeName + "()");
                result += GenerateFilesUtils.GetIndentationStr(1, 4, "{");

                foreach (CompoundVarTypePLP_Variable oCompVar in oCompType.Variables)
                {
                    if (oCompVar.Type == PLPsData.ANY_VALUE_TYPE_NAME)
                    {
                        result += GenerateFilesUtils.GetIndentationStr(2, 4, oCompVar.Name + " = false;");
                    }
                    else if (oCompVar.Default != null)
                    {
                        result += GenerateFilesUtils.GetIndentationStr(2, 4, oCompVar.Name + " = " + oCompVar.Default + ";");
                    }
                }

                result += GenerateFilesUtils.GetIndentationStr(1, 4, "}") + Environment.NewLine;
            }
            return result;
        }
        public static string GetStateVarTypesCppFile(PLPsData data)
        {
            string file = @"#include <string>
#include <cstdlib>
#include <cmath>
#include <cassert>
#include <despot/model_primitives/" + data.ProjectName + @"/state_var_types.h> 


using namespace std;

namespace despot {
	

" + GetStateVarTypesCppConstructors(data) + @"

}// namespace despot
";
            return file;
        }

        private static HashSet<string> GetAnyValueVarNames(PLPsData data)
        {
            HashSet<string> varNames = new HashSet<string>();
            foreach (GlobalVariableDeclaration gVar in data.GlobalVariableDeclarations)
            {

                if (gVar.Type == PLPsData.ANY_VALUE_TYPE_NAME)
                {
                    varNames.Add("state." + gVar.Name);
                }
                else
                {
                    foreach (CompoundVarTypePLP oCompType in data.GlobalCompoundTypes.Where(x => x.TypeName.Equals(gVar.Type)))
                    {
                        GetCompoundTypeAnyValues(oCompType, "state." + gVar.Name + ".", varNames, data);
                    }
                }
            }
            return varNames;
        }

        private static void GetCompoundTypeAnyValues(CompoundVarTypePLP oCompType, string baseName, HashSet<string> varNames, PLPsData data)
        {
            foreach (CompoundVarTypePLP_Variable oVar in oCompType.Variables)
            {
                if (oVar.Type.Equals(PLPsData.ANY_VALUE_TYPE_NAME))
                {
                    varNames.Add(baseName + oVar.Name);
                }
                foreach (CompoundVarTypePLP ocCompType in data.GlobalCompoundTypes.Where(x => x.TypeName.Equals(oVar.Type)))
                {
                    GetCompoundTypeAnyValues(ocCompType, baseName + "." + oVar.Name + ".", varNames, data);
                }
            }
        }
        private static string GetStateCppUpdateDicInit(PLPsData data)
        {
            string result = "";
            HashSet<string> names = GetAnyValueVarNames(data);

            foreach (string name in names)
            {
                result += GenerateFilesUtils.GetIndentationStr(3, 4, "state->anyValueUpdateDic[\"" + name + "\"] = &(" + name.Replace("state.", "state->") + ");");
            }
            return result;
        }


        public static string GetStateCppFile(PLPsData data)
        {
            string file = @"#include <despot/model_primitives/" + data.ProjectName + @"/state.h> 
namespace despot {
	

" + GetStateVarTypesCppConstructors(data) + @"



		void " + data.ProjectNameWithCapitalLetter + @"State::SetAnyValueLinks(" + data.ProjectNameWithCapitalLetter + @"State *state)
		{
" + GetStateCppUpdateDicInit(data) + @"
		}
}// namespace despot";
            return file;
        }




        private static string GetPrintStateFunction(PLPsData data)
        {
            string result = GenerateFilesUtils.GetIndentationStr(1, 4, "std::string Prints::PrintState(" + data.ProjectNameWithCapitalLetter + @"State state)");
            result += GenerateFilesUtils.GetIndentationStr(1, 4, "{");
            result += GenerateFilesUtils.GetIndentationStr(2, 4, "stringstream ss;");
            result += GenerateFilesUtils.GetIndentationStr(2, 4, "ss << \"STATE: \";");
            foreach (GlobalVariableDeclaration oStateVar in data.GlobalVariableDeclarations)
            {
                if (data.GlobalCompoundTypes.Where(x => x.TypeName.Equals(oStateVar.Type)).Count() == 0)
                {
                    result += GenerateFilesUtils.GetIndentationStr(2, 4, "ss << \"|" + oStateVar.Name + ":\";");
                    if (data.GlobalEnumTypes.Where(x => x.TypeName.Equals(oStateVar.Type)).Count() > 0)
                    {
                        result += GenerateFilesUtils.GetIndentationStr(2, 4, "ss <<  Prints::Print" + oStateVar.Type + "(state." + oStateVar.Name + ");");
                    }
                    else
                    {
                        result += GenerateFilesUtils.GetIndentationStr(2, 4, "ss <<  state." + oStateVar.Name + ";");
                    }
                }
            }
            result += GenerateFilesUtils.GetIndentationStr(2, 4, "return ss.str();");
            result += GenerateFilesUtils.GetIndentationStr(1, 4, "}" + Environment.NewLine);
            return result;
        }
        private static string GetPrintActionDescriptionFunction(PLPsData data)
        {
            string result = GenerateFilesUtils.GetIndentationStr(1, 4, "std::string Prints::PrintActionDescription(ActionDescription* act)");
            result += GenerateFilesUtils.GetIndentationStr(1, 4, "{");
            result += GenerateFilesUtils.GetIndentationStr(2, 4, "stringstream ss;");
            result += GenerateFilesUtils.GetIndentationStr(2, 4, "ss << \"ID:\" << act->actionId;");
            result += GenerateFilesUtils.GetIndentationStr(2, 4, "ss << \",\" << PrintActionType(act->actionType);");
            foreach (PLP plp in data.PLPs.Values)
            {
                if (plp.GlobalVariableModuleParameters.Count > 0)
                {
                    result += GenerateFilesUtils.GetIndentationStr(2, 4, "if(act->actionType == " + plp.Name + "Action)");
                    result += GenerateFilesUtils.GetIndentationStr(2, 4, "{");

                    string actionDescVarName = plp.Name + "A";
                    result += GenerateFilesUtils.GetIndentationStr(3, 4, GenerateFilesUtils.ToUpperFirstLetter(plp.Name) + "ActionDescription *" + actionDescVarName +
                        " = static_cast<" + GenerateFilesUtils.ToUpperFirstLetter(plp.Name) + "ActionDescription *>(act);");
                    foreach (GlobalVariableModuleParameter oVar in plp.GlobalVariableModuleParameters)
                    {
                        List<CompoundVarTypePLP> compType = data.GlobalCompoundTypes.Where(x => x.TypeName.Equals(oVar.Type)).ToList();
                        if (compType.Count == 0)
                        {
                            result += GenerateFilesUtils.GetIndentationStr(3, 4, "ss << \",\" << " + actionDescVarName + "->" + oVar.Name + ";");
                        }
                        else
                        {
                            foreach (var constPar in compType[0].Variables)
                            {
                                bool primitive = GenerateFilesUtils.IsPrimitiveType(constPar.Type) || constPar.Type.Equals(PLPsData.ANY_VALUE_TYPE_NAME);
                                string print = primitive ? (actionDescVarName + "->" + oVar.Name + "." + constPar.Name) : ("Prints::Print" + constPar.Type + "((" + constPar.Type + ")" + actionDescVarName + "->" + oVar.Name + "." + constPar.Name + ");");
                                result += GenerateFilesUtils.GetIndentationStr(3, 4, "ss << \",\" << \"" + constPar.Name + ":\" << "+print+";");
                            }
                        }

                        //result += GenerateFilesUtils.GetIndentationStr(3, 4, "ss << \",\" << Prints::PrintLocation(("+oVar.Type+")"+plp.Name+"A->"+oVar.+" oDesiredLocation.discrete_location);");
                    }

                    result += GenerateFilesUtils.GetIndentationStr(2, 4, "}" + Environment.NewLine);
                }
            }

            result += GenerateFilesUtils.GetIndentationStr(2, 4, "return ss.str();");
            result += GenerateFilesUtils.GetIndentationStr(1, 4, "}");
            return result;
        }


        private static string GetPrintActionType(PLPsData data)
        {
            string result = @"";
            result += GenerateFilesUtils.GetIndentationStr(1, 4, "std::string Prints::PrintActionType(ActionType actType)");
            result += GenerateFilesUtils.GetIndentationStr(1, 4, "{");
            result += GenerateFilesUtils.GetIndentationStr(2, 4, "switch (actType)");
            result += GenerateFilesUtils.GetIndentationStr(2, 4, "{");
            foreach (string plpName in data.PLPs.Keys)
            {
                result += GenerateFilesUtils.GetIndentationStr(2, 4, "case " + plpName + "Action:");
                result += GenerateFilesUtils.GetIndentationStr(3, 4, "return \"" + plpName + "Action\";");
            }
            result += GenerateFilesUtils.GetIndentationStr(2, 4, "}");
            result += GenerateFilesUtils.GetIndentationStr(1, 4, "}");
            return result;
        }
        private static string GetGlobalVarEnumsPrintFunctions(PLPsData data)
        {
            string result = "";
            foreach (EnumVarTypePLP enumType in data.GlobalEnumTypes)
            {
                result += GenerateFilesUtils.GetIndentationStr(1, 4, "std::string Prints::Print" + enumType.TypeName + "(" + enumType.TypeName + " enumT)");
                result += GenerateFilesUtils.GetIndentationStr(1, 4, "{");
                result += GenerateFilesUtils.GetIndentationStr(2, 4, "switch (enumT)");
                result += GenerateFilesUtils.GetIndentationStr(2, 4, "{");
                foreach (string val in enumType.Values)
                {
                    result += GenerateFilesUtils.GetIndentationStr(3, 4, "case " + val + ":");
                    result += GenerateFilesUtils.GetIndentationStr(4, 4, "return \"" + val + "\";");
                }
                result += GenerateFilesUtils.GetIndentationStr(2, 4, "}");
                result += GenerateFilesUtils.GetIndentationStr(1, 4, "}" + Environment.NewLine);
            }
            return result;
        }

        private static string ReplacePLPEnumNamesToBeUnique(string codeLine, string fromFile)
        {
            if (EnumMappingsForModuleResponseAndTempVar == null)
            {
                throw new Exception("EnumMappingsForModuleResponseAndTempVar was not initialized!");
            }

            if (fromFile == PLPsData.PLP_TYPE_NAME_ENVIRONMENT)
            {
                return codeLine;
            }

            string code = codeLine;
            foreach (KeyValuePair<string, string> enumMapping in EnumMappingsForModuleResponseAndTempVar[fromFile])
            {
                code = code.Replace(enumMapping.Key, enumMapping.Value);
            }
            return code;
        }
        private static string HandleCodeLine(PLPsData data, string codeLine, string fromFile)
        {
            bool environmentFileCode = fromFile == PLPsData.PLP_TYPE_NAME_ENVIRONMENT;
            string code = codeLine;
            code = HandleC_Code_AOS_SingleVariableFunctions_str(code, PLPsData.AOS_INITIALIZED_FUNCTION_NAME);
            code = HandleC_Code_AOS_SingleVariableFunctions_str(code, PLPsData.AOS_UN_INITIALIZED_FUNCTION_NAME);
            code = HandleC_Code_AOS_SingleVariableFunctions_str(code, PLPsData.AOS_IS_INITIALIZED_FUNCTION_NAME);
            code = HandleC_Code_AOS_SingleVariableFunctions_str(code, PLPsData.AOS_Bernoulli_FUNCTION_NAME);
            code = HandleC_Code_AOS_SingleVariableFunctions_str(code, PLPsData.AOS_SET_NULL_FUNCTION_NAME);
            //code = HandleC_Code_IAOS_SetNull_str(code);


            foreach (string lowLevelConstantName in data.LocalVariableConstants.Select(x => x.Name))
            {
                code = code.Replace(lowLevelConstantName, "true");//lowLevelConstants are assigned as true regarding the global varible level 
            }
            foreach (DistributionSample dist in data.DistributionSamples.Values)
            {
                string replacementCode = "";
                switch (dist.Type)
                {
                    case DistributionType.Discrete:
                        if (environmentFileCode)
                        {
                            replacementCode = "state." + dist.Parameters[0] + "Objects[" + dist.C_VariableName + "(generator)];";
                        }
                        else
                        {


                            replacementCode = "(" + data.ProjectNameWithCapitalLetter + "ResponseModuleAndTempEnums)(" /*+ dist.FromFile + "_"*/ + dist.Parameters[0] + " + 1 + " + data.ProjectNameWithCapitalLetter + "::" + dist.C_VariableName + "(" + data.ProjectNameWithCapitalLetter + "::generator))";
                        }
                        break;
                    case DistributionType.Normal:
                        replacementCode = data.ProjectNameWithCapitalLetter + "::" + dist.C_VariableName + "(" + data.ProjectNameWithCapitalLetter + "::generator)";
                        break;
                }

                code = code.Replace(dist.FunctionDescription, replacementCode);
            }
            code = ReplacePLPEnumNamesToBeUnique(code, fromFile);
            code = code.Replace("else", "else ");
            return code;
        }

        private static string GetModelCppCreatStartStateFunction(PLPsData data)
        {
            string result = "";
            result += GenerateFilesUtils.GetIndentationStr(0, 4, "State* " + data.ProjectNameWithCapitalLetter + "::CreateStartState(string tyep) const {");
            result += GenerateFilesUtils.GetIndentationStr(1, 4, data.ProjectNameWithCapitalLetter + "State* startState = memory_pool_.Allocate();");
            result += GenerateFilesUtils.GetIndentationStr(1, 4, data.ProjectNameWithCapitalLetter + "State& state = *startState;");

            foreach (EnumVarTypePLP enumT in data.GlobalEnumTypes)
            {
                foreach (string val in enumT.Values)
                {
                    result += GenerateFilesUtils.GetIndentationStr(1, 4, "startState->" + enumT.TypeName + "Objects.push_back(" + val + ");");
                }
            }

            foreach (GlobalVariableDeclaration oVar in data.GlobalVariableDeclarations)
            {
                bool isCompundType = data.GlobalCompoundTypes.Any(x => x.TypeName.Equals(oVar.Type));
                if (oVar.Type == PLPsData.ANY_VALUE_TYPE_NAME)
                {
                    result += GenerateFilesUtils.GetIndentationStr(1, 4, "state." + oVar.Name + " = false;");
                }
                if (isCompundType)
                {
                    result += GenerateFilesUtils.GetIndentationStr(1, 4, "state." + oVar.Name + " = " + oVar.Type + "();");
                }
                if (oVar.Default != null)
                {
                    result += GenerateFilesUtils.GetIndentationStr(1, 4, "state." + oVar.Name + " = " + HandleCodeLine(data, oVar.Default, PLPsData.PLP_TYPE_NAME_ENVIRONMENT) + ";");
                }
                if (oVar.DefaultCode != null)
                {
                    foreach (string codeLine in oVar.DefaultCode.Split(";"))
                    {
                        if (codeLine.Length > 0)
                        {
                            result += GenerateFilesUtils.GetIndentationStr(1, 4, HandleCodeLine(data, codeLine, PLPsData.PLP_TYPE_NAME_ENVIRONMENT) + ";");
                        }
                    }
                }
            }

            HashSet<string> handeled = new HashSet<string>();
            foreach (GlobalVariableDeclaration gVarDec in data.GlobalVariableDeclarations)
            {
                if (gVarDec.IsActionParameterValue && handeled.Add(gVarDec.Type))
                {
                    foreach (GlobalVariableDeclaration gVarDec2 in data.GlobalVariableDeclarations.Where(x => x.Type.Equals(gVarDec.Type)))
                    {
                        result += GenerateFilesUtils.GetIndentationStr(1, 4, "startState->" + gVarDec.Type + "ObjectsForActions[\"state." +
                                gVarDec2.Name + "\"] = (state." + gVarDec2.Name + ");");
                    }
                }
            }

            foreach (Assignment assign in data.InitialBeliefAssignments)
            {
                result += GenerateFilesUtils.GetIndentationStr(1, 4, HandleCodeLine(data, assign.AssignmentCode, PLPsData.PLP_TYPE_NAME_ENVIRONMENT));
            }
            result += GenerateFilesUtils.GetIndentationStr(1, 4, "if (ActionManager::actions.size() == 0)");
            result += GenerateFilesUtils.GetIndentationStr(1, 4, "{");
            result += GenerateFilesUtils.GetIndentationStr(2, 4, "ActionManager::Init(const_cast <" + data.ProjectNameWithCapitalLetter + @"State*> (startState));");
            result += GenerateFilesUtils.GetIndentationStr(1, 4, "}");
            result += GenerateFilesUtils.GetIndentationStr(1, 4, "return startState;");
            result += GenerateFilesUtils.GetIndentationStr(0, 4, "}" + Environment.NewLine);
            return result;
        }



        // private static string HandleC_Code_IAOS_SetNull_str(string codeLine)
        // {
        //     string code = codeLine;
        //     string functionSign = PLPsData.AOS_SET_NULL_FUNCTION_NAME;
        //     bool found = false;
        //     do
        //     {
        //         int startIndex = code.IndexOf(functionSign);
        //         found = startIndex > -1;
        //         if (found)
        //         {
        //             int closeIndex = code.IndexOf(")", startIndex);
        //             string start = code.Substring(0, startIndex);
        //             string end = code.Substring(closeIndex + 1);
        //             string variableName = code.Substring(startIndex, closeIndex - startIndex).Replace(functionSign, "").Replace("(", "").Replace(")", "");
        //             code = start + variableName + " = false" + end;
        //         }
        //     } while (found);
        //     return code;
        // }

        //AOS_UN_INITIALIZED_NULL_FUNCTION_NAME
        private static string HandleC_Code_AOS_SingleVariableFunctions_str(string codeLine, string functionSignature)
        {
            string code = codeLine;
            //string functionSign = PLPsData.AOS_IS_INITIALIZED_FUNCTION_NAME;
            string template = "";
            string START = "__START__";
            string END = "__END__";
            string VARIABLE_NAME = "__VARIABLE_NAME__";
            switch (functionSignature)
            {
                case PLPsData.AOS_IS_INITIALIZED_FUNCTION_NAME:
                    template = START + "(" + VARIABLE_NAME + " == true)" + END;
                    break;
                case PLPsData.AOS_UN_INITIALIZED_FUNCTION_NAME:
                case PLPsData.AOS_SET_NULL_FUNCTION_NAME:
                    template = START + VARIABLE_NAME + " = false" + END;
                    break;
                case PLPsData.AOS_INITIALIZED_FUNCTION_NAME:
                    template = START + VARIABLE_NAME + " = true" + END;
                    break;
                case PLPsData.AOS_Bernoulli_FUNCTION_NAME:
                    return codeLine.Replace(PLPsData.AOS_Bernoulli_FUNCTION_NAME, "AOSUtils::Bernoulli");
            }
            bool found = false;
            do
            {
                int startIndex = code.IndexOf(functionSignature);
                found = startIndex > -1;
                if (found)
                {
                    int closeIndex = code.IndexOf(")", startIndex);
                    string start = code.Substring(0, startIndex);
                    string end = code.Substring(closeIndex + 1);
                    string variableName = code.Substring(startIndex, closeIndex - startIndex).Replace(functionSignature, "").Replace("(", "").Replace(")", "");
                    code = template.Replace(START, start).Replace(VARIABLE_NAME, variableName).Replace(END, end);
                    //code = start + "(" + variableName + " == true)" + end;
                }
            } while (found);
            return code;
        }
        private static string GetCheckPreconditionsForModelCpp(PLPsData data)
        {
            string result = GenerateFilesUtils.GetIndentationStr(0, 4, "void " + data.ProjectNameWithCapitalLetter + @"::CheckPreconditions(const " + data.ProjectNameWithCapitalLetter + "State& state, double &reward, bool &meetPrecondition, int actionId) const");
            result += GenerateFilesUtils.GetIndentationStr(1, 4, "{");
            result += GenerateFilesUtils.GetIndentationStr(2, 4, "ActionType &actType = ActionManager::actions[actionId]->actionType;");
            result += GenerateFilesUtils.GetIndentationStr(2, 4, "meetPrecondition = false;");
            foreach (PLP plp in data.PLPs.Values)
            {
                result += GenerateFilesUtils.GetIndentationStr(3, 4, "if(actType == " + plp.Name + "Action)");
                result += GenerateFilesUtils.GetIndentationStr(3, 4, "{");

                result += GenerateFilesUtils.GetIndentationStr(4, 4, "if(" + HandleCodeLine(data, plp.Preconditions_GlobalVariableConditionCode, plp.Name) + " && " + HandleCodeLine(data, plp.Preconditions_PlannerAssistancePreconditions, plp.Name) + ")");
                result += GenerateFilesUtils.GetIndentationStr(4, 4, "{");
                result += GenerateFilesUtils.GetIndentationStr(5, 4, "meetPrecondition = true;");
                result += GenerateFilesUtils.GetIndentationStr(4, 4, "}");
                result += GenerateFilesUtils.GetIndentationStr(4, 4, "else");
                result += GenerateFilesUtils.GetIndentationStr(4, 4, "{");
                result += GenerateFilesUtils.GetIndentationStr(5, 4, "reward += " + plp.Preconditions_ViolatingPreconditionPenalty + ";");
                result += GenerateFilesUtils.GetIndentationStr(4, 4, "}");


                result += GenerateFilesUtils.GetIndentationStr(3, 4, "}");
            }

            result += GenerateFilesUtils.GetIndentationStr(1, 4, "}");


            return result;
        }
        private static string GetModelCppFileDistributionVariableDefinition(PLPsData data)
        {
            double temp;
            string result = "";
            foreach (DistributionSample dist in data.DistributionSamples.Values)
            {
                switch (dist.Type)
                {
                    case DistributionType.Normal:
                        //std::normal_distribution<double> Icaps::normal_dist1(40000,10000); 


                        result += GenerateFilesUtils.GetIndentationStr(0, 4, "std::normal_distribution<double>  " +
                            data.ProjectNameWithCapitalLetter + "::" + dist.C_VariableName +
                                "(" + String.Join(",", dist.Parameters) + "); //" + dist.FunctionDescription);
                        break;
                    case DistributionType.Discrete:
                        //std::discrete_distribution<> Icaps::discrete_dist1{0.6, 0.4,0,0};
                        result += GenerateFilesUtils.GetIndentationStr(0, 4, "std::discrete_distribution<> " + data.ProjectNameWithCapitalLetter + "::" + dist.C_VariableName + "{" + String.Join(",", dist.Parameters.Where(x => double.TryParse(x, out temp))) + "}; //" + dist.FunctionDescription);
                        break;
                    case DistributionType.Uniform:
                        throw new NotImplementedException("Uniform distribution is not supported yet, remove '" + dist.FunctionDescription + "'!");
                }
            }
            return result;
        }

        private static string GetProcessSpecialStatesFunction(PLPsData data)
        {
            string result = "";
            result += GenerateFilesUtils.GetIndentationStr(0, 4, "bool " + data.ProjectNameWithCapitalLetter + "::ProcessSpecialStates(const " + data.ProjectNameWithCapitalLetter + "State &state, double &reward) const");
            result += GenerateFilesUtils.GetIndentationStr(0, 4, "{");
            result += GenerateFilesUtils.GetIndentationStr(1, 4, "bool isFinalState = false;");
            foreach (SpecialState spState in data.SpecialStates)
            {
                result += GenerateFilesUtils.GetIndentationStr(1, 4, "if (" + HandleCodeLine(data, spState.StateConditionCode, PLPsData.PLP_TYPE_NAME_ENVIRONMENT) + ")");
                result += GenerateFilesUtils.GetIndentationStr(1, 4, "{");
                result += GenerateFilesUtils.GetIndentationStr(2, 4, "reward += " + spState.Reward + ";");
                result += GenerateFilesUtils.GetIndentationStr(2, 4, "isFinalState = " + spState.IsGoalState.ToString().ToLower() + ";");
                result += GenerateFilesUtils.GetIndentationStr(1, 4, "}");
            }

            result += GenerateFilesUtils.GetIndentationStr(1, 4, "return isFinalState;");
            result += GenerateFilesUtils.GetIndentationStr(0, 4, "}");

            return result;
        }

        private static string GetAssignmentsCode(PLPsData data, string fromFile, List<Assignment> assignments, int indentCount, int indentSize)
        {
            if (EnumMappingsForModuleResponseAndTempVar == null)
            {
                throw new Exception("'EnumMappingsForModuleResponseAndTempVar' not initialized!");
            }

            string result = "";
            /*NavigateActionDescription act = *(static_cast<NavigateActionDescription *>(ActionManager::actions[actionId]));
			
			tLocation &oDesiredLocation = act.oDesiredLocation;*/
            if (data.PLPs.ContainsKey(fromFile) && data.PLPs[fromFile].GlobalVariableModuleParameters.Count > 0)
            {
                string upperPlpName = GenerateFilesUtils.ToUpperFirstLetter(fromFile);
                result += GenerateFilesUtils.GetIndentationStr(indentCount, indentSize, upperPlpName +
                        "ActionDescription act = *(static_cast<" + upperPlpName + "ActionDescription *>(ActionManager::actions[actionId]));");
                foreach (GlobalVariableModuleParameter oPar in data.PLPs[fromFile].GlobalVariableModuleParameters)
                {
                    result += GenerateFilesUtils.GetIndentationStr(indentCount, indentSize, oPar.Type + " &" + oPar.Name + " = act." + oPar.Name + ";");
                }
            }

            foreach (Assignment assign in assignments)
            {
                if (assign.TempVariable.Type != null)
                {
                    string TempVarType = assign.TempVariable.Type == PLPsData.ENUM_VARIABLE_TYPE_NAME ? assign.TempVariable.EnumName : assign.TempVariable.Type;
                    result += GenerateFilesUtils.GetIndentationStr(indentCount, indentSize, data.ProjectNameWithCapitalLetter + "ResponseModuleAndTempEnums " + assign.TempVariable.VariableName + ";");
                }
                foreach (string codeLine in assign.AssignmentCode.Split(";"))
                {
                    if (codeLine.Length > 0)
                    {
                        result += GenerateFilesUtils.GetIndentationStr(indentCount, indentSize, HandleCodeLine(data, codeLine, fromFile) + ";");
                    }
                }
            }
            return result;
        }
        private static string GetModuleDynamicModelFunction(PLPsData data)
        {
            string result = "";
            result += GenerateFilesUtils.GetIndentationStr(0, 4, "void " + data.ProjectNameWithCapitalLetter + "::ModuleDynamicModel(const " +
                data.ProjectNameWithCapitalLetter + "State &state, const " + data.ProjectNameWithCapitalLetter + @"State &state_, " +
                data.ProjectNameWithCapitalLetter + "State &state__, double rand_num, int actionId, double &__reward, OBS_TYPE &observation, const int &__moduleExecutionTime) const");
            result += GenerateFilesUtils.GetIndentationStr(0, 4, "{");


            result += GenerateFilesUtils.GetIndentationStr(1, 4, "ActionType &actType = ActionManager::actions[actionId]->actionType;");
            result += GenerateFilesUtils.GetIndentationStr(1, 4, "observation = -1;");
            result += GenerateFilesUtils.GetIndentationStr(1, 4, "int startObs = observation;");
            result += GenerateFilesUtils.GetIndentationStr(1, 4, "OBS_TYPE &__moduleResponse = observation;");
            foreach (PLP plp in data.PLPs.Values)
            {
                result += GenerateFilesUtils.GetIndentationStr(1, 4, "if(actType == " + plp.Name + "Action)");
                result += GenerateFilesUtils.GetIndentationStr(1, 4, "{");

                result += GetAssignmentsCode(data, plp.Name, plp.DynamicModel_VariableAssignments, 2, 4);
                /*foreach (Assignment assign in plp.DynamicModel_VariableAssignments)
                {
                    if (assign.TempVariable.Type != null)
                    {
                        string TempVarType = assign.TempVariable.Type == PLPsData.ENUM_VARIABLE_TYPE_NAME ? assign.TempVariable.EnumName : assign.TempVariable.Type;
                        result += GenerateFilesUtils.GetIndentationStr(2, 4, TempVarType + " " + assign.TempVariable.VariableName);
                    }
                    foreach (string codeLine in assign.AssignmentCode.Split(";"))
                    {
                        if (codeLine.Length > 0)
                        {
                            result += GenerateFilesUtils.GetIndentationStr(2, 4, HandleCodeLine(data, codeLine, plp.Name) + ";");
                        }
                    }
                }*/
                result += GenerateFilesUtils.GetIndentationStr(1, 4, "}");
            }

            result += GenerateFilesUtils.GetIndentationStr(1, 4, "if(startObs == __moduleResponse)");
            result += GenerateFilesUtils.GetIndentationStr(1, 4, "{");
            result += GenerateFilesUtils.GetIndentationStr(1, 4, "stringstream ss;");
            result += GenerateFilesUtils.GetIndentationStr(1, 4, "ss << \"Observation/__moduleResponse Not initialized!!! on action:\" << Prints::PrintActionDescription(ActionManager::actions[actionId]) << endl;");
            result += GenerateFilesUtils.GetIndentationStr(1, 4, "loge << ss.str() << endl;");
            result += GenerateFilesUtils.GetIndentationStr(1, 4, "throw 1;");
            result += GenerateFilesUtils.GetIndentationStr(1, 4, "}");
            result += GenerateFilesUtils.GetIndentationStr(0, 4, "}");
            return result;
        }
        private static string GetExtrinsicChangesDynamicModelFunction(PLPsData data)
        {
            string result = "";
            result += GenerateFilesUtils.GetIndentationStr(0, 4, "void " + data.ProjectNameWithCapitalLetter + @"::ExtrinsicChangesDynamicModel(const " + data.ProjectNameWithCapitalLetter + @"State& state, " + data.ProjectNameWithCapitalLetter + @"State& state_, double rand_num, int actionId, double& reward, const int &__moduleExecutionTime)  const");
            result += GenerateFilesUtils.GetIndentationStr(0, 4, "{");
            result += GenerateFilesUtils.GetIndentationStr(1, 4, "ActionType &actType = ActionManager::actions[actionId]->actionType;");
            foreach (Assignment assign in data.ExtrinsicChangesDynamicModel)
            {
                foreach (string codeLine in assign.AssignmentCode.Split(";"))
                {
                    if (codeLine.Length > 0)
                    {
                        result += GenerateFilesUtils.GetIndentationStr(1, 4, HandleCodeLine(data, codeLine, PLPsData.PLP_TYPE_NAME_ENVIRONMENT) + ";");
                    }
                }
            }
            result += GenerateFilesUtils.GetIndentationStr(0, 4, "}");
            return result;
        }

        private static string GetSampleModuleExecutionTimeFunction(PLPsData data)
        {
            string result = "";
            result += GenerateFilesUtils.GetIndentationStr(0, 4, "void " + data.ProjectNameWithCapitalLetter + "::SampleModuleExecutionTime(const " + data.ProjectNameWithCapitalLetter + "State& farstate, double rand_num, int actionId, int &__moduleExecutionTime) const");
            result += GenerateFilesUtils.GetIndentationStr(0, 4, "{");
            result += GenerateFilesUtils.GetIndentationStr(1, 4, "ActionType &actType = ActionManager::actions[actionId]->actionType;");
            foreach (PLP plp in data.PLPs.Values)
            {
                result += GenerateFilesUtils.GetIndentationStr(1, 4, "if(actType == " + plp.Name + "Action)");
                result += GenerateFilesUtils.GetIndentationStr(1, 4, "{");
                foreach (Assignment assign in plp.ModuleExecutionTimeDynamicModel)
                {
                    foreach (string codeLine in assign.AssignmentCode.Split(";"))
                    {
                        if (codeLine.Length > 0)
                        {
                            result += GenerateFilesUtils.GetIndentationStr(2, 4, HandleCodeLine(data, codeLine, plp.Name) + ";");
                        }
                    }
                }
                result += GenerateFilesUtils.GetIndentationStr(1, 4, "}");
            }
            result += GenerateFilesUtils.GetIndentationStr(0, 4, "}");
            return result;
        }

        public static string GetModelCppFile(PLPsData data)
        {
            string file = @"#include """ + data.ProjectName + @".h""
#include <despot/core/pomdp.h> 
#include <stdlib.h>
#include <despot/solver/pomcp.h>
#include <sstream>
#include <despot/model_primitives/" + data.ProjectName + @"/actionManager.h> 
#include <despot/model_primitives/" + data.ProjectName + @"/enum_map_" + data.ProjectName + @".h> 
#include <despot/model_primitives/" + data.ProjectName + @"/state.h> 

using namespace std;

namespace despot {


bool AOSUtils::Bernoulli(double p)
{
	/* generate secret number between 1 and 100: */
	int randInt = rand() % 100 + 1;
	return (p * 100) >= randInt;
}

/* ==============================================================================
 *" + data.ProjectNameWithCapitalLetter + @"Belief class
 * ==============================================================================*/
int " + data.ProjectNameWithCapitalLetter + @"Belief::num_particles = 5000;


" + data.ProjectNameWithCapitalLetter + @"Belief::" + data.ProjectNameWithCapitalLetter + @"Belief(vector<State*> particles, const DSPOMDP* model,
	Belief* prior) :
	ParticleBelief(particles, model, prior),
	" + data.ProjectName + @"_(static_cast<const " + data.ProjectNameWithCapitalLetter + @"*>(model)) {
}
 
" + GetGlobalVarEnumsPrintFunctions(data) + GetPrintActionDescriptionFunction(data) + @"

std::string Prints::PrintObs(int action, int obs)
{
	" + data.ProjectNameWithCapitalLetter + @"ResponseModuleAndTempEnums eObs = (" + data.ProjectNameWithCapitalLetter + @"ResponseModuleAndTempEnums)obs;
	return enum_map_" + data.ProjectName + @"::vecResponseEnumToString[eObs]; 
}
" + GetPrintStateFunction(data) + @"
	
 	std::string " + data.ProjectNameWithCapitalLetter + @"::GetActionDescription(int actionId) const
	 {
		 return Prints::PrintActionDescription(ActionManager::actions[actionId]);
	 }

" + GetPrintActionType(data) + @"


void " + data.ProjectNameWithCapitalLetter + @"Belief::Update(int actionId, OBS_TYPE obs, std::map<std::string,bool> updates) {
	history_.Add(actionId, obs);

	vector<State*> updated;
	double reward;
	OBS_TYPE o;
	int cur = 0, N = particles_.size(), trials = 0;
	while (updated.size() < num_particles && trials < 10 * num_particles) {
		State* particle = " + data.ProjectName + @"_->Copy(particles_[cur]);
		bool terminal = " + data.ProjectName + @"_->Step(*particle, Random::RANDOM.NextDouble(),
			actionId, reward, o);
 
		if (!terminal && o == obs) 
			{
				" + data.ProjectNameWithCapitalLetter + @"State &" + data.ProjectName + @"_particle = static_cast<" + data.ProjectNameWithCapitalLetter + @"State &>(*particle);
				if(!Globals::IsInternalSimulation() && updates.size() > 0)
				{
					" + data.ProjectNameWithCapitalLetter + @"State::SetAnyValueLinks(&" + data.ProjectName + @"_particle);
					map<std::string, bool>::iterator it;
					for (it = updates.begin(); it != updates.end(); it++)
					{
						*(" + data.ProjectName + @"_particle.anyValueUpdateDic[it->first]) = it->second; 
					} 
				}
				updated.push_back(particle);
		} else {
			" + data.ProjectName + @"_->Free(particle);
		}

		cur = (cur + 1) % N;

		trials++;
	}

	for (int i = 0; i < particles_.size(); i++)
		" + data.ProjectName + @"_->Free(particles_[i]);

	particles_ = updated;

	for (int i = 0; i < particles_.size(); i++)
		particles_[i]->weight = 1.0 / particles_.size();
}

/* ==============================================================================
 * " + data.ProjectNameWithCapitalLetter + @" class
 * ==============================================================================*/

" + data.ProjectNameWithCapitalLetter + @"::" + data.ProjectNameWithCapitalLetter + @"(){
	
}

int " + data.ProjectNameWithCapitalLetter + @"::NumActions() const {
	return ActionManager::actions.size();
}

double " + data.ProjectNameWithCapitalLetter + @"::ObsProb(OBS_TYPE obs, const State& state, int actionId) const {
	return 0.9;
}

 


std::default_random_engine " + data.ProjectNameWithCapitalLetter + @"::generator;

" + GetModelCppFileDistributionVariableDefinition(data) + Environment.NewLine + GetModelCppCreatStartStateFunction(data) + @"

Belief* " + data.ProjectNameWithCapitalLetter + @"::InitialBelief(const State* start, string type) const {
	int N = " + data.ProjectNameWithCapitalLetter + @"Belief::num_particles;
	vector<State*> particles(N);
	for (int i = 0; i < N; i++) {
		particles[i] = CreateStartState();
		particles[i]->weight = 1.0 / N;
	}

	return new " + data.ProjectNameWithCapitalLetter + @"Belief(particles, this);
}
 

 

POMCPPrior* " + data.ProjectNameWithCapitalLetter + @"::CreatePOMCPPrior(string name) const { 
		return new UniformPOMCPPrior(this);
}

void " + data.ProjectNameWithCapitalLetter + @"::PrintState(const State& state, ostream& ostr) const {
	const " + data.ProjectNameWithCapitalLetter + @"State& farstate = static_cast<const " + data.ProjectNameWithCapitalLetter + @"State&>(state);
	if (ostr)
		ostr << Prints::PrintState(farstate);
}

void " + data.ProjectNameWithCapitalLetter + @"::PrintObs(const State& state, OBS_TYPE observation,
	ostream& ostr) const {
	const " + data.ProjectNameWithCapitalLetter + @"State& farstate = static_cast<const " + data.ProjectNameWithCapitalLetter + @"State&>(state);
	
	ostr << observation <<endl;
}

void " + data.ProjectNameWithCapitalLetter + @"::PrintBelief(const Belief& belief, ostream& out) const {
	 out << ""called PrintBelief(): b printed""<<endl;
		out << endl;
	
}

void " + data.ProjectNameWithCapitalLetter + @"::PrintAction(int actionId, ostream& out) const {
	out << Prints::PrintActionDescription(ActionManager::actions[actionId]) << endl;
}

State* " + data.ProjectNameWithCapitalLetter + @"::Allocate(int state_id, double weight) const {
	" + data.ProjectNameWithCapitalLetter + @"State* state = memory_pool_.Allocate();
	state->state_id = state_id;
	state->weight = weight;
	return state;
}

State* " + data.ProjectNameWithCapitalLetter + @"::Copy(const State* particle) const {
	" + data.ProjectNameWithCapitalLetter + @"State* state = memory_pool_.Allocate();
	*state = *static_cast<const " + data.ProjectNameWithCapitalLetter + @"State*>(particle);
	state->SetAllocated();
	return state;
}

void " + data.ProjectNameWithCapitalLetter + @"::Free(State* particle) const {
	memory_pool_.Free(static_cast<" + data.ProjectNameWithCapitalLetter + @"State*>(particle));
}

int " + data.ProjectNameWithCapitalLetter + @"::NumActiveParticles() const {
	return memory_pool_.num_allocated();
}

bool " + data.ProjectNameWithCapitalLetter + @"::Step(State& s_state__, double rand_num, int actionId, double& reward,
	OBS_TYPE& observation) const {
	bool isNextStateFinal = false;
	Random random(rand_num);
	int __moduleExecutionTime = -1;
	bool meetPrecondition = false;
	
	" + data.ProjectNameWithCapitalLetter + @"State &state__ = static_cast<" + data.ProjectNameWithCapitalLetter + @"State &>(s_state__);
	 logd << ""[" + data.ProjectNameWithCapitalLetter + @"::Step] Selected Action:"" << Prints::PrintActionDescription(ActionManager::actions[actionId]) << ""||State""<< Prints::PrintState(state__);
	CheckPreconditions(state__, reward, meetPrecondition, actionId);
	if (!meetPrecondition)
	{
		__moduleExecutionTime = 0;
		observation = illegalActionObs;
		return false;
	}

	State *s_state = Copy(&s_state__);
	" + data.ProjectNameWithCapitalLetter + @"State &state = static_cast<" + data.ProjectNameWithCapitalLetter + @"State &>(*s_state);

	
	SampleModuleExecutionTime(state__, rand_num, actionId, __moduleExecutionTime);

	ExtrinsicChangesDynamicModel(state, state__, rand_num, actionId, reward, __moduleExecutionTime);

	State *s_state_ = Copy(&s_state__);
	" + data.ProjectNameWithCapitalLetter + @"State &state_ = static_cast<" + data.ProjectNameWithCapitalLetter + @"State &>(*s_state_);

	ModuleDynamicModel(state, state_, state__, rand_num, actionId, reward,
					   observation, __moduleExecutionTime);
	
	Free(s_state);
	Free(s_state_);
	bool finalState = ProcessSpecialStates(state__, reward);
	return finalState;
}

" + GetCheckPreconditionsForModelCpp(data) + Environment.NewLine +
 GetSampleModuleExecutionTimeFunction(data) + Environment.NewLine +
 GetExtrinsicChangesDynamicModelFunction(data) + Environment.NewLine +
 GetModuleDynamicModelFunction(data) + Environment.NewLine +
 GetProcessSpecialStatesFunction(data) + Environment.NewLine + @"



std::string " + data.ProjectNameWithCapitalLetter + @"::PrintObs(int action, OBS_TYPE obs) const 
{
	return Prints::PrintObs(action, obs);
}

std::string " + data.ProjectNameWithCapitalLetter + @"::PrintStateStr(const State &state) const { return """"; };
}// namespace despot
";

            return file;
        }
    }


}