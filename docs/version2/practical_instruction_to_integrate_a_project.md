# Practical instruction to integrate a project
## Bottom-Up Steps: first, connect the AOS to the robot skills, then make sure you are getting a good policy
*Document your environment and SD files. Write only the needed section to activate the robot skills, such as:
  - The state variables that are marked as "IsActionParameterValue" in the environment file.
  - For each skill, write an SD file with the skill parameters and a minimum dynamic model section (arbitrary assignments for the reward and observation)
  - Write the AM files.

* Send an integration request for executing internal simulation.
* Send a get possible actions request
  - See that the action list with their parameters is as you expected. 
* Send an integration request for robot execution with an array of action IDs you want to inspect (use the "ActionsToSimulate" field).
* If all is well, document your skills behavior in the SD files
* Send an integration request for executing internal simulation (without "ActionsToSimulate").
* Send a get execution outcome request
  - If the result is not as expected, start debugging (see instruction below).
 
## Top Down Steps:first a good solver policy, then connect the robot skills
* Document your environment and SD files.
* Send an integration request for internal simulation.
  - You will see that the documentation structure is legal.
  - You can now check that the list of possible actions is as you planned.
  - You can now check the execution outcome of the simulation.
* Send a get possible actions request
  - See that the action list with their parameters is as you expected. 
* Send a get execution outcome request
  - If the result is not as expected, start debugging (see instruction below).



# Debug the AOS-Solver using VSCode (recommended)
* Use Postman to send an integration request with the "OnlyGenerateCode," and the "SolverConfiguration.IsInternalSimulation" flag to 'true.' Send the request every time you change your documentation and want to debug the changes.

* Open VSCode.
  - Click: File->Open Folder
  - Select the `~/AOS/AOS-Solver` folder

Your project domain is located in `/home/or/AOS/AOS-Solver/examples/cpp_models/<your project name>/src/<your project name>.cpp`</br>

Now you can debug the project.</br>

### Debug your rollout policy:
In your domain C++ file, the `ComputePreference` function computes the value of your rollout policy. You can see that it acts well by adding a break point with conditions on the state you want to evaluate.

### Debug the effects of skills and extrinsic actions:
The `Step` function inside your C++ domain file contains your domain dynamic model, reward model, and observation model (it uses the functions `CheckPreconditions`, `ModuleDynamicModel`, and `ProcessSpecialStates`). 

## Blackbox AOS-Solver debug using the HTTP requests
1. Send an integration request with "SolverConfiguration.IsInternalSimulation".
2. Send Get  possible actions.
4. Send an integration request with "SolverConfiguration.IsInternalSimulation" and with "SolverConfiguration.ActionsToSimulate" that contains an array of actions IDs of actions you want to validate their documentation (they will be executed by their order).
5.  Send an execution outcome request and see that the actions change the state as expcted. If not, update the documentation and do step 4-5 again.
