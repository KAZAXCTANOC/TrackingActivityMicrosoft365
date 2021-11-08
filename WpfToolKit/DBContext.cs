using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfToolKit.Models;

namespace WpfToolKit
{
    public class DBContext : DbContext
    {
        public DbSet<UserList> UserLists { get; set; }
        public DbSet<User> Users { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //DataBase=MyTestDb: Name DB
            optionsBuilder.UseSqlServer(@"Data Source=P-C-31\MSSQLSERVER2;Database=MyTestDb;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
        }

        #region Этот метод изменяет положение точки
        /// Этот метод изменяет положение точки
        /// <summary>
        /// <list type="bullet|number|table">
        ///    <item>
        ///        Пример кода:
        ///            <code>
        ///                 optionsBuilder.UseSqlServer(@"Data Source=P-C-31\MSSQLSERVER2;Database=MyTestDb;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
        ///            </code>
        ///    </item>
        ///    <item>
        ///        <term>Пример термина</term>
        ///        <description>описание теримина</description>
        ///    </item>
        ///</list>
        ///    <seealso>
        ///     Третье описание
        ///    </seealso>
        ///</summary>
        private void FDfd(int xor, int yor)
        {
            int X = xor;
            int Y = yor;
        }
        #endregion

    }
}