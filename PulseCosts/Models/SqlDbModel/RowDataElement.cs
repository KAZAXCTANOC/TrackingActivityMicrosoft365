using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PulseCosts.Models.SqlDbModel
{
    public class RowDataElement
    {
        public int Id { get; set; }
        public string RowName { get; set; }
        public string Data { get; set; }
        public DateTime ChangeTime { get; set; }
    }
}
