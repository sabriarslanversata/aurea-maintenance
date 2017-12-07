using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
// ReSharper disable InconsistentNaming

namespace Aurea.Maintenance.Debugger.Spark.Models
{
    [Table("Meter")]
    [RequiredEntity(typeof(EdiLoadProfile), SourceField = "EdiLoadProfileId")]
    public class Meter:ICopyableEntity
    {
        public int MeterID { get; set; }
        public int ESIIDID { get; set; }
        public int AcctID { get; set; }
        public int AddrID { get; set; }
        public int TypeID { get; set; }
        public int PremID { get; set; }
        public string MeterNo { get; set; }
        public string MeterUniqueNo { get; set; }
        public string Pool { get; set; }
        public string MeterReadType { get; set; }
        public string MeterFactoryID { get; set; }
        public decimal MeterFactor { get; set; }
        public decimal BegRead { get; set; }
        public decimal EndRead { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime Dateto { get; set; }
        public string MeterStatus { get; set; }
        public int SourceLevel { get; set; }
        public DateTime CreateDate { get; set; }
        public int EdiRateClassId { get; set; }
        public int EdiLoadProfileId { get; set; }
        public string AMSIndicator { get; set; }
    }
}
