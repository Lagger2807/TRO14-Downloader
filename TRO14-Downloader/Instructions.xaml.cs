using System.Windows;


namespace TRO14_Downloader
{
    /// <summary>
    /// Logica di interazione per Instructions.xaml
    /// </summary>
    public partial class Instructions : Window
    {
        public Instructions()
        {
            InitializeComponent();
        }

        private void Btn_Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
