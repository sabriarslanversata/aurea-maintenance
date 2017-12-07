using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
// ReSharper disable InconsistentNaming

namespace Aurea.Maintenance.Debugger.Spark.Models
{
    [Table("EdiLoadProfile")]
    public class EdiLoadProfile:ICopyableEntity
    {
        public int EdiLoadProfileId { get; set; }
        public string LoadProfile { get; set; }
        public int LDCCOde { get; set; }
        public int Migr_LoadProfile { get; set; }
    }
}
