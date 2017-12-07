using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
// ReSharper disable InconsistentNaming

namespace Aurea.Maintenance.Debugger.Spark.Models
{
    [Table("RateTransition")]
    [RequiredEntity(typeof(Customer), SourceField = "CustID")]
    [RequiredEntity(typeof(Rate), SourceField = "RateID")]
    public class RateTransition : ICopyableEntity
    {
        public int RateTransitionID { get; set; }
        public int CustID { get; set; }
        public int RateID { get; set; }
        public int UserID { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime SwitchDate { get; set; }
        public DateTime EndDate { get; set; }
        public int StatusID { get; set; }
        public DateTime SoldDate { get; set; }
        public short RolloverFlag { get; set; }
    }
}
