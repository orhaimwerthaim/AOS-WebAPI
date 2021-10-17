using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;  
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WebApiCSharp.Models;
using WebApiCSharp.Services;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Text.Json;

namespace WebApiCSharp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ExecutionOutcomeController : ControllerBase
    { 

        private readonly ILogger<ExecutionOutcomeController> _logger;

        public ExecutionOutcomeController(ILogger<ExecutionOutcomeController> logger)
        {
            _logger = logger;
        }


        [HttpGet] 
        public dynamic Get(string belief_size)
        {
            int _belief_size = string.IsNullOrEmpty(belief_size) ? 0 : Convert.ToInt32(belief_size); 
            this.Response.ContentType = "application/json";
            
            string jsonString = BeliefStateService.GetBeliefForExecution(0, _belief_size, 0).ToJson();
            int ind = jsonString.IndexOf("\"BeliefeState");
            jsonString = ind > -1 ? jsonString.Substring(ind) : "\"BeliefeState\":[]}]";
            jsonString = "{"+jsonString.Replace("BeliefeState", "InitialBeliefeState");
            jsonString = jsonString.Substring(0, jsonString.LastIndexOf(']'));


            jsonString = ExecutionOutcomeService.Get(_belief_size, jsonString);
            //jsonString = jsonString + ", " + jsonString2 + "}";
            return Content(jsonString);
        }


        // public IEnumerable<Solver> Get()
        // {
        //     var rng = new Random();
        //     return Enumerable.Range(1, 5).Select(index => new Solver
        //     {
        //         Name="2"
        //     })
        //     .ToArray();
        // }

        [HttpGet("{id}")]
        public ActionResult<Solver> Get(int id)
        {
            var solver = SolversService.Get(id);
            if(solver == null)
                return NotFound();
            return solver;
        }

        [HttpPost]
        public IActionResult Create(Solver solver)
        {
            Solver n = SolversService.Add(solver);
            if(n == null)
                return Conflict();
            return CreatedAtAction(nameof(Create), n);
        }

        [HttpPut("{id}")]
        public IActionResult Update(Solver solver)
        {
            SolversService.Update(solver);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var lVar = SolversService.Get(id);

            if (lVar is null)
                return NotFound();

            SolversService.Delete(new Solver() { SolverId = id });

            return NoContent();
        }
    }
}
