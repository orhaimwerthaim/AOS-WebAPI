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
            string buildOutput;
            string runOutput;
            if(initProj.DebugConfiguration == null)
            {
                initProj.DebugConfiguration = new DebugConfiguration();
                initProj.DebugConfiguration.ActionsToSimulate = new List<int>(); 
                initProj.DebugConfiguration.DebugOn = false;
            }

            InitializeProjectBL.InitializeProject(initProj, out errors, out buildOutput, out runOutput);
            if(errors.Count > 0)
            {
                return BadRequest(new {Errors = errors});
            }
            return CreatedAtAction(nameof(Create), new {BuildOutput = buildOutput, RunOutput = runOutput});
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

            LocalVariableService.Delete(new LocalVariable() { Id = id });

            return NoContent();
        }
    }
}
