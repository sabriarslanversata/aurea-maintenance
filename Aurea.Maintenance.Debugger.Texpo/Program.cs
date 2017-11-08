using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading;
using Aurea.Maintenance.Debugger.Common;


namespace Aurea.Maintenance.Debugger.Texpo
{

    public class Program
    {
        private static CIS.BusinessEntity.GlobalApplicationConfigurationDS.GlobalApplicationConfiguration _clientConfiguration;

        public class MyExport : CIS.Clients.Texpo.Export.MainProcess//CIS.Export.BaseExport
        {
            private static readonly string _uaaDir = Assembly.GetExecutingAssembly().Location + "\\uua\\";
            private static readonly string _uqcDir = Assembly.GetExecutingAssembly().Location + "\\uqc\\";

            public MyExport(string connectionMarket, string connectionCsr, string connectionAdmin)
            {
                _connectionMarket = connectionMarket;
                _connectionCsr = connectionCsr;
                _connectionAdmin = connectionAdmin;
                _serviceExecuteDate = DateTime.Now;
                LoadConfiguration(string.Empty);
            }

            public void MyExportTransactions()
            {
                ExportTransactions();
            }

            public void MyTransmitFiles()
            {
                TransmitFiles();
            }

            public void MyExportLoadForecasting()
            {
                //ExportLoadForecasting();//private and exists on Texpo only, so if we need to debug this we need to implement logic here again
            }

            public void MyProcessEFTPayments()
            {
                ProcessEFTPayments();//protected virtual
            }

            public void MyRunAutoDNPProcess()
            {
                //RunAutoDNPProcess();//private and exists on Texpo only, so if we need to debug this we need to implement logic here again
            }

            public void MyLogService()
            {
                LogService();//protected derived from BaseService
            }

