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
    public class LogMessageController : ControllerBase
    { 

        private readonly ILogger<LogMessageController> _logger;

        public LogMessageController(ILogger<LogMessageController> logger)
        {
            _logger = logger;
        }


        [HttpGet]
        public List<LogMessage> GetAll()
        {
            return LogMessageService .Get();
        }
      
        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var lVar = LogMessageService.DeleteAll();

            return NoContent();
        }
    }
}
