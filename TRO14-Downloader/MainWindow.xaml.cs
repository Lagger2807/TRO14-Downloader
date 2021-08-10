using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

namespace TRO14_Downloader
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            using (WebClient wc = new WebClient())
            {
                wc.Headers.Add("a", "a");
                try
                {
                    wc.DownloadFile("https://github.com/Lagger2807/TROP-Standard/raw/main/TROP%20Standard.zip", @"C:/Users/Lagger/Desktop/test.zip");
                    //SERVE REPO PUBBLICA
                }
                catch (Exception ex)
                {
                    //Console.WriteLine(ex.ToString());
                    MessageBox.Show(ex.ToString());
                }
            }
        }
    }
}
