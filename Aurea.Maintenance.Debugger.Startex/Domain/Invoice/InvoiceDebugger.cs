namespace Aurea.Maintenance.Debugger.Spark
{
    using System;
    using System.Diagnostics;
    using CIS.BusinessEntity;
    using CIS.Clients.StarTex;
    using Aurea.Maintenance.Debugger.Common.Models;
    using System.IO;
    using System.Reflection;
    using CIS.Correspondence.Core;
    using CIS.Correspondence.Services.Client;

    public static class InvoiceDebugger
    {
        public static bool InvoiceGeneration(
            GlobalApplicationConfigurationDS.GlobalApplicationConfiguration applicationConfig,
            ClientEnvironmentConfiguration clientConfig)
        {
            // Setup - Update values of the variables below to debug your case.
            var customerId = 368083;
            var startDate = new DateTime(2017, 6, 30);
            var endDate = new DateTime(2017, 7, 31);
            var invoiceDate = DateTime.Now;

            // Initialize and setup invoice.
            Invoice invoice = new Invoice(applicationConfig.ConnectionCsr);
            invoice.ConnectionAdmin = clientConfig.ConnectionBillingAdmin;
            invoice.ClientID = clientConfig.ClientId;

            // Generate standart invoice. Set break to alert debugger!
#if DEBUG
            if (Debugger.IsAttached)
            {
                Debugger.Break();
            }
#endif
            var isSuccess = invoice.GenerateStandardInvoice(customerId, startDate, endDate, invoiceDate);

            return isSuccess;
        }

        public static bool InvoiceXmlGeneration(
            GlobalApplicationConfigurationDS.GlobalApplicationConfiguration applicationConfig,
            ClientEnvironmentConfiguration clientConfig,
            int invoiceId)
        {
            // Setup - Update values of the variables below to debug your case.
            

            // Initialize and setup invoice.
            Invoice invoice = new Invoice(applicationConfig.ConnectionCsr);
            invoice.ConnectionAdmin = clientConfig.ConnectionBillingAdmin;
            invoice.ClientID = clientConfig.ClientId;

            // Generate standart invoice XML. Set break to alert debugger!
#if DEBUG
            if (Debugger.IsAttached)
            {
                Debugger.Break();
            }
#endif
            var isSuccess = invoice.GenerateInvoiceXml(invoiceId, true);

            return isSuccess;
        }

        public static bool InvoicePdfGeneration(int invoiceId)
        {
            // Generate standart invoice PDF. Set break to alert debugger!
#if DEBUG
            if (Debugger.IsAttached)
            {
                Debugger.Break();
            }
#endif

            var executingAssembly = Assembly.GetExecutingAssembly();
            var appPath = Path.GetDirectoryName(executingAssembly.Location);
            var generatedPdfFilePath = Path.Combine(appPath, invoiceId + ".pdf");

            new Aspose.Words.License().SetLicense("Aspose.Words.Product.Family.lic");

            using (var fileStream = File.OpenWrite(generatedPdfFilePath))
            {
                ContentProxy.Instance.GetContent(CategoryType.Invoice, invoiceId, fileStream);
            }

            return true;
        }

        
    }
}
