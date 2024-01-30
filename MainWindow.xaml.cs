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

namespace MW3Download
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string defaultPath = System.IO.Path.GetFullPath("./CoDMW3/");
        private bool state1 => chckboxDownloadDlc.IsChecked == true && chckboxDownloadTeknoMW3.IsChecked == true && chckboxDownloadGame.IsChecked == true;
        private bool state2 => ((chckboxDownloadDlc.IsChecked == true || chckboxDownloadDlc.IsChecked == false)
            && (chckboxDownloadTeknoMW3.IsChecked == true || chckboxDownloadTeknoMW3.IsChecked == false)
            && (chckboxDownloadGame.IsChecked == true || chckboxDownloadGame.IsChecked == false));
        private bool state3 => chckboxDownloadDlc.IsChecked == false && chckboxDownloadTeknoMW3.IsChecked == false && chckboxDownloadGame.IsChecked == false;

        public MainWindow()
        {
            InitializeComponent();
            global::MW3Download.Properties.Settings.Default.DownloadPath = defaultPath;
            txtboxDownloadPath.Text = global::MW3Download.Properties.Settings.Default.DownloadPath;
        }

        private void chckboxFullDownload_Click(object sender, RoutedEventArgs e)
        {
            if (chckboxFullDownload.IsChecked == false || chckboxFullDownload.IsChecked == null)
            {
                chckboxDownloadDlc.IsEnabled = true;
                chckboxDownloadTeknoMW3.IsEnabled = true;
                chckboxDownloadGame.IsEnabled = true;
                chckboxDownloadGame.IsChecked = false;
                chckboxDownloadDlc.IsChecked = false;
                chckboxDownloadTeknoMW3.IsChecked = false;
            }
            else if (chckboxFullDownload.IsChecked == true)
            {
                chckboxDownloadDlc.IsEnabled = false;
                chckboxDownloadTeknoMW3.IsEnabled = false;
                chckboxDownloadGame.IsEnabled = false;
                chckboxDownloadGame.IsChecked = true;
                chckboxDownloadDlc.IsChecked = true;
                chckboxDownloadTeknoMW3.IsChecked = true;
            }
        }

        private void checkbox_Click(object sender, RoutedEventArgs e)
        {
            if (state1)
                chckboxFullDownload.IsChecked = true;
            else if (state2)
                chckboxFullDownload.IsChecked = null;
            else if (state3)
                chckboxFullDownload.IsChecked = false;
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            global::MW3Download.Properties.Settings.Default.DownloadPath = System.IO.Path.GetFullPath(txtboxDownloadPath.Text);
            global::MW3Download.Properties.Settings.Default.DownloadTeknoMW3 = chckboxDownloadTeknoMW3.IsChecked.Value || chckboxFullDownload.IsChecked.Value;
            global::MW3Download.Properties.Settings.Default.DownloadDlc = chckboxDownloadDlc.IsChecked.Value || chckboxFullDownload.IsChecked.Value;
            global::MW3Download.Properties.Settings.Default.FullDownload = chckboxFullDownload.IsChecked.Value;
            global::MW3Download.Properties.Settings.Default.DownloadGame = chckboxDownloadGame.IsChecked.Value || chckboxFullDownload.IsChecked.Value;

            if (!System.IO.Directory.Exists(global::MW3Download.Properties.Settings.Default.DownloadPath))
            {
                if (global::MW3Download.Properties.Settings.Default.DownloadPath != defaultPath)
                {
                    MessageBox.Show("Directory is invaild", "Error...", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                System.IO.Directory.CreateDirectory(defaultPath);
            }

            // TODO: Open download window
            var dlw = new DownloadWindow();
            this.Close();
            dlw.Show();
        }

        private void btnExplorePath_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.DialogResult dgr;
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                dialog.SelectedPath = System.IO.Path.GetFullPath(global::MW3Download.Properties.Settings.Default.DownloadPath);
                dialog.ShowDialog();
                dialog.SelectedPath += dialog.SelectedPath.EndsWith("\\") ? string.Empty : "\\";
                txtboxDownloadPath.Text = System.IO.Path.GetFullPath(dialog.SelectedPath);
                global::MW3Download.Properties.Settings.Default.DownloadPath = System.IO.Path.GetFullPath(dialog.SelectedPath);
            }
        }

        private void btnCredits_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
