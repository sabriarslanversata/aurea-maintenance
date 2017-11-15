namespace Aurea.Maintenance.Debugger.Common.Models
{
    using Aurea.Maintenance.Debugger.Common.Extensions;

    public class ClientEnvironmentConfiguration
    {
        private const string _billingAdminConnectionString = "Server={0}.{1},24955;Initial Catalog={2}_BillingAdmin;Trusted_Connection=Yes";

        public ClientEnvironmentConfiguration(Clients client, Stages stage)
        {
            Client = client.Abbreviation();
            ClientId = client.Id();
            Stage = stage.Name();
            Server = stage.Server();
            ConnectionBillingAdmin = string.Format(_billingAdminConnectionString, stage.Server(), stage.Domain(), stage.Prefix());
        }

        public string Client { get; set; }
        public int ClientId { get; set; }
        public string Stage { get; set; }        
        public string Server { get; set; }
        public string ConnectionBillingAdmin { get; set; }
    }
}
