namespace Aurea.Maintenance.Debugger.Texpo
{
    using Aurea.Maintenance.Debugger.Common;
    using Aurea.Maintenance.Debugger.Common.Models;

    public class Program
    {

        public static void Main(string[] args)
        {
            var config = ClientConfiguration.GetClientConfiguration(Clients.Texpo, Stages.UserAcceptance);
            ClientConfiguration.SetConfigurationContext(config);
        }
    }
}
