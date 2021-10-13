using PulseCosts.Models.SqlDbModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PulseCosts.Controllers
{
    class DBController
    {
        private DBContext _DbContext { get; set; }
        public DBController()
        {
            _DbContext = new DBContext();
        }

        public RowDataElement GetDataElement(string Id)
        {
            return _DbContext.Rows.Where(El => El.RowName == Id).FirstOrDefault();
        }
        public void CreateRow(RowDataElement rowDataElement)
        {
            _DbContext.Rows.Add(rowDataElement);
            _DbContext.SaveChanges();
        }
    }
}
