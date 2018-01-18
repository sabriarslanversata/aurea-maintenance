namespace Aurea.Maintenance.Debugger.Common.Extensions
{
    using Aurea.Maintenance.Debugger.Common.Models;

    public static class StageExtensions
    {
        public static string Name(this Stages stage)
        {
            switch (stage)
            {
                case Stages.Development:
                    return "DEV";
                case Stages.UserAcceptance:
                    return "UA";
                case Stages.Production:
                    return "PROD";
                default:
                    return string.Empty;
            }
        }

        public static string Prefix(this Stages stage)
        {
            switch (stage)
            {
                case Stages.Development:
                    return "daes";
                case Stages.UserAcceptance:
                    return "saes";
                case Stages.Production:
                    return "paes";
                default:
                    return string.Empty;
            }
        }

        public static string Domain(this Stages stage)
        {
            switch (stage)
            {
                case Stages.Development:
                case Stages.UserAcceptance:
                    return "aesua.local";
                case Stages.Production:
                    return "aesprod.local";
                default:
                    return string.Empty;
            }
        }

        public static string Server(this Stages stage)
        {
            switch (stage)
            {
                case Stages.Development:
                case Stages.UserAcceptance:
                    return "SGISUSEUAV01";
                case Stages.Production:
                    return "SGISUSEPRV01";
                default:
                    return string.Empty;
            }
        }
    }
}
