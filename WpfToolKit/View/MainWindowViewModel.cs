using LiveCharts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using WpfToolKit.Command;
using WpfToolKit.Models;

namespace WpfToolKit.View
{
    class MainWindowViewModel : BaseModel
    {
        public List<UserList> UserLists { get; set; } = new List<UserList>();
        public List<User> Users { get; set; } = new List<User>();
        public ICommand Create { get; }
        private void CreateCommand(object p)
        {
            DBContext context = new DBContext();
            User user = new User { Name = "OLEG" };
            context.Users.Add(user);
            context.SaveChanges();

            Users.Add(user);
            OnPropertyChanged(nameof(Users));
        }
        public MainWindowViewModel()
        {
            DBContext context = new DBContext();
            UserLists = context.UserLists.ToList();
            Create = new LambdaCommand(CreateCommand);
        }
    }
}
