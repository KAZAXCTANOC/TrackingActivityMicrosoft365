using PulseCosts.Models.SqlDbModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PulseCosts.Models
{
    public class PulseCostTableElement : RowDataElement
    {
        public int MaterialId { get; set; }
        public int ClassifierId { get; set; }
        public int WorkId { get; set; }
        public Materials Material { get; set; }
        public Classifier Classifier { get; set; }
        public Work Work { get; set; }

    }
}
