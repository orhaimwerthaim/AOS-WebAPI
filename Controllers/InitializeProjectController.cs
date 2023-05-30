using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WebApiCSharp.Models;
using WebApiCSharp.Services;
using WebApiCSharp.BL;
using System.Diagnostics;
using System.Text;
using System.IO;
using System.Threading;
using System.ComponentModel;

namespace WebApiCSharp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class InitializeProjectController : ControllerBase
    {

                private readonly ILogger<InitializeProjectController> _logger;

        public InitializeProjectController(ILogger<InitializeProjectController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public IActionResult Create(InitializeProject initProj)
        { 
            List<string> errors = new List<string>();
            List<string> remarks = new List<string>();
            string buildSolverOutput;
            string buildRosMiddlewareOutput;
            if(initProj.MiddlewareConfiguration == null)
            {
                initProj.MiddlewareConfiguration = new MiddlewareConfiguration();
                initProj.MiddlewareConfiguration.KillRosCoreBeforeStarting=true;
            }
            initProj.MiddlewareConfiguration = (initProj.MiddlewareConfiguration == null) ? new MiddlewareConfiguration() : initProj.MiddlewareConfiguration;
            initProj.SolverConfiguration = (initProj.SolverConfiguration == null) ? new SolverConfiguration() : initProj.SolverConfiguration;
            initProj.RunWithoutRebuild = initProj.RunWithoutRebuild == null ? false : initProj.RunWithoutRebuild;
            initProj.OnlyGenerateCode ??= false;
            initProj.RosTarget.RosDistribution ??= "noetic";//"kinetic";
            initProj.SolverConfiguration.NumOfBeliefStateParticlesToSaveInDB =
                initProj.SolverConfiguration.NumOfBeliefStateParticlesToSaveInDB > initProj.SolverConfiguration.NumOfParticles ?
                    initProj.SolverConfiguration.NumOfParticles :
                    initProj.SolverConfiguration.NumOfBeliefStateParticlesToSaveInDB;
            if(initProj.RosTarget != null)
            {
                initProj.RosTarget.TargetProjectInitializationTimeInSeconds ??= 5;
            }

            if(initProj.SolverConfiguration.LoadBeliefFromDB && BeliefStateService.GetNumOfStatesSavedInCurrentBelief() == 0)
            {
                errors.Add("The request contains SolverConfiguration.LoadBeliefFromDB=='true', but there is no saved belief.");
                return StatusCode(501, new { Errors = errors, Remarks = remarks });
            }
            InitializeProjectBL.InitializeProject(initProj, out errors, out remarks, out buildSolverOutput, out buildRosMiddlewareOutput);
            foreach(string error in errors) 
            {
                LogMessageService.Add(new LogMessagePost()
                    {Component="WebAPI", Event= error, LogLevelDesc="Error", LogLevel=2});
            }
            if(errors.Count > 0)
            {
                return BadRequest(new {Errors = errors, Remarks = remarks});
            }
            return CreatedAtAction(nameof(Create), new {Remarks = remarks, BuildSolverOutput = buildSolverOutput,BuildRosMiddlewareOutput = buildRosMiddlewareOutput});
        }

        

        [HttpPut("{id}")]
        public IActionResult Update(LocalVariable localVar)
        {
            LocalVariableService.Update(localVar);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var lVar = LocalVariableService.Get(id);

            if (lVar is null)
                return NotFound();

         //   LocalVariableService.Delete(new LocalVariable() { Id = id });

            return NoContent();
        }
    }
}
