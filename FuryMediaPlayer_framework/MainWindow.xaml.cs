using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace FuryMediaPlayer_framework
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public bool isCmd = false;
        //MediaPlayerWindow
        views.templates.MediaPlayerWindow playerWindow = new views.templates.MediaPlayerWindow();
        public MainWindow()
        {
            InitializeComponent();
            _ = isStartingAsync();
            
            if (Environment.GetCommandLineArgs().Length > 1)
            {
                isCmd = true;
                Close();
                playerWindow.Show();
            }
        }

        private async Task<bool> isStartingAsync()
        {
            await Task.Delay(2000);
            loadingBar.Visibility = Visibility.Visible;

            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += worker_doWork;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            worker.RunWorkerAsync();
            return true;
        }

        private async void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            await Task.Delay(1000);
            loadingBar.Visibility = Visibility.Collapsed;
            await Task.Delay(500);

            if (isCmd == false)
            {
                Close();
                playerWindow.Show();
            }
        }

        private void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            loadingBar.Value = e.ProgressPercentage;
        }

        private void worker_doWork(object sender, DoWorkEventArgs e)
        {
            for (int i = 0; i <= 100; i++)
            {
                (sender as BackgroundWorker).ReportProgress(i);
                Thread.Sleep(15);
            } 
        }
    }
}
