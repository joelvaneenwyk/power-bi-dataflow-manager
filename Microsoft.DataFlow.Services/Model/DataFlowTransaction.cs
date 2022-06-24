using System;

namespace Microsoft.DataFlow.Services.Model
{
    public class DataFlowTransaction
    {
        public string id { get; set; }
        public string refreshType { get; set; }
        public DateTime startTime { get; set; }
        public DateTime endTime { get; set; }
        public string status { get; set; }
        public string errorInfo { get; set; }

        public string DataFlowId { get; set; }
    }
}
