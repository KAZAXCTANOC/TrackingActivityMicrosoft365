using PulseCosts.Models;
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

        public PulseCostTableElement GetDataElement(string Id)
        {
            DBContext DbContext = new DBContext();
            PulseCostTableElement row = DbContext.PulseCostTableElements.Where(El => El.RowName == Id).FirstOrDefault();
            try
            {
                row.Material = DbContext.Materials.Where(m => m.Id == row.MaterialId).FirstOrDefault();
                row.Work = DbContext.Works.Where(m => m.Id == row.WorkId).FirstOrDefault();
                row.Classifier = DbContext.Classifiers.Where(m => m.Id == row.ClassifierId).FirstOrDefault();
            }
            catch (Exception e)
            {
                return null;
            }

            return row;
        }
        public void CreateRow(PulseCostTableElement rowDataElement)
        {
            _DbContext.PulseCostTableElements.Add(rowDataElement);
            _DbContext.SaveChanges();
        }

        public void UpdatePulseCostTableElement(PulseCostTableElement rowDataElement, string Id)
        {
            DBContext DbContext = new DBContext();
            PulseCostTableElement row = GetDataElement(Id);

            var work = DbContext.Works.Where(w => w.Id == row.WorkId).FirstOrDefault();
            work.B = rowDataElement.Work.B;
            work.C = rowDataElement.Work.C;
            work.D = rowDataElement.Work.D;
            work.E = rowDataElement.Work.E;
            work.F = rowDataElement.Work.F;

            DbContext.SaveChanges();
        }
    }
}
