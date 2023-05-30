using MongoDB.Bson;
using System;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using WebApiCSharp.Services;
using WebApiCSharp.Models;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;

namespace WebApiCSharp.JsonTextModel
{
    public class SDL
    {
        public static LanguageStructure AmStructure{get;}
        static SDL()
        {
            AmStructure = new LanguageStructure();
            LanguageElement project = new LanguageElement("project");
            AmStructure.AddFirstLevelElement(project);

            LanguageElement response = new LanguageElement("response");
            LanguageElement responseRule = new LanguageElement("response_rule", response);
            response.Children.Add(responseRule);
            AmStructure.AddFirstLevelElement(response);
        }

    }

    public class LanguageStructure
    { 
        private Dictionary<string, LanguageElement> AllElements{get;}
    
        public LanguageElement Current{get; private set;}
        private Dictionary<string, LanguageElement> FirstLevelElements{get;}
        public LanguageStructure()
        {
            FirstLevelElements = new Dictionary<string, LanguageElement>();
            Current = null;
            AllElements=new Dictionary<string, LanguageElement>();
        }

        public void LoadLine(string line)
        {
            List<string> contextWords = GetSavedWordsInContext();
            string firstSaved = line.IndexOf(": ") > -1 ? line.Substring(0,line.IndexOf(": ") -1):null;
            // if(firstSaved)
            // if(line.IndexOf(' '))
        }

        public void ChangeContext(string savedWord)
        {
            if(FirstLevelElements.ContainsKey(savedWord))
            {
                Current = FirstLevelElements[savedWord];
                return;
            }
            else if(Current != null)
            {
                foreach(LanguageElement child in Current.Children)
                {
                    if(savedWord == child.Name)
                    {
                        Current = child;
                        return;
                    }
                }
            }
            string possibleWords = String.Join(',', GetSavedWordsInContext());
            throw new Exception("Saved word '"+savedWord+"' is not in context! The context is:"+
                Current == null ? "top level" : Current.Name+", possible words are:'"+possibleWords+"'");
        }
        public void AddFirstLevelElement(LanguageElement elem)
        {
            if(AllElements.ContainsKey(elem.Name)) throw new Exception("First Level element with name: '"+elem.Name+"' already exists!");
            AllElements.Add(elem.Name, elem);
        }

        public List<string> GetSavedWordsInContext()
        {
            List<string> res = new List<string>();
            res.AddRange(FirstLevelElements.Keys);
            if (Current != null)
            {
                res.AddRange(Current.Children.Select(x=> x.Name));
            }
            return res;
        }
    }

    public class LanguageElement
    {
        public string Name{get;}
        public LanguageElement Parent{get;}
        
        public List<LanguageElement> Children{get;}

        public LanguageElement(string name, LanguageElement parent = null)
        {
            Name = name;
            Parent = parent;
            Children = new List<LanguageElement>();
        }
    }

}