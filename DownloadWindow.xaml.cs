using Microsoft.Win32;
using MW3Download.Download;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
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
using System.Windows.Shapes;
using System.Xml.Linq;

namespace MW3Download
{
    /// <summary>
    /// Interaction logic for DownloadWindow.xaml
    /// </summary>
    public partial class DownloadWindow : Window
    {
        private string gameUrl = "https://EXAMPLE.com/EXAMPLE_MW3_FILE.zip";
        private string dlcUrl = "";
        private string teknoMW3Url = "";

        private bool downloadGame => global::MW3Download.Properties.Settings.Default.DownloadGame;
        private bool downloadTeknoMW3 => global::MW3Download.Properties.Settings.Default.DownloadTeknoMW3;
        private bool downloadDlc => global::MW3Download.Properties.Settings.Default.DownloadDlc;

        bool gameDone = false, teknomw3Done = false, dlcDone = false, paused = true;

        string currentDownload = "None";

        private NumberFormatInfo numberFormat = NumberFormatInfo.InvariantInfo;
        private WebDownloadClient download = null;
        private System.Windows.Forms.Timer tm = new System.Windows.Forms.Timer();

        public DownloadWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var drs = MessageBox.Show("Start downloading to " + global::MW3Download.Properties.Settings.Default.DownloadPath +  " ?", "Start Downloading?", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (drs == MessageBoxResult.Yes)
            {
                tm.Interval = 10;
                tm.Start();

                if (downloadGame)
                {
                    startDownload(gameUrl, "Modern-Warfare-3.rar");
                    currentDownload = "Main Game Stuff";
                }
                else if (downloadDlc)
                {
                    startDownload(dlcUrl, "DLCs.rar");
                    currentDownload = "DLCs";
                }
                else if (downloadTeknoMW3)
                {
                    startDownload(teknoMW3Url, "TeknoMW3.rar");
                    currentDownload = "TeknoMW3 Stuff";
                }

                if (!downloadGame)
                    gameDone = true;
                if (!downloadDlc)
                    dlcDone = true;
                if (!downloadTeknoMW3)
                    teknomw3Done = true;

                SystemEvents.SessionEnding += new SessionEndingEventHandler(SystemEvents_SessionEnding);
                tm.Tick += Tm_Tick;
            }
            else
            {
                drs = MessageBox.Show("Download canceled, exiting...", "Notice", MessageBoxButton.OK, MessageBoxImage.Information);
                Environment.Exit(-1);
            }
        }

        private void Tm_Tick(object sender, EventArgs e)
        {
            if (download != null)
            {
                lblSpeed.Content = download.DownloadSpeed.ToString();
                lblElapsed.Content = download.TotalElapsedTimeString;
                lblTime.Content = download.TimeLeft;
                lblDownloaded.Content = download.DownloadedSizeString;
                prgbarDownloadPercent.Value = download.Progress;
            }
        }

