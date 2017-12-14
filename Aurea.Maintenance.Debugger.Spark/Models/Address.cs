using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aurea.Maintenance.Debugger.Common.Models;

// ReSharper disable InconsistentNaming

namespace Aurea.Maintenance.Debugger.Spark.Models
{
    [Table("Address", PrimaryKey = "AddrId")]
    public class Address : ICopyableEntity
    {
        public int AddrID { get; set; }
        public int ValidationStatusID { get; set; }
        public string AttnMS { get; set; }
        public string Addr1 { get; set; }
        public string Addr2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string Zip4 { get; set; }
        public string DPBC { get; set; }
        public int CityID { get; set; }
        public int CountyID { get; set; }
        public string County { get; set; }
        public string HomePhone { get; set; }
        public string WorkPhone { get; set; }
        public string FaxPhone { get; set; }
        public string OtherPhone { get; set; }
        public string Email { get; set; }
        public string ESIID { get; set; }
        public string GeoCode { get; set; }
        public string Status { get; set; }
        public string DeliveryPointCode { get; set; }
        public DateTime CreateDate { get; set; }
        public string Migr_Enrollid { get; set; }
        public string PhoneExtension { get; set; }
        public string OtherExtension { get; set; }
        public string FaxExtension { get; set; }
        public string TaxingDistrict { get; set; }
        public short TaxInCity { get; set; }
        public int CchVersion { get; set; }
    }
}
