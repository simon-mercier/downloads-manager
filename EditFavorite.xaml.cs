using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DownloadsManager
{
    /// <summary>
    /// Interaction logic for EditFavorite.xaml
    /// </summary>
    public partial class EditFavorite : Window
    {
        private Path selectedPath = Path.Empty;
        private Path defaultfavorite = Path.Empty;


        public EditFavorite(string defaultName = "Name...", string defaultPath = "Select path...")
        {
            InitializeComponent();

            NameInput.Text = defaultName;
            SelectPath.Content = defaultPath;

            selectedPath.PathName = defaultName;
            selectedPath.FullPath = defaultPath;
            this.defaultfavorite = selectedPath;

        }

        private void CancelExecution(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void EditTheFavorite(object sender, RoutedEventArgs e)
        {
            if(selectedPath.PathName == "")
            {
                MessageBox.Show("Please chose a name for the path.", "Invalid Path", MessageBoxButton.OK, MessageBoxImage.Error); 
                return;
            }
                

            if (FavoritePaths.Instance.Favorites.Any(x => x.PathName == selectedPath.PathName && x.PathName != defaultfavorite.PathName))
            {
                MessageBox.Show($"The name \"{selectedPath.PathName}\" is already used for another path. \nPlease chose another name.", "Invalid Path", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }


            if (selectedPath.FullPath == "")
            {
                MessageBox.Show("Please chose a path.", "Invalid Path", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
                

            if (!Directory.Exists(selectedPath.FullPath))
            {
                MessageBox.Show("Please chose an existing path.", "Invalid Path", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
                

            if (FavoritePaths.Instance.Favorites.Any(x => x.FullPath == selectedPath.FullPath && x.FullPath != defaultfavorite.FullPath))
            {
                MessageBox.Show($"The path \"{selectedPath.FullPath}\" is already added as a favorite. \nPlease chose another path.", "Invalid Path", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
                
            FavoritePaths.Instance.EditFavorite(defaultfavorite, selectedPath.FullPath, selectedPath.PathName);
            this.Close();
        }

        private void NameInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            selectedPath = new Path(selectedPath.FullPath, ((TextBox)sender).Text);
            //NameInput.Background = selectedPath.PathName == "" || !FavoritePaths.Instance.Favorites.Any(x => x.PathName == selectedPath.PathName) ? Colors.INVALID_RED : Brushes.White;
        }

        private void SelectPath_Click(object sender, RoutedEventArgs e)
        {
            var path = Utils.GetPathFromExplorer();
            if (path == Path.Empty)
                return;
            
            selectedPath = new Path(path.FullPath, selectedPath.PathName);
            SelectPath.Content = selectedPath.FullPath;
            //SelectPath.Background = (selectedPath.FullPath != "" && !Directory.Exists(selectedPath.FullPath)) || FavoritePaths.Instance.Favorites.Any(x => x.FullPath == selectedPath.FullPath) ? Colors.INVALID_RED : Brushes.White;
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }
}
