using System;

namespace DownloadsManager
{
    public class NewDownloadEventArgs : EventArgs
    {
        public Path NewPath { get; set; }
    }
}