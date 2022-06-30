using Microsoft.DataFlow.Services.Common;
using Microsoft.DataFlow.Services.Model;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Microsoft.DataFlow.Services
{
    public class DataFlowService : IDataFlowService
    {

        private string groupId { get; set; }

        public DataFlowService(IConfiguration configuration, IAuthService authService)
        {
            _config = configuration;
            groupId = _config["GroupId"];

            _authService = authService;
        }

        private readonly IAuthService _authService;

        private readonly IConfiguration _config;

        public async Task<List<DataflowInfo>> GetDataFlows()
        {
            
            var resp = await CallAPI(() => Util.GetRequestMessage(HttpMethod.Get, $"https://api.powerbi.com/v1.0/myorg/groups/{groupId}/dataflows"));
            
            var body = await resp.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<List<DataflowInfo>>(JObject.Parse(body)["value"].ToString());
            
        }

        public async Task<List<DataFlowTransaction>> GetDataFlowTransactions(string dataFlowid)
        {
            
            var resp = await CallAPI(() => Util.GetRequestMessage(HttpMethod.Get, $"https://api.powerbi.com/v1.0/myorg/groups/{groupId}/dataflows/{dataFlowid}/transactions"));

            var body = await resp.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<List<DataFlowTransaction>>(JObject.Parse(body)["value"].ToString());

        }

        public async Task<bool> CancelDataFlow(string dataflowId, string dataflowTransactionId)
        {
            
            var resp = await CallAPI(() => Util.GetRequestMessage(HttpMethod.Post, $"https://api.powerbi.com/v1.0/myorg/groups/{groupId}/dataflows/{dataflowId}/transactions/{dataflowTransactionId}/cancel"));

            return resp.StatusCode == HttpStatusCode.OK;

        }

        public async Task<bool> RefreshDataFlow(string dataflowId)
        {

            var reqBody =  new Dictionary<string, string>() { { "notifyOption", "NoNotification" } };

            var resp = await CallAPI(() => Util.GetRequestMessage(HttpMethod.Post, $"https://api.powerbi.com/v1.0/myorg/groups/{groupId}/dataflows/{dataflowId}/refreshes", reqBody));

            return resp.StatusCode == HttpStatusCode.OK;

        }

        private async Task<HttpResponseMessage> CallAPI(Func<HttpRequestMessage> RequestMessage) 
        {

            using var reqMessage = RequestMessage();
            {

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await _authService.GetBearerToken());

                return await client.SendAsync(reqMessage);

            }                   
        }

    }
}
