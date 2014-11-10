using System.ComponentModel.Composition;

namespace Its.Configuration.Tests
{
    [Export]
    public class OrderRepository
    {
        [Import(contractName:"orders-db-connection-string", AllowDefault = true)]
        public string ConnectionString { get; set; }

        [Import("db-retry-count", AllowDefault = true)]
        private int? retryCount;
        
        public int RetryCount
        {
            get
            {
                return retryCount ?? 123;
            }
            set
            {
                retryCount = value;
            }
        }
    }
}