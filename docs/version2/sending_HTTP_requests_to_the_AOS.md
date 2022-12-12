
# The AOS-Web API

* [prerequisites](#prerequisites)
* [Run project request](#run-project-request)
 

 
### prerequisites:
Install the AOS ([see instructions](https://github.com/orhaimwerthaim/AOS-WebAPI/blob/master/README.md#aos-installtion)).
Download [Postman](https://www.postman.com/downloads/).
Using Postman import the `AOS.postman_collection.json` collection ([see location](https://github.com/orhaimwerthaim/AOS-WebAPI/tree/master/docs/version2)). 

# Run project request
Open the collection, click on the "Run project" request, and click on the "Body" tab to see the request body.

You will see the following JSON:
```
{ 
    "PLPsDirectoryPath":"/home/or/Downloads/AOS-mini-project-main/printHelloWorld",
    "OnlyGenerateCode":false,
    "RunWithoutRebuild":false,
    "RosTarget":
    {
        "RosDistribution":"noetic",
        "WorkspaceDirectortyPath":"/home/or/catkin_ws",
        "TargetProjectLaunchFile":"/home/or/catkin_ws/src/letter_printer/launch/servers.launch",
        "RosTargetProjectPackages":["letter_printer"],
        "TargetProjectInitializationTimeInSeconds":10
    },
    "SolverConfiguration":{  
        "PolicyGraphDepth":0,
        "LoadBeliefFromDB":false,
        "DebugOn":true,
        "NumOfParticles":3,
        "NumOfBeliefStateParticlesToSaveInDB":10,
        "Verbosity":true,
        "ActionsToSimulate":[],
        "IsInternalSimulation":false,
        "PlanningTimePerMoveInSeconds":0.5
        },
    "MiddlewareConfiguration":{
        "DebugOn":true
         
    }
}
```

