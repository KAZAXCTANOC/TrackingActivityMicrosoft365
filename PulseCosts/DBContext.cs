using Microsoft.EntityFrameworkCore;
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
        public DBContext()
        {
            Database.EnsureCreated();
        }
        public DbSet<RowDataElement> Rows { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=MicrosoftActivity;Username=postgres;Password=admin");
        }
    }
}
