using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PulseCosts.Models
{
    public class PulseCostTableElement
    {
        public Materials Material { get; set; }
        public Classifier Classifier { get; set; }
        public Work Work { get; set; }

    }
}
