namespace Aurea.Maintenance.Debugger.Common.Extensions
{
    using Aurea.Maintenance.Debugger.Common.Models;

    public static class ClientExtensions
    {
        public static int Id(this Clients client)
        {
            switch(client)
            {
                case Clients.Accent:
                    return 26;
                case Clients.AEP:
                    return 52;
                case Clients.GreenMountain:
                    return 15;
                case Clients.Hudson:
                    return 36;
                case Clients.Spark:
                    return 48;
                case Clients.StarTex:
                    return 16;
                case Clients.Stream:
                    return 45;
                case Clients.Texpo:
                    return 22;
                case Clients.TRE:
                    return 42;
                default:
                    return 0;
            }
        }

        public static string Abbreviation(this Clients client)
        {
            switch (client)
            {
                case Clients.Accent:
                    return "ACC";
                case Clients.AEP:
                    return "AEP";
                case Clients.GreenMountain:
                    return "GMC";
                case Clients.Hudson:
                    return "HES";
                case Clients.Spark:
                    return "SPK";
                case Clients.StarTex:
                    return "ST";
                case Clients.Stream:
                    return "SGE";
                case Clients.Texpo:
                    return "TXP";
                case Clients.TRE:
                    return "TRE";
                default:
                    return string.Empty;
            }
        }

        public static System.Guid GetServiceGuid(this Clients client)
        {
            switch (client)
            {
                case Clients.AEP:
                    return new System.Guid("15E69993-11A4-4507-9E2A-8DD9DE7A0FA3");
                default:
                    throw new System.NotImplementedException($"GetServiceGuid for client {client} not implemented");
            }
        } 
    }
}
