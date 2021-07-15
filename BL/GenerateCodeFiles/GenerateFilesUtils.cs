
using MongoDB.Bson;
using System;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using WebApiCSharp.Services;
using WebApiCSharp.Models;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace WebApiCSharp.GenerateCodeFiles
{
    public class GenerateFilesUtils
    {
        public static string ToUpperFirstLetter(string str)
        {
            return char.ToUpper(str[0]) + str.Substring(1);
        }

        public static string GetIndentationStr(int numOfIndentations, int indentSize = 4, string str="", bool withNewLine = true)
        {
            return (new string(' ', numOfIndentations * indentSize))+str + Environment.NewLine;
        }
    }
}