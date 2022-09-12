using WebApiCSharp.Models;
using System.Collections.Generic;
using System.Linq;

namespace WebApiCSharp.Services
{
    public static class ConfigurationService
    {
        private static Configuration configuration;
        static ConfigurationService()
        {
            configuration = new Configuration() { SolverPath = "~/AOS/AOS-Solver", SolverGraphPDF_DirectoryPath = "~/AOS", SolverGraphPDF_Depth = 1 };
        }


        public static Configuration Get()
        {
            return configuration;
        }

        public static void Update(Configuration _configuration)
        {
            configuration = _configuration;
        }
    }
}
