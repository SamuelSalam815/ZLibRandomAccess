using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace ArchiveAccessPointVisualizer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.FileName = "LogFile"; // Default file name
            dialog.DefaultExt = ".gz"; // Default file extension
            dialog.Filter = "Compressed Log Files|*.gz;*.gzap"; // Filter files by extension

            if (dialog.ShowDialog() is not true)
            {
                return;
            }

            var filename = dialog.FileName;
        }
    }
}
