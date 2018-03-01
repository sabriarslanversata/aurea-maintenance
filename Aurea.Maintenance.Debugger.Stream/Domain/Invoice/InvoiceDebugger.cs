namespace Aurea.Maintenance.Debugger.Stream
{
    using System;
    using System.IO;
    using System.Diagnostics;
    using System.Reflection;
    using CIS.BusinessEntity;
    using CIS.Clients.Stream;
    using CIS.Correspondence.Core;
    using CIS.Correspondence.Services.Client;
    using Aurea.Maintenance.Debugger.Common.Models;
    using Aurea.Maintenance.Debugger.Stream.Domain.Invoice;

    public static class InvoiceDebugger
    {
        public static bool InvoiceGeneration(
            GlobalApplicationConfigurationDS.GlobalApplicationConfiguration applicationConfig,
            ClientEnvironmentConfiguration clientConfig,
            InvoiceGenerationInfo invoiceGenerationInfo)
        {
            var invoice = new Invoice(applicationConfig.ConnectionCsr)
            {
                ConnectionAdmin = clientConfig.ConnectionBillingAdmin,
                ClientID = clientConfig.ClientId
            };
#if DEBUG
            if (Debugger.IsAttached)
            {
                Debugger.Break();
            }
#endif
            return invoice.GenerateStandardInvoice(
                invoiceGenerationInfo.CustomerId,
                invoiceGenerationInfo.StartDate,
                invoiceGenerationInfo.EndDate,
                invoiceGenerationInfo.InvoiceDate);
        }

        public static bool InvoiceXmlGeneration(
            GlobalApplicationConfigurationDS.GlobalApplicationConfiguration applicationConfig,
            ClientEnvironmentConfiguration clientConfig,
            int invoiceId)
        {
            var invoice = new Invoice(applicationConfig.ConnectionCsr)
            {
                ConnectionAdmin = clientConfig.ConnectionBillingAdmin,
                ClientID = clientConfig.ClientId
            };
#if DEBUG
            if (Debugger.IsAttached)
            {
                Debugger.Break();
            }
#endif
            return invoice.GenerateInvoiceXml(invoiceId, true);
        }

        public static bool InvoicePdfGeneration(int invoiceId)
        {
#if DEBUG
            if (Debugger.IsAttached)
            {
                Debugger.Break();
            }
#endif
            //don't forget to place CIS.Clients.Stream.Invoices.dll into Aurea.Maintenance.Debugger.Stream\bin\Debug\Invoice\SGE\Invoice\Standard_V01 path!
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
