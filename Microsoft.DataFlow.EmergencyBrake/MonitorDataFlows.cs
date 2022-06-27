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
using System.Threading;

namespace Microsoft.DataFlow.EmergencyBrake
{
    public class MonitorDataFlows
    {
        public MonitorDataFlows(IDataFlowService dataFlowService, IConfiguration configuration)
        {
            _dataFlowServices = dataFlowService;
            _config = configuration;
        }

        private readonly IConfiguration _config;

        private readonly IDataFlowService _dataFlowServices;

        private int _pollingInterval => Convert.ToInt32(_config["PollingIntervalInMinutes"]);

        private int _monitorDuration => Convert.ToInt32(_config["MonitorTImeInMinutes"]);

        private int _timeout => Convert.ToInt32(_config["FailureTimeOutInMinutes"]);
        
        [FunctionName("Orchestrator")]
        public async Task<List<string>> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context, ILogger log)
        {
            log.LogInformation($"Entered Orchestrator {context.CurrentUtcDateTime}");

            var outputs = new List<string>();

            DateTime expiryTime = context.CurrentUtcDateTime.AddMinutes(_monitorDuration);

            log.LogInformation($"Expiration Time = {expiryTime}");

            try
            {

                while (context.CurrentUtcDateTime < expiryTime)
                {

                    log.LogInformation($"Current Time = {context.CurrentUtcDateTime}; Expiry Time = {expiryTime}");

                    outputs.Add(await context.CallActivityAsync<string>("MonitorDataFlowTransactions", ""));

                    var nextCheck = context.CurrentUtcDateTime.AddMinutes(_pollingInterval);
                    await context.CreateTimer(nextCheck, CancellationToken.None);
                }

            }
            catch (Exception ex)
            {
                log.LogError(ex, ex.Message);
            }

            return outputs;
        }


        [FunctionName("MonitorDataFlowTransactions")]
        public async Task<bool> MonitorDataFlowTransactions([ActivityTrigger] string dataVal, ILogger log)
        {

            log.LogInformation($"Entered MonitorDataFlowTransactions = {DateTime.Now}");

            try
            {

                var dataFlows = await _dataFlowServices.GetDataFlows();

                var transactionList = new List<DataFlowTransaction>();

                foreach (var dataFlow in dataFlows)
                {

                    var item = (await _dataFlowServices.GetDataFlowTransactions(dataFlow.objectId)).FirstOrDefault();
                    if (item != null)
                    {
                        item.DataFlowId = dataFlow.objectId;
                        transactionList.Add(item);
                    }
                }

                log.LogInformation($"Transactions returned = {transactionList.Count}");

                var erroredList = transactionList.Where(x => x.status.ToLower() == "failed" || x.status.ToLower() == "error").ToList();

                //Hanging Processes
                erroredList.AddRange(transactionList.Where(x => x.status.ToLower() == "in progress" && (DateTime.Now - x.startTime).Minutes > _timeout).ToList());

                if (erroredList.Any())
                    transactionList.Where(x => x.status.ToLower() != "cancelled" || x.status.ToLower() != "success").ToList()
                                   .ForEach(async e => await _dataFlowServices.CancelDataFlow(e.DataFlowId, e.id));
            }
            catch (Exception ex)
            {
                log.LogError(ex, ex.Message);
            }

            return true;
        }

        [FunctionName("Monitor")]
        [HttpPost(nameof(Monitor))]
        public async Task<HttpResponseMessage> Monitor(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            
            string instanceId = await starter.StartNewAsync("Orchestrator", null);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}