namespace Aurea.Maintenance.Debugger.Common.Models
{
    using Aurea.Maintenance.Debugger.Common.Extensions;

    public class ClientEnvironmentConfiguration
    {
        private const string _billingAdminConnectionString = "Server={0}.{1},24955;Initial Catalog={2}_BillingAdmin;Trusted_Connection=Yes{3}";

        public ClientEnvironmentConfiguration(Clients client, Stages stage, TransactionMode transactionMode)
        {
            Client = client.Abbreviation();
            ClientId = client.Id();
            Stage = stage.Name();
            Server = stage.Server();
            TransactionMode = transactionMode;
            ConnectionBillingAdmin = string.Format(_billingAdminConnectionString, stage.Server(), stage.Domain(), stage.Prefix(),
                transactionMode == TransactionMode.Enlist ? ";Enlist=false" : "");
        }

        public string Client { get; set; }

        public int ClientId { get; set; }

        public string Stage { get; set; }    
        
        public string Server { get; set; }

        public TransactionMode TransactionMode { get; set; }

        public string ConnectionBillingAdmin { get; set; }
    }
}
