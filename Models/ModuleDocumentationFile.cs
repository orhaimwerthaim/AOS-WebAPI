using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using System.Collections.Generic;

namespace WebApiCSharp.Models
{
    public class ModuleDocumentationFile
    {
        public string Name { get; set; }

        public string Type { get; set; }

        public string Project { get; set; }
    }
}