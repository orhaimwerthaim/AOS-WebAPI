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
            configuration = new Configuration() { SolverPath = "/home/or/Projects/AOS-Solver", SolverGraphPDF_DirectoryPath = "/home/or/Projects", SolverGraphPDF_Depth = 0 };
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