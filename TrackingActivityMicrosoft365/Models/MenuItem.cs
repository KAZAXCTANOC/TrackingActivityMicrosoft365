using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackingActivityMicrosoft365.Models
{
    class MenuItem
    {
        public MenuItem()
        {
            this.Items = new ObservableCollection<MenuItem>();
        }
        public string Title { get; set; }
        public ObservableCollection<MenuItem> Items { get; set; }
    }
}
