using Microsoft.DataFlow.Services.Common;
using Microsoft.DataFlow.Services.Model;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

        public async Task<bool> CancelDataFlow(string dataflowId, string dataflowTransactionId)
        {
            bool res = false;

            using var reqMessage = Util.GetRequestMessage(HttpMethod.Get, $"https://api.powerbi.com/v1.0/myorg/groups/{groupId}/dataflows/{dataflowId}/transactions/{dataflowTransactionId}/cancel");
            {

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await _authService.GetBearerToken());

                var resp = await client.SendAsync(reqMessage);

                res = resp.StatusCode == HttpStatusCode.OK;
            }

            return res;
        }

        public async Task<List<DataflowInfo>> GetDataFlows()
        {
            
            var res = new List<DataflowInfo>();

            using var reqMessage = Util.GetRequestMessage(HttpMethod.Get, $"https://api.powerbi.com/v1.0/myorg/groups/{groupId}/dataflows");
            {

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await _authService.GetBearerToken());

                var resp = await client.SendAsync(reqMessage);

                var body = await resp.Content.ReadAsStringAsync();

                res = JsonConvert.DeserializeObject<List<DataflowInfo>>(JObject.Parse(body)["value"].ToString());
            }

            return res;
        }

        public async Task<List<DataFlowTransaction>> GetDataFlowTransactions(string dataFlowid)
        {

            var dftList = new List<DataFlowTransaction>();

            using var reqMessage = Util.GetRequestMessage(HttpMethod.Get, $"https://api.powerbi.com/v1.0/myorg/groups/{groupId}/dataflows/{dataFlowid}/transactions"); 
            {

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await _authService.GetBearerToken());

                var resp = await client.SendAsync(reqMessage);

                var body = await resp.Content.ReadAsStringAsync();

                dftList = JsonConvert.DeserializeObject<List<DataFlowTransaction>>(JObject.Parse(body)["value"].ToString());

            }

            return dftList;
        }
    }
}
