using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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

namespace HCWpf
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow() => InitializeComponent();
        private void ButtonLoadDataset_Click(object sender, RoutedEventArgs e)
        {
            string defaultDirectory = "../../../ExampleDatasets";

            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog
            {
                DefaultExt = ".csv",
                Filter = "Csv datasets (.csv)|*.csv"
            };

            if (Directory.Exists(defaultDirectory))
            {
                Trace.WriteLine("defaultDirectory exists");
                dlg.InitialDirectory = defaultDirectory;
            }
            Trace.WriteLine("but anyway");

            bool? result = dlg.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                // Open document
                string filename = dlg.FileName;
                MessageBox.Show(filename);
            }
        }
    }
}
