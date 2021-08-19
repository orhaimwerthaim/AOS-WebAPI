using System;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using WebApiCSharp.Models;
using System.Collections.Generic;

namespace WebApiCSharp.Services
{
    public class SolverActionsService : ServiceBase
    {
        public static IMongoCollection<BsonDocument> ActionsCollection = dbAOS.GetCollection<BsonDocument>(Globals.ACTIONS_COLLECTION_NAME);
        public static List<SolverAction> Get()
        {
            try
            {
                List<SolverAction> olResult = new List<SolverAction>();
                List<BsonDocument> results = ActionsCollection.Find<BsonDocument>(c => true).ToList();

                foreach (var doc in results)
                {
                    SolverAction item = new SolverAction();

                    item.ActionName = doc["ActionName"].ToString();
                    item.ActionDescription = doc["ActionDecription"].ToString();
                    item.ActionID = doc["ActionID"].AsInt32;
                    item.ActionConstantParameters = new List<ActionConstantParameter>();
                    foreach (var arrItem in doc["ActionConstantParameters"].AsBsonArray)
                    {
                        BsonDocument bParameterConst = arrItem.AsBsonDocument;


                        string[] sItems = bParameterConst.ToString().Replace(" ", "").Replace("{", "").Replace("}", "").Replace("\"", "").Split(",");
                        foreach (string sI in sItems)
                        {
                            ActionConstantParameter par = new ActionConstantParameter();
                            par.ParameterName = sI.Split(":")[0];
                            par.Value = sI.Split(":")[1];
                            item.ActionConstantParameters.Add(par);
                        }


                    }
                    olResult.Add(item);
                }
                return olResult;

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error:" + ex.Message);
                return null;
            }
        }
    }
}