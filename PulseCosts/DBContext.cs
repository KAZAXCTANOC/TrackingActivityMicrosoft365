using Microsoft.EntityFrameworkCore;
using PulseCosts.Models;
using PulseCosts.Models.SqlDbModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PulseCosts
{
    class DBContext : DbContext
    {
        public DbSet<HistoryChange> HistoryChanges { get; set; }
        public DbSet<Materials> Materials { get; set; }
        public DbSet<Classifier> Classifiers { get; set; }
        public DbSet<Work> Works { get; set; }
        public DbSet<PulseCostTableElement> PulseCostTableElements { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=MicrosoftActivity;Username=postgres;Password=admin");
        }

    }
}
