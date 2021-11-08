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
using log4net;

namespace PulseCosts
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public string Collumn { get; set; }
        public int RowNumber { get; set; }
        public List<Task> tasks { get; set; } = new List<Task>();
        public MainWindow()
        {
            log4net.Config.XmlConfigurator.Configure();
            InitializeComponent();

            string IdDocument = "01ADEVTET6J647IDZNJVB2N56SMNAJ7YAT";
            string IdGroup = "892eb031-5560-4d2c-9142-0030091aabfa";

            var a = Task.Run(() => MicrosoftActivityHelper.MyClient.Groups[IdGroup].Drive.Items[IdDocument].Workbook.CreateSession(true).Request().PostAsync().Result);
            MicrosfotAcytivityController microsfotAcytivity = new MicrosfotAcytivityController();

            tasks.Add(MyThreed1Async());
            tasks.Add(MyThreed2Async());

            Thread thread = new Thread(new ThreadStart(TaskWait));
        }


        #region Методы для создания потоков
        public void TaskWait()
        {
            Task.WaitAll(tasks.ToArray());
        }

        public async Task MyThreed1Async()
        {
            log.Info($"Поток запущен {Collumn}{RowNumber}");
            try
            {
                MicrosfotAcytivityController microsfotAcytivity = new MicrosfotAcytivityController();
                while (true)
                {
                    await microsfotAcytivity.TrakingActivityCalcTemplateAsync("D");
                }
            }
            catch (Exception e)
            {
                MicrosoftActivityHelper.MyClient = MicrosoftActivityHelper.SingAndReturnMe();
                log.Error($"{e}");
            }
        }

        public async Task MyThreed2Async()
        {
            log.Info($"Поток запущен {Collumn}{RowNumber}");
            try
            {
                MicrosfotAcytivityController microsfotAcytivity = new MicrosfotAcytivityController();
                while (true)
                {
                    await microsfotAcytivity.TrakingActivityCalcTemplateAsync("H");
                }
            }
            catch (Exception e)
            {
                MicrosoftActivityHelper.MyClient = MicrosoftActivityHelper.SingAndReturnMe();
                log.Error($"{e}");
            }
        }
        #endregion
    }
}
