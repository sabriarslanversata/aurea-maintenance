﻿namespace Aurea.Maintenance.Debugger.Startex.Domain.Invoice
{
    using System;

    public class InvoiceGenerationInfo
    {
        public int CustomerId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime InvoiceDate { get; set; }
    }
}
