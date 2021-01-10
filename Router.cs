using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;

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

            var destFile = System.IO.Path.Combine(SelectedDirectory.FullPath, NewestDownload.PathName);

            File.Move(NewestDownload.FullPath, destFile, true);

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
