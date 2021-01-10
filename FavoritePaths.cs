using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;

namespace DownloadsManager
{
    public sealed class FavoritePaths
    {
        private static FavoritePaths instance;
        public static FavoritePaths Instance => instance ??= new FavoritePaths();

        public delegate void FavoriteAddedEventHandler(object sender, FavoriteAddedEventArgs e);

        public event FavoriteAddedEventHandler FavoriteAddedEvent;
        public List<Path> Favorites { get; private set; }

        private static readonly Path favoritesPath = new Path($"C:\\Users\\mison\\source\\repos\\DownloadsManager\\Favorites\\Favorites.JSON", "Favorites");

        public FavoritePaths()
        {
            Favorites = DeserializeFavorites() ?? new List<Path>();
        }


        private List<Path> DeserializeFavorites()
        {
            try
            {
               return JsonConvert.DeserializeObject<List<Path>>(File.ReadAllText(favoritesPath.FullPath));
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), null, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return null;
        }

        public bool RemoveFavorite(Path path)
        {
            Favorites.Remove(path);
            return SerializeFavorites();
        }

        public bool AddFavorite(Path path)
        {
            Favorites.Add(path);
            FavoriteAddedEvent(this, new FavoriteAddedEventArgs { AddedFavorite = path });
            return SerializeFavorites();
        }

        public bool EditFavorite(Path path, string newPath, string newName)
        {
            Favorites[Favorites.IndexOf(path)] = new Path(newPath, newName);
            return SerializeFavorites();
        }

        private bool SerializeFavorites()
        {
            try
            {
                File.WriteAllText(favoritesPath.FullPath, JsonConvert.SerializeObject(Favorites));
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), null, MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public void SetFavorites(List<Path> newFavorites)
        {
            Favorites = newFavorites;
            SerializeFavorites();
        }
    }
}
