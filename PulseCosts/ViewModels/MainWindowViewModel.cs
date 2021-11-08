using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Markup;

namespace PulseCosts.ViewModels
{
    class MainWindowViewModel : BaseViewMolel
    {
        public string document { get; set; }
        private void Update()
        {
            FileInfo log = new FileInfo($@"C:\Users\Ippolitov\Desktop\Logger\log.txt");
            while (true)
            {
                using (var streamReader = new StreamReader(log.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite), System.Text.Encoding.Default))
                {
                    document = streamReader.ReadToEnd();
                    OnPropertyChanged(nameof(document));
                }
                Thread.Sleep(1000);
            }
        }
        public MainWindowViewModel()
        {
            Task.Run(() => Update());
        }
    }
}
