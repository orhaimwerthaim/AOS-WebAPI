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
    public class ManualActionController : ControllerBase
    { 

        private readonly ILogger<ManualActionController> _logger;

        public ManualActionController(ILogger<ManualActionController> logger)
        {
            _logger = logger;
        }


        [HttpGet]
        public List<ManualAction> GetAll()
        {
            return ManualActionsService .Get();
        }
      
        [HttpDelete]
        public IActionResult Delete()
        {
            var lVar = ManualActionsService.DeleteAll();

            return NoContent();
        }

        [HttpPost]
        public IActionResult Create(ManualAction item)
        {
            ManualAction n = ManualActionsService.Add(item);
            if(n == null)
                return Conflict();
            return CreatedAtAction(nameof(Create), n);
        }
    }
}