        private void startDownload(string url, string fileName)
        {
            try
            {
                download = new WebDownloadClient(url);

                download.FileName = fileName;

                // Register WebDownloadClient events
                download.DownloadProgressChanged += download.DownloadProgressChangedHandler;
                download.DownloadCompleted += download.DownloadCompletedHandler;
                download.PropertyChanged += Download_PropertyChanged;
                download.StatusChanged += Download_StatusChanged;
                download.DownloadCompleted += Download_DownloadCompleted; 

                // Create path to temporary file
                if (!Directory.Exists(System.IO.Path.GetFullPath(global::MW3Download.Properties.Settings.Default.DownloadPath)))
                {
                    Directory.CreateDirectory(System.IO.Path.GetFullPath(global::MW3Download.Properties.Settings.Default.DownloadPath));
                }
                string filePath = System.IO.Path.GetFullPath(global::MW3Download.Properties.Settings.Default.DownloadPath) + fileName;
                string tempPath = filePath + ".tmp";

                // Check if there is already an ongoing download on that path
                if (File.Exists(tempPath))
                {
                    string message = "There is already a download in progress at the specified path.";
                    Xceed.Wpf.Toolkit.MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Check if the file already exists
                if (File.Exists(filePath))
                {
                    string message = "There is already a file with the same name, do you want to overwrite it? "
                                   + "If not, please change the file name or download folder.";
                    MessageBoxResult result = MessageBox.Show(message, "File Name Conflict: " + filePath, MessageBoxButton.YesNo, MessageBoxImage.Warning);

                    if (result == MessageBoxResult.Yes)
                        File.Delete(filePath);
                    else
                        return;
                }

                // Check the URL
                download.CheckUrl();
                if (download.HasError)
                    return;

                download.TempDownloadPath = tempPath;

                download.AddedOn = DateTime.UtcNow;
                download.CompletedOn = DateTime.MinValue;
                //download.OpenFileOnCompletion = this.cbOpenFileOnCompletion.IsChecked.Value;

                // Add the download to the downloads list
                DownloadManager.Instance.DownloadsList.Add(download);

                // Start downloading the file
                if (paused)
                    download.Start();
                else
                {
                    download.Pause();
                    download.Status = DownloadStatus.Paused;
                }

                paused = !paused;

                Title = currentDownload;
                lblCurrentDownload.Content = currentDownload;
            }
            catch (Exception ex)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Download_DownloadCompleted(object sender, EventArgs e)
        {
            WebDownloadClient download = (WebDownloadClient)sender;

            MessageBox.Show(download.Status.ToString());
            if (download.Status == DownloadStatus.Completed)
            {
                lblSpeed.Content = "0 kb/s";
                lblElapsed.Content = download.TotalElapsedTimeString;
                lblTime.Content = download.TimeLeft;
                lblDownloaded.Content = download.DownloadedSizeString;
                prgbarDownloadPercent.Value = 100;

                if(currentDownload == "Main Game Stuff")
                    gameDone = true;
                else if (currentDownload == "DLCs")
                    dlcDone = true;
                else if (currentDownload == "TeknoMW3 Stuff")
                    teknomw3Done = true;

                currentDownload = "None";

                Title = currentDownload;
                lblCurrentDownload.Content = currentDownload;

                if (downloadGame && !gameDone)
                {
                    startDownload(gameUrl, "Modern-Warfare-3.rar");
                    currentDownload = "Main Game Stuff";
                }
                else if (downloadDlc && !dlcDone)
                {
                    startDownload(dlcUrl, "DLCs.rar");
                    currentDownload = "DLCs";
                }
                else if (downloadTeknoMW3 && !teknomw3Done)
                {
                    startDownload(teknoMW3Url, "TeknoMW3.rar");
                    currentDownload = "TeknoMW3 Stuff";
                }

                //string title = "Download Completed";
                //string text = download.FileName + " has finished downloading.";

                //XNotifyIcon.ShowBalloonTip(title, text, BalloonIcon.Info);
            }
        }

        private void Download_StatusChanged(object sender, EventArgs e)
        {
            WebDownloadClient dl = (WebDownloadClient)sender;
            if (dl.Status == DownloadStatus.Paused || dl.Status == DownloadStatus.Completed
                || dl.Status == DownloadStatus.Deleted || dl.HasError)
            {
                foreach (WebDownloadClient d in DownloadManager.Instance.DownloadsList)
                {
                    if (d.Status == DownloadStatus.Queued)
                    {
                        d.Start();
                        break;
                    }
                }
            }

            foreach (WebDownloadClient d in DownloadManager.Instance.DownloadsList)
            {
                if (d.Status == DownloadStatus.Downloading)
                {
                    d.SpeedLimitChanged = true;
                }
            }

            int active = DownloadManager.Instance.ActiveDownloads;
            int completed = DownloadManager.Instance.CompletedDownloads;

            //if (active > 0)
            //{
            //    if (completed == 0)
            //        this.statusBarActive.Content = " (" + active + " Active)";
            //    else
            //        this.statusBarActive.Content = " (" + active + " Active, ";
            //}
            //else
            //    this.statusBarActive.Content = String.Empty;

            //if (completed > 0)
            //{
            //    if (active == 0)
            //        this.statusBarCompleted.Content = " (" + completed + " Completed)";
            //    else
            //        this.statusBarCompleted.Content = completed + " Completed)";
            //}
            //else
            //    this.statusBarCompleted.Content = String.Empty;
        }

        private void Download_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            
        }

        private string getFreeDiskSpace()
        {
            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady && drive.Name == global::MW3Download.Properties.Settings.Default.DownloadPath.Substring(0,1))
                {
                    long freeSpace = drive.AvailableFreeSpace;
                    double mbFreeSpace = (double)freeSpace / Math.Pow(1024, 2);
                    double gbFreeSpace = mbFreeSpace / 1024D;

                    if (freeSpace < Math.Pow(1024, 3))
                    {
                        return mbFreeSpace.ToString("#.00", numberFormat) + " MB";
                    }
                    return gbFreeSpace.ToString("#.00", numberFormat) + " GB";
                }
            }
            return String.Empty;
        }

        private void SystemEvents_SessionEnding(object sender, SessionEndingEventArgs e)
        {

        }

        private void btnChangeState_Click(object sender, RoutedEventArgs e)
        {
            if (paused)
                download.Start();
            else
                download.Pause();
            paused = !paused;

            btnChangeState.Content = paused ? "Start" : "Pause";
        }
    }
}
