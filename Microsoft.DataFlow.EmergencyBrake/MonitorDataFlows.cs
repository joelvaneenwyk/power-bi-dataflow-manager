using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.DataFlow.Services;
using Microsoft.DataFlow.Services.Model;
using Microsoft.Extensions.Logging;

using System.Linq;
using Microsoft.Extensions.Configuration;
using System.Threading;
using Microsoft.Azure.Functions.Worker;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using OrchestrationTriggerAttribute = Microsoft.Azure.Functions.Worker.OrchestrationTriggerAttribute;
using ActivityTriggerAttribute = Microsoft.Azure.Functions.Worker.ActivityTriggerAttribute;

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

        private bool _Retry => Convert.ToBoolean(_config["RestartCancelledDataFlow"]);

        [Function("Orchestrator")]
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


        [Function("MonitorDataFlowTransactions")]
        public async Task<bool> MonitorDataFlowTransactions([ActivityTrigger] string dataVal, ILogger log)
        {

            log.LogInformation($"Entered MonitorDataFlowTransactions = {DateTime.Now}");

            try
            {

                var transactionList = new List<DataFlowTransaction>();

                foreach (var dataFlow in await _dataFlowServices.GetDataFlows())
                {

                    var item = (await _dataFlowServices.GetDataFlowTransactions(dataFlow.objectId)).FirstOrDefault();
                    if (item != null)
                    {
                        item.DataFlowId = dataFlow.objectId;
                        transactionList.Add(item);
                    }
                }

                log.LogInformation($"Transactions returned = {transactionList.Count}");

                static bool stopped(DataFlowTransaction x) => x.status.ToLower() == "failed" || x.status.ToLower() == "error";

                var erroredList = transactionList.Where(stopped).ToList();

                //Hanging Processes
                erroredList.AddRange(transactionList.Where(x => x.status.ToLower() == "in progress" && (DateTime.Now - x.startTime).Minutes > _timeout).ToList());

                if (erroredList.Count != 0)
                    if (_Retry)
                        transactionList.Where(s => stopped(s)).ToList()
                                       .ForEach(async e =>
                                                await _dataFlowServices.CancelDataFlow(e.DataFlowId, e.id)
                                                .ContinueWith(cw => _dataFlowServices.RefreshDataFlow(e.DataFlowId)));
                    else
                        transactionList.Where(s => !stopped(s))
                                       .Where(x => 
                                            !x.status.Equals("cancelled", StringComparison.CurrentCultureIgnoreCase) 
                                            || !x.status.Equals("success", StringComparison.CurrentCultureIgnoreCase))
                                       .ToList()
                                       .ForEach(async e =>  await _dataFlowServices.CancelDataFlow(e.DataFlowId, e.id));
            }
            catch (Exception ex)
            {
                log.LogError(ex, ex.Message);
            }

            return true;
        }

        [Function("Monitor")]
        [HttpPost(nameof(Monitor))]
        public async Task<IActionResult> Monitor(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req,
            [Azure.Functions.Worker.DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            
            string instanceId = await starter.StartNewAsync("Orchestrator", null);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}