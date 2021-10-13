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
            Microsoft365Controller microsoft365 = new Microsoft365Controller();

            #region Потоки для отслеживания данных

            Thread thread1 = new Thread(new ThreadStart(MyThreed1));
            thread1.Start();

            Thread thread = new Thread(new ThreadStart(MyThreed2));
            thread.Start();

            //Thread thread3 = new Thread(new ThreadStart(MyThreed3));
            //thread3.Start();

            Thread thread4 = new Thread(new ThreadStart(MyThreed4));
            thread4.Start();
            #endregion
        }

        #region Методы для создания потоков
        public void MyThreed1()
        {
            Microsoft365Controller microsoft365 = new Microsoft365Controller();
            while (true)
            {
                var a = Task.Run(() => microsoft365.TrakingChangeAsync(Microsoft365Controller.SingAndReturnMe(), 
                    "01ADEVTET6J647IDZNJVB2N56SMNAJ7YAT", "892eb031-5560-4d2c-9142-0030091aabfa", "D")).Result;
            }
        }
        public void MyThreed2()
        {
            Microsoft365Controller microsoft365 = new Microsoft365Controller();

            while (true)
            {
                var a = Task.Run(() => microsoft365.TrakingChangeAsync(Microsoft365Controller.SingAndReturnMe(),
                    "01ADEVTET6J647IDZNJVB2N56SMNAJ7YAT", "892eb031-5560-4d2c-9142-0030091aabfa", "H")).Result;
            }
        }

        public void MyThreed3()
        {
            Microsoft365Controller microsoft365 = new Microsoft365Controller();

            while (true)
            {
                var a = Task.Run(() => microsoft365.TrakingChangeAsync(Microsoft365Controller.SingAndReturnMe(),
                    "01ADEVTET6J647IDZNJVB2N56SMNAJ7YAT", "892eb031-5560-4d2c-9142-0030091aabfa", "С")).Result;
            }
        }

        public void MyThreed4()
        {
            Microsoft365Controller microsoft365 = new Microsoft365Controller();

            while (true)
            {
                var a = Task.Run(() => microsoft365.TrakingChangeAsync(Microsoft365Controller.SingAndReturnMe(),
                    "01ADEVTET6J647IDZNJVB2N56SMNAJ7YAT", "892eb031-5560-4d2c-9142-0030091aabfa", "G")).Result;
            }
        }
        #endregion
    }
}
