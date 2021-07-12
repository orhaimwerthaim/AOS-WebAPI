using MongoDB.Bson;  
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using WebApiCSharp.Services;
using WebApiCSharp.Models;
using System.IO;
using System.Collections.Generic;
namespace WebApiCSharp.GenerateCodeFiles
{
    public class GenerateCodeFilesUtils
    {
        public static void WriteTextFile(string path, string content)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            // Create a file to write to.
            using (StreamWriter sw = File.CreateText(path))
            {
                sw.Write(content);
            }
        }
    }
}