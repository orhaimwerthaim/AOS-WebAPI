using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;  
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WebApiCSharp.Models;
using WebApiCSharp.Services;

namespace WebApiCSharp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SolverController : ControllerBase
    { 

        private readonly ILogger<SolverController> _logger;

        public SolverController(ILogger<SolverController> logger)
        {
            _logger = logger;
        }


        [HttpGet]
        public List<Solver> GetAll()
        {
            return SolversService.Get();
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

        [HttpDelete]
        public IActionResult DeleteAll()
        {
            SolversService.RemoveAllSolversAndGetNextSolverID();

            return NoContent();
        }
    }
}
