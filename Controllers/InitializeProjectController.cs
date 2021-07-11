using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WebApiCSharp.Models;
using WebApiCSharp.Services;
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

        private static StringBuilder buildOutput = null;
        private static StringBuilder buildErrors = null;
        private readonly ILogger<InitializeProjectController> _logger;

        public InitializeProjectController(ILogger<InitializeProjectController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public IActionResult Create(InitializeProject initProj)
        {
            // Get the path that stores favorite links.

            ProcessStartInfo sInfo = new ProcessStartInfo()
            {
                WorkingDirectory = "/home/or/Projects/AOS-Solver",
                FileName = "/opt/cmake-3.19.8-Linux-x86_64/bin/cmake",
                Arguments = "--build /home/or/Projects/AOS-Solver/build --config Debug --target all -j 14 --"
            };
            Process process = Process.Start(sInfo);//Process.Start("echo", "asdasd"); 

            // Set UseShellExecute to false for redirection.
            process.StartInfo.UseShellExecute = false;
 
            process.StartInfo.RedirectStandardOutput = true;



            InitializeProjectController.buildOutput = new StringBuilder();
            InitializeProjectController.buildErrors = new StringBuilder();

            // Set our event handler to asynchronously read the sort output.
            process.OutputDataReceived += BuildOutputHandler;
            // Redirect standard input as well.  This stream
            // is used synchronously.
            process.StartInfo.RedirectStandardInput = true;

            // Start the process.
            process.Start();
 
            process.BeginOutputReadLine(); 
            process.WaitForExit();

            Console.WriteLine("Build output:");
            Console.WriteLine(buildOutput);
            Console.WriteLine("Build Errors:");
            Console.WriteLine(buildErrors);

            process.Close();
            return CreatedAtAction(nameof(Create), initProj);
        }

        private static void BuildOutputHandler(object sendingProcess,
            DataReceivedEventArgs outLine)
        {
            // Collect the sort command output.
            if (!String.IsNullOrEmpty(outLine.Data))
            {
                // Add the text to the collected output.
                buildOutput.Append(Environment.NewLine + outLine.Data);
                if(outLine.Data.ToLower().Contains("error"))
                    buildErrors.Append(Environment.NewLine + outLine.Data);
            }
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
