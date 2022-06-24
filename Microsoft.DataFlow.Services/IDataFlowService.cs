using Microsoft.DataFlow.Services.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.DataFlow.Services
{
    public interface IDataFlowService
    {
        Task<List<DataflowInfo>> GetDataFlows();

        Task<List<DataFlowTransaction>> GetDataFlowTransactions(string dataFlowid);

        Task<bool> CancelDataFlow(string dataflowId, string dataflowTransactionId);

    }
}