            public override void InitializeVariables(string exportFunction)
            {
                _directoryFtpOut = Path.Combine(_uaaDir, @"Data\ClientData\TXP\Services\Transport\Ftp\Out\");
                _directoryEncrypted = Path.Combine(_uaaDir, @"Data\Clientdata\TXP\Services\Transport\Ftp\Out\Market\Encrypted\");
                _directoryDecrypted = Path.Combine(_uaaDir, @"Data\Clientdata\TXP\Services\Transport\Ftp\Out\Market\Decrypted\");
                _directoryArchive = Path.Combine(_uaaDir, @"Data\Clientdata\TXP\Services\TransportArchive\Ftp\Out\");
                _directoryException = Path.Combine(_uaaDir, @"Data\Clientdata\TXP\Services\Transport\Ftp\Out\Market\Exception\");
                _pgpPassPhrase = "hokonxoyg";
                _pgpEncryptionKey = Path.Combine(_uqcDir, @"Data\PGPKeys\ista-na.asc");
                _pgpSignatureKey = Path.Combine(_uaaDir, @"Data\PGPKeys\ista-na.asc");
                _ftpRemoteServer = "localhost";
                _ftpRemoteDirectory = string.Empty;
                _ftpUserName = string.Empty;
                _ftpUserPassword = string.Empty;
                _runHour = "6";
                _runDay = "*";
                _runDayOfWeek = "*";
                _marketFileVersion = "3.0";
                _serviceInterval = 5;
                _historicalUsageRequestType = "HU";
                _clientID = Utility.Clients["TXP"];
                if(!Directory.Exists(_directoryDecrypted))
                    Directory.CreateDirectory(_directoryDecrypted);

                if (!Directory.Exists(_directoryEncrypted))
                    Directory.CreateDirectory(_directoryEncrypted);

                if (!Directory.Exists(_directoryException))
                    Directory.CreateDirectory(_directoryException);


                if (!File.Exists(_pgpEncryptionKey))
                {
                    Directory.CreateDirectory(_pgpEncryptionKey.Replace(Path.GetFileName(_pgpEncryptionKey),""));
                    File.WriteAllText(_pgpEncryptionKey, @"-----BEGIN PGP PRIVATE KEY BLOCK-----
Version: BCPG C# v1.6.1.0

lQOsBFn53zQBCADMlxdkpJRay9J1lA2Xc6fsyM0A7CRzcY5TCuU31ZROV0s8Z+97
hpwAdRzRXPfwh7465505tW2q3hqpOOyMmXXAbua8fVn/hklXxVnCB0WQFLHIL/Dm
UyJnRy9vyKkt5QGzAhLMDWtRn/bejIJ8jzB8WFjf+luRpR/GdvPjy8Q7/0tY6By4
Ua67nlHTFkHJ7iytE1ShbNHoe1BtsuEdwu5BmNHDkixpUAFvm5MoC0Xvib4yvAVj
1+r/U9w5AFGMoNZrYO4Ckg3ZKCHwNMvI0sD0zHnW36jGV4yoNBQ+Uc3bBaxZKoy9
DxLIWT+066UqD2V3pIcObQT8xVQybxIHzoxFABEBAAH/AwMCB5I8JHaajnJgZ10f
dxEBzvlTmxBq2x4WGVvfhV6vks1CAu+KO6pGxaskJG/rNIXBrDv5g4dO3hIjaDQX
tOuyAp3WB+b06EnCRuXsUBQcDPoRZ8SfNlUkyNCEh9M3sDHng9nfVbGd5jjpFHiU
1Uw1OOGNd6TZLrmTzwqTpMoKs/cGw5GnT1LOsAy57oG0UuF9KVeknkOApN6K3Mez
VX6LiheaS8izV4Vfgr4pZn+GrL97iV41CITxPDOqL0AJvXmzAzT95324Bjo6+WAW
q3geDzMgd0udO4n/DQ1NSpBz1Wjvd9f60RDxfavpYKntEyy074ZbUAEGFE5CQhOy
r4JQQcXGzkkRK65/lkVbDsUAe2ICH2WQ5KEHe1gNEJyBQZr/s1XK0xO7B3EwPzDT
qdgN47H0DA7pXAbb3QxVxNTVx8HZkzxySJjmjrNvOC3VqKtDkQwY4dIDQ+R2ArrQ
7h9F/XdUAAFcw8XdtISl5XHWH13l3ZQb5DxpMOtjazLnFu5P00Drf9NyftO6eAY8
Xph+T1Oq24HXIO91zKsjpaV4Ru3U/5cxP8jp3rBLHkhWFnQrV6mVNuXeXPHYzMhj
LVxeTIoY/ELuM17fouyOEMhNLtDX3JZ5AXLctP9BEr5mwnjIJMmmB2yOOXMKZYeL
UO0pTBzgTI4BzlEpCcUQ2G8GAbpSN2f6R9trVgWNaX+hnC5GBv2yMIFfeKMIasS1
soGP5C6NPCBJDfd5iztfz8YLYwoaIb9d6HsQ06Xeylb/5cwpjehC4iL9OFI4emaJ
Ghrf/MeFfQVDdhM2MULaiJGMrWCvJNy+VMQfiX11/5iFtWgM6FK9D8yEdj2L7PEH
t/WzujPxWTS/AtnI7xnNhc7sb82LRsV7R2US7XTLDbQYc2FicmkuYXJzbGFuQHZl
cnNhdGEuY29tiQEcBBABAgAGBQJZ+d80AAoJEGHYYRGYWV281UkH/2wW6vqlqRzT
G2WFGAJEf3SoqYiEb5rJvA0aI10izRla2hzWV45F8xafmQEQN6ncU8k3cFIVdXw8
4Jq4NBlkIVngpbE7I1WqhiP5snFmJhdlpGjTdUvtKnSZNT/qMrdsyBwrOJ9SkmTO
NMG76HYXOGYJV0wxMh9PyABx+IzIHR/jdZ/5wDxhv76O/cV5oLcX/TK6UAjuQchO
drGAQFcbiwOlXv1wz8x4LrchcPgd2c5l9elozFLlDSKtukAnRIpgcNr71mv36/xF
bIICDu7Y9DBejbH0JPwumR3M6L4tVPAvgH1jcVzW28yF/qHrtfIoY+o1H/e7PF1v
XHfN4TUbhDg=
=sYfZ
-----END PGP PRIVATE KEY BLOCK-----
", Encoding.UTF8);
                }

                if (!File.Exists(_pgpSignatureKey))
                {
                    Directory.CreateDirectory(_pgpSignatureKey.Replace(Path.GetFileName(_pgpSignatureKey), ""));
                    File.WriteAllText(_pgpSignatureKey, @"-----BEGIN PGP PUBLIC KEY BLOCK-----
Version: BCPG C# v1.6.1.0

mQENBFn53zQBCADMlxdkpJRay9J1lA2Xc6fsyM0A7CRzcY5TCuU31ZROV0s8Z+97
hpwAdRzRXPfwh7465505tW2q3hqpOOyMmXXAbua8fVn/hklXxVnCB0WQFLHIL/Dm
UyJnRy9vyKkt5QGzAhLMDWtRn/bejIJ8jzB8WFjf+luRpR/GdvPjy8Q7/0tY6By4
Ua67nlHTFkHJ7iytE1ShbNHoe1BtsuEdwu5BmNHDkixpUAFvm5MoC0Xvib4yvAVj
1+r/U9w5AFGMoNZrYO4Ckg3ZKCHwNMvI0sD0zHnW36jGV4yoNBQ+Uc3bBaxZKoy9
DxLIWT+066UqD2V3pIcObQT8xVQybxIHzoxFABEBAAG0GHNhYnJpLmFyc2xhbkB2
ZXJzYXRhLmNvbYkBHAQQAQIABgUCWfnfNAAKCRBh2GERmFldvNVJB/9sFur6pakc
0xtlhRgCRH90qKmIhG+aybwNGiNdIs0ZWtoc1leORfMWn5kBEDep3FPJN3BSFXV8
POCauDQZZCFZ4KWxOyNVqoYj+bJxZiYXZaRo03VL7Sp0mTU/6jK3bMgcKzifUpJk
zjTBu+h2FzhmCVdMMTIfT8gAcfiMyB0f43Wf+cA8Yb++jv3FeaC3F/0yulAI7kHI
TnaxgEBXG4sDpV79cM/MeC63IXD4HdnOZfXpaMxS5Q0irbpAJ0SKYHDa+9Zr9+v8
RWyCAg7u2PQwXo2x9CT8LpkdzOi+LVTwL4B9Y3Fc1tvMhf6h67XyKGPqNR/3uzxd
b1x3zeE1G4Q4
=Yf0P
-----END PGP PUBLIC KEY BLOCK-----
", Encoding.UTF8);
                }

            }
        }
        
        /*
        private static TaskContext CreateContext()
        {
            return new TaskContext
            {
                EnvironmentId = _clientConfiguration.EnvironmentID, // Dev = 1, Qc = 2, Ua = 12, Production = 13
                BillingAdminConnection = Utility.BillingAdminDEV,
                Client = "Texpo",
                ClientName = "Texpo",
                Namespace = "Texpo",
                Abbreviation = "TXP",
                ClientId = 22,
                ClientConnection = _clientConfiguration.ConnectionCsr,
                MarketConnection = _clientConfiguration.ConnectionMarket,
                TDSPConnection = _clientConfiguration.ConnectionTdsp,
                TaxConnection = "",
                Logger = new ConsoleLogger()
            };
        }
        */
        public static void Main(string[] args)
        {
            _clientConfiguration = Utility.SetSecurity(Utility.BillingAdminDEV, Utility.Clients["TXP"]);
            
            // Set culture to en-EN to prevent string manipulation issues in base code
            Utility.SetThreadCulture("en-US");
            ExecuteExport();
            //ExecuteProcessTransactionRequests();
            //GenerateSimpleMarketTransactionEvaluationEvents();
            //ProcessEvents();
            
            /*
			CalculateConsumptionDueDatesTask myTask = new CalculateConsumptionDueDatesTask();

            myTask.Initialize(CreateContext());
            
            //execute Maintenance.CalculateConsumptionDueDates 
            myTask.Execute();
			*/
        }

        private static void ExecuteExport()
        {
            //exec csp_GetServiceMethods 2 (Export = ExportTransactions, EncryptFiles, TransmitFiles, ExportLoadForecasting, ProcessEFTPayments, RunAutoDNPProcess, LogService)
            var myExport = new MyExport(_clientConfiguration.ConnectionMarket, _clientConfiguration.ConnectionCsr, Utility.BillingAdminDEV);
            myExport.MyExportTransactions();
            //myExport.EncryptFiles();
            //myExport.MyTransmitFiles();
            //myExport.MyExportLoadForecasting();
            //myExport.MyProcessEFTPayments();
            //myExport.MyRunAutoDNPProcess();
            //myExport.MyLogService();

            /*
            var exporter = new CIS.Export.Billing.Market814(_clientConfiguration.ConnectionMarket, _clientConfiguration.ConnectionCsr)
            {
                HistoricalUsageRequestType = _clientConfiguration.ExportHistoricalUsageRequestType,
                ConnectionAdmin = Utility.BillingAdminDEV,
                Client = _clientConfiguration.ConnectionCsr,
                ClientID = Utility.Clients["TXP"]
            };
            exporter.Export();
            */
        }
        

        private static void GenerateSimpleMarketTransactionEvaluationEvents()
        {
            var hashTable = new Hashtable();
            var gen1 = new CIS.Framework.Event.EventGenerator.CustomerEvaluation(_clientConfiguration.ConnectionCsr, Utility.BillingAdminDEV);
            if (gen1.Generate(Utility.Clients["TXP"], hashTable))
            {
                Console.WriteLine("CustomerEvaluation Events Generated");
            }
            else
            {
                Console.WriteLine("CustomerEvaluation Events could not generated");
            }
            var gen = new CIS.Framework.Event.EventGenerator.SimpleMarketTransactionEvaluation(_clientConfiguration.ConnectionCsr, Utility.BillingAdminDEV);
            

            if (gen.Generate(Utility.Clients["TXP"], hashTable))
            {
                Console.WriteLine("SimpleMarketTransactionEvaluation Events Generated");
            }
            else
            {
                Console.WriteLine("SimpleMarketTransactionEvaluation Events could not generated");
            }
        }

        private static void ProcessEvents()
        {
            var engine = new CIS.Engine.Event.Queue(Utility.BillingAdminDEV);
            engine.ProcessEventQueue(_clientConfiguration.ClientID, _clientConfiguration.ConnectionCsr, _clientConfiguration.ConnectionMarket, _clientConfiguration.ClientAbbreviation);
        }
    }
}