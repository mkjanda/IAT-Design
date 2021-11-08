using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using System.Windows.Threading;

namespace Launcher
{
    /// <summary>
    /// Interaction logic for DownloadSplash.xaml
    /// </summary>
    public partial class DownloadSplash : Window
    {
        DispatcherTimer timer = new DispatcherTimer();
        private int downloadFrame = 0;
        public DownloadSplash()
        {
            InitializeComponent();
        }

        public new void Show()
        {
            base.Show();
            //timer.Interval = new TimeSpan(5000);
            //timer.Tick += new EventHandler(Timer_Tick);
            //timer.Start();
        }

        public new void Close()
        {
            //timer.Stop();
            base.Close();
        }
/*
        private void Timer_Tick(object sender, EventArgs e)
        {
            if (this.IsLoaded)
            {
                if (++downloadFrame > 2)
                    downloadFrame = 0;
                switch (downloadFrame)
                {
                    case 0:
                        Downloading1.Visibility = Visibility.Visible;
                        Downloading2.Visibility = Visibility.Hidden;
                        Downloading3.Visibility = Visibility.Hidden;
                        break;

                    case 1:
                        Downloading1.Visibility = Visibility.Hidden;
                        Downloading2.Visibility = Visibility.Visible;
                        Downloading3.Visibility = Visibility.Hidden;
                        break;

                    case 2:
                        Downloading1.Visibility = Visibility.Hidden;
                        Downloading2.Visibility = Visibility.Hidden;
                        Downloading3.Visibility = Visibility.Visible;
                        break;

                }
            }
        }*/
    }
}
