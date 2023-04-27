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
    public class SimulatedStateController : ControllerBase
    { 

        private readonly ILogger<SimulatedStateController> _logger;

        public SimulatedStateController(ILogger<SimulatedStateController> logger)
        {
            _logger = logger;
        }


        [HttpGet] 
        public dynamic Get()
        {
            this.Response.ContentType = "application/json";
            
            string jsonString = SimulatedStateService.Get().ToJson();
            return Content(jsonString);
        }
    }
}
