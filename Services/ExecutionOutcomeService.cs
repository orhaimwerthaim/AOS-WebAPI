using System; 
using MongoDB.Driver;
using MongoDB.Bson;  
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using WebApiCSharp.Models;
using System.Collections.Generic;
using System.Linq;

namespace WebApiCSharp.Services
{
    public class ExecutionOutcomeService : ServiceBase
    {
        public static string Get(int belief_size, string initilaBeliefJson)
        {
            try
            {
                string startRes = "{\"ExecutionOutcome\":[" + initilaBeliefJson + ",";
                string result = startRes;
                List<SolverAction> solverActions = SolverActionsService.Get();
                List<BsonDocument> actions = ActionsForExecutionService.Get();
                List<BsonDocument> belief = BeliefStateService.Get(0, belief_size);
                List<ModuleResponse> responses = ModuleResponseService.Get();
                int actionSequnceId = 1;
                while(true)
                {
                    string actionRes = GetJsonForActionSequnceId(actionSequnceId, actions, responses, belief, solverActions);
                    if (string.IsNullOrEmpty(actionRes))
                    { 
                        break;
                    }
                    else
                    {
                        result += result.Length == startRes.Length ? "" : ",";
                        result += actionRes;
                    }

                    actionSequnceId++;
                }
                bool removeDelimiter = result.EndsWith(",");
                return  (removeDelimiter ? result.Substring(0, result.Length-1) : result) + "]}";
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error:" + ex.Message);
                return null;
            }
        }

        private static int GetClosingJsonIndex(string json, char openChar)
        {
            int count = 1;
            char closeChar = openChar == '[' ? ']' : openChar == '{' ? '}' : '\n';
            if (json[0] != openChar) return 0;
            for (int i = 1; i < json.Length; i++)
            {
                count += json[i].Equals(openChar) ? 1 : json[i].Equals(closeChar) ? -1 : 0;
                if (count == 0) return i;
            }
            return -1;
        }
        private static string GetJsonFirstFieldValue(string fieldName, string json)
        {
            json = json.Replace(" ", "");
            int fieldIndex = json.IndexOf(fieldName);
            if (fieldIndex < 0) return null;

            json = json.Substring(fieldIndex + fieldName.Length + 2);
            if (json[0].Equals('{') || json[0].Equals('['))
            {
                int endInd = GetClosingJsonIndex(json, json[0]);
                return json.Substring(0, endInd + 1);
            }
            else
            {
                int endIndex = json.IndexOf(",") > -1 ? json.IndexOf(",") : json.IndexOf("}");
                if (endIndex < 0) return null;
                return json.Substring(0, endIndex);
            }
        }

        private static string GetJsonForActionSequnceId(int actionSequenceId, List<BsonDocument> actions, 
            List<ModuleResponse> responses, List<BsonDocument> belief, List<SolverAction> solverActions)
        { 
            if(actions.Count < actionSequenceId)return null;
            if(actions.Where<BsonDocument>(x=> x["ActionSequenceId"] == actionSequenceId).LastOrDefault()==null)return null;
            //string actionJson = actions[actionSequenceId - 1].ToJson();
            string actionJson = actions.Where<BsonDocument>(x=> x["ActionSequenceId"] == actionSequenceId).LastOrDefault().ToJson();
            string beliefJson = belief.Where<BsonDocument>(x=> x["ActionSequnceId"] == actionSequenceId).LastOrDefault().ToJson();

            //string beliefJson = (belief.Count < actionSequenceId) ? "{\"BeliefeState\":[]}" : belief[actionSequenceId - 1].ToJson();
            List<ModuleResponse> actionResponses = responses.Where(x => x.ActionSequenceId.Equals(actionSequenceId)).ToList();
            ModuleResponse actionResponse = actionResponses.FirstOrDefault();
            bool middlewareRecievedAction = actionResponse != null;
            int actionId = int.Parse(GetJsonFirstFieldValue("ActionID", actionJson));
            string jsonRes = "{\"ActionSequenceId\" : " + actionSequenceId;

            SolverAction solverAction = solverActions.Where(x => x.ActionID.Equals(actionId)).FirstOrDefault();

            jsonRes += ", \"ActionDetails\":" + solverAction.ToJson();

             
            jsonRes += ", \"SolverSentActionTime\" : \""+ DateTimeToString(ISO_ToDateTime(GetJsonFirstFieldValue("RequestCreateTime",actionJson)).Value)+"\"";
 
            jsonRes += ", \"ModuleExecutionStartTime\" : \""+
                (middlewareRecievedAction ? DateTimeToString(actionResponse.StartTime.Value) : "null")+"\"";

            jsonRes += ", \"ModuleExecutionEndTime\" : \""+
                (middlewareRecievedAction ? DateTimeToString(actionResponse.EndTime.Value) : "null")+"\"";

            jsonRes += ", \"ModuleResponseText\" : \""+
                (middlewareRecievedAction ? 
                ((actionResponses.Count > 1) ? "Fatal Error: more than one response received from module!!!" : actionResponse.ModuleResponseText) : "null")+"\"";


            int ind = beliefJson.IndexOf("\"BeliefeState") + ("\"BeliefeState\":").Length;
            beliefJson = beliefJson.Substring(ind, beliefJson.Length - ind - 1);
            string deslimiter = beliefJson.Replace(" ", "").StartsWith(":") ? "" : ":";
            jsonRes += ", \"BeliefStatesAfterExecution\" " + deslimiter + beliefJson;

            jsonRes += "}";


            return jsonRes;
        }
    }
}
