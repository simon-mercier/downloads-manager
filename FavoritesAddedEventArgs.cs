using System;

namespace DownloadsManager
{
    public class FavoriteAddedEventArgs : EventArgs
    {
        public Path AddedFavorite { get; set; }
    }

}