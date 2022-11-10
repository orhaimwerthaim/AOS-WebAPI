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
            string homePath = System.Environment.GetEnvironmentVariable("HOME");
            configuration = new Configuration() { SolverPath = homePath+"/AOS/AOS-Solver", SolverGraphPDF_DirectoryPath = homePath+"/AOS"};
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
