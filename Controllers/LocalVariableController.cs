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
    public class LocalVariableController : ControllerBase
    { 

        private readonly ILogger<LocalVariableController> _logger;

        public LocalVariableController(ILogger<LocalVariableController> logger)
        {
            _logger = logger;
        }


        [HttpGet]
        public List<LocalVariable> GetAll()
        {
            return LocalVariableService.Get();
        }
        // public IEnumerable<LocalVariable> Get()
        // {
        //     var rng = new Random();
        //     return Enumerable.Range(1, 5).Select(index => new LocalVariable
        //     {
        //         Name="2"
        //     })
        //     .ToArray();
        // }

        [HttpGet("{id}")]
        public ActionResult<LocalVariable> Get(int id)
        {
            var localVar = LocalVariableService.Get(id);
            if(localVar == null)
                return NotFound();
            return localVar;
        }

        [HttpPost]
        public IActionResult Create(LocalVariable localVar)
        {
            LocalVariable n = LocalVariableService.Add(localVar);
            if(n == null)
                return Conflict();
            return CreatedAtAction(nameof(Create), n);
        }

        [HttpPut("{id}")]
        public IActionResult Update(LocalVariable localVar)
        {
            LocalVariableService.Update(localVar);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(string id)
        {
           // var lVar = LocalVariableService.Get(id);

            // if (lVar is null)
            //     return NotFound();

           // LocalVariableService.Delete(new LocalVariable() { Id = BsonObjectId(id) });

            return NoContent();
        }
    }
}
