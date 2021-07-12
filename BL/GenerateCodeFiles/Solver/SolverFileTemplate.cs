using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using WebApiCSharp.Services;
using WebApiCSharp.Models;
using System.IO;
using System.Collections.Generic;
namespace WebApiCSharp.GenerateCodeFiles
{
    public class SolverFileTemplate
    {
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
    }
}