using PulseCosts.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PulseCosts
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MicrosfotAcytivityController microsfotAcytivity = new MicrosfotAcytivityController();

            Thread thread1 = new Thread(new ThreadStart(MyThreed1));
            thread1.Start();

            Thread thread2 = new Thread(new ThreadStart(MyThreed2));
            thread2.Start();
        }

        #region Методы для создания потоков
        public void MyThreed1()
        {
            MicrosfotAcytivityController microsfotAcytivity = new MicrosfotAcytivityController();
            while (true)
            {
                var a = Task.Run(() => microsfotAcytivity.TrakingActivityCalcTemplateAsync("D")).Result;
            }
        }

        public void MyThreed2()
        {
            MicrosfotAcytivityController microsfotAcytivity = new MicrosfotAcytivityController();
            while (true)
            {
                var a = Task.Run(() => microsfotAcytivity.TrakingActivityCalcTemplateAsync("H")).Result;
            }
        }
        #endregion
    }
}
