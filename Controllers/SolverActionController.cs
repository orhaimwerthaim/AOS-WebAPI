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
    public class SolverActionController : ControllerBase
    {
         
        private readonly ILogger<SolverActionController> _logger;

        public SolverActionController(ILogger<SolverActionController> logger)
        {
            _logger = logger;
        }


        [HttpGet]
        public List<SolverAction> GetAll()
        {
            return SolverActionsService.Get();
        }
        

        
    }
}
