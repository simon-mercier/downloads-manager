using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;

namespace DownloadsManager
{
    class Router
    {
        public delegate void NewDownloadEventHandler(object sender, NewDownloadEventArgs e);
        public event NewDownloadEventHandler newDownloadEvent;

        public Path SelectedDirectory { get; set; } = null;
        public Path NewestDownload { get; set; } = null;

        public Router(Path downloadsPath)
        {
            CreateFileWatcher(downloadsPath);
        }

        public void CreateFileWatcher(Path path)
        {
            var watcher = new FileSystemWatcher
            {
                Path = path.FullPath,
            };

            watcher.Renamed += new RenamedEventHandler(OnNewDownload);
            
            watcher.EnableRaisingEvents = true;
        }
        public void RouteToSelectedDirectory()
        {
            if (SelectedDirectory == null || NewestDownload == null)
                return;

            if (!File.Exists(NewestDownload.FullPath))
                return;

            if (!Directory.Exists(SelectedDirectory.FullPath))
                Directory.CreateDirectory(SelectedDirectory.FullPath);

            var destPath = System.IO.Path.Combine(SelectedDirectory.FullPath, NewestDownload.PathName);

            File.Move(NewestDownload.FullPath, destPath, true);
            try
            {
                new Process { StartInfo = new ProcessStartInfo(destPath) { UseShellExecute = true } }.Start();
            }
            catch (Exception)
            {
                MessageBox.Show("Unable to open the download.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            
            NewestDownload = null;
        }

        private void OnNewDownload(object source, RenamedEventArgs e)
        {
            if (Regex.IsMatch(e.Name, @".crdownload"))
                return;

            var path = new Path(e.FullPath, e.Name);
            NewestDownload = path;
            newDownloadEvent(this, new NewDownloadEventArgs { NewPath = path });
        }
    }
}
