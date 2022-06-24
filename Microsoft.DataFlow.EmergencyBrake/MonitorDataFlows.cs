using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.DataFlow.Services;
using Microsoft.DataFlow.Services.Model;
using Microsoft.Extensions.Logging;

using System.Linq;
using Microsoft.Extensions.Configuration;

namespace Microsoft.DataFlow.EmergencyBrake
{
    public class MonitorDataFlows
    {
        public MonitorDataFlows(IDataFlowService dataFlowService, IConfiguration configuration)
        {
            DataFlowServices = dataFlowService;
            _config = configuration;
        }

        private readonly IConfiguration _config;

        private readonly IDataFlowService DataFlowServices;

        [FunctionName("Orchestrator")]
        public  async Task<List<string>> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var outputs = new List<string>();

            try
            {

                outputs.Add(await context.CallActivityAsync<string>("MonitorDataFlowTransactions",""));
                
            }
            catch (Exception ex)
            {             
                ex.Message.ToString();
            }

            return outputs;
        }


        [FunctionName("MonitorDataFlowTransactions")]
        public async Task<bool> MonitorDataFlowTransactions([ActivityTrigger] ILogger log)
        {
            
            int timeout = Convert.ToInt32(_config["FailureTimeOutInMinutes"]);

            var dataFlows = await DataFlowServices.GetDataFlows();

            var transactionList = new List<DataFlowTransaction>();

            foreach (var dataFlow in dataFlows) {

                var item = (await DataFlowServices.GetDataFlowTransactions(dataFlow.objectId)).FirstOrDefault();
                if (item != null)
                {
                    item.DataFlowId = dataFlow.objectId;
                    transactionList.Add(item);
                }
            }

            var erroredList = transactionList.Where(x => x.status.ToLower() == "failed" || x.status.ToLower() == "error").ToList();
            
            //Hanging Processes
            erroredList.AddRange(transactionList.Where(x => x.status.ToLower() == "in progress" && (DateTime.Now - x.startTime).Minutes > timeout).ToList());

            if (erroredList.Any())
                transactionList.Where(x => x.status.ToLower() == "in progress").ToList()
                               .ForEach(async e => await DataFlowServices.CancelDataFlow(e.DataFlowId, e.id));

            return true;
        }

        [FunctionName("Monitor")]
        [HttpPost(nameof(Monitor))]
        public async Task<HttpResponseMessage> Monitor(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync("Orchestrator", null);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}