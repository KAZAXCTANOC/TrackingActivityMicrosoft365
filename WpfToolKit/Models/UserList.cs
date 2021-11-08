using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfToolKit.Models
{
    public class UserList
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User 
        { 
            get
            {
                DBContext context = new DBContext();
                return context.Users.Where(U => U.Id == UserId).FirstOrDefault();
            }
        }
    }
}
