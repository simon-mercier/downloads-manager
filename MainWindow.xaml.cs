using Syroot.Windows.IO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Linq;
using System.IO;
using System.ComponentModel;
using System.Diagnostics;

namespace DownloadsManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
		private ListViewDragDropManager<Path> dragMgr;
		private readonly Router router;

		public MainWindow()
		{
			InitializeComponent();
			this.Loaded += OnLoaded;
			this.Focusable = true;
			router = new Router(new Path(new KnownFolder(KnownFolderType.Downloads).Path, "Downloads"));
			router.newDownloadEvent += OnNewDownload;
			FavoritePaths.Instance.FavoriteAddedEvent += FavoriteAdded;
			this.IsEnabled = false;
			this.Hide();
			CreateInputgestures();

		}

		private static void CreateInputgestures()
		{
			Command.OtherPath.InputGestures.Add(new KeyGesture(Key.O, ModifierKeys.Control));
			Command.MovePath.InputGestures.Add(new KeyGesture(Key.Return));
			Command.Cancel.InputGestures.Add(new KeyGesture(Key.Escape));
		}

		private void FavoriteAdded(object sender, FavoriteAddedEventArgs e)
		{
			this.Dispatcher.Invoke(() =>
			{
				if (this.listViewFavorites.ItemsSource == null)
					return;
				(this.listViewFavorites.ItemsSource as ObservableCollection<Path>)?.Add(e.AddedFavorite);
				ChangeListView();
			});
		}

		private void OnNewDownload(object sender, NewDownloadEventArgs e)
		{
			this.Dispatcher.Invoke(() =>
			{
				this.Topmost = true;
				Keyboard.Focus(this);
				this.Show();
				this.IsEnabled = true;
				this.DownloadName.Content = $"Move \"{e.NewPath.PathName}\" to ";
			});
		}


		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			Keyboard.Focus(this);
			InitializeListView();
		}

		private void InitializeListView()
		{
			var favorites = new ObservableCollection<Path>(FavoritePaths.Instance.Favorites);

			this.listViewFavorites.ItemsSource = favorites ?? new ObservableCollection<Path>();

			this.dragMgr = new ListViewDragDropManager<Path>(this.listViewFavorites)
			{
				DragAdornerOpacity = 0.7,
				ShowDragAdorner = true,
				ListView = this.listViewFavorites
			};
			this.dragMgr.ProcessDrop += dragMgr_ProcessDrop;			
		}
		private void ChangeListView()
		{
			var favorites = new ObservableCollection<Path>(FavoritePaths.Instance.Favorites);
			this.listViewFavorites.ItemsSource = favorites ?? new ObservableCollection<Path>();
		}

		#region dragMgr_ProcessDrop

		// Performs custom drop logic for the top ListView.
		void dragMgr_ProcessDrop(object sender, ProcessDropEventArgs<Path> e)
		{
			// This shows how to customize the behavior of a drop.
			// Here we perform a swap, instead of just moving the dropped item.

			int higherIdx = Math.Max(e.OldIndex, e.NewIndex);
			int lowerIdx = Math.Min(e.OldIndex, e.NewIndex);

			if (lowerIdx < 0)
			{
				// The item came from the lower ListView
				// so just insert it.
				e.ItemsSource.Insert(higherIdx, e.DataItem);
			}
			else
			{
				// null values will cause an error when calling Move.
				// It looks like a bug in ObservableCollection to me.
				if (e.ItemsSource[lowerIdx] == null ||
					e.ItemsSource[higherIdx] == null)
					return;

				// The item came from the ListView into which
				// it was dropped, so swap it with the item
				// at the target index.
				e.ItemsSource.Move(lowerIdx, higherIdx);
				e.ItemsSource.Move(higherIdx - 1, lowerIdx);
			}

			// Set this to 'Move' so that the OnListViewDrop knows to 
			// remove the item from the other ListView.
			e.Effects = DragDropEffects.Move;

			var obsCollection = (IEnumerable<Path>)this.listViewFavorites.ItemsSource;
			FavoritePaths.Instance.SetFavorites(new List<Path>(obsCollection));
		}

		#endregion

		private void OnDoubleClickFavorite(object sender, MouseButtonEventArgs e)
		{
			MovePath(sender, new RoutedEventArgs());
		}


		private void OnListViewClick(object sender, MouseButtonEventArgs e)
		{
			_ = Utils.FindParent<ListView>((ListViewItem)sender) == this.listViewFavorites ?
				this.listViewOther.SelectedItem = null : 
				this.listViewFavorites.SelectedItem = null;

			router.SelectedDirectory = (Path)((ListViewItem)sender).Content;
		}
		

		private void CancelExecution(object sender, RoutedEventArgs e)
		{
			CloseWindow();
		}

		private void CloseWindow()
		{
			router.NewestDownload = null;
			this.Hide();
			IsEnabled = false;
		}

		private void MovePath(object sender, RoutedEventArgs e)
		{
			if(router.SelectedDirectory == null)
			{
				MessageBox.Show("Please chose a directory.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}


			if (!Directory.Exists(router.SelectedDirectory.FullPath))
			{
				RemoveFavorite(router.SelectedDirectory);
				MessageBox.Show("This directory no longer exists. Please choose another one.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}

			router.RouteToSelectedDirectory();

			CloseWindow();

		}

		private void OtherPath(object sender, RoutedEventArgs e)
		{
			var selectedpath = Utils.GetPathFromExplorer();
			if(selectedpath == Path.Empty)
				return;

			router.SelectedDirectory = selectedpath;
			this.listViewFavorites.SelectedItem = null;
			var otherPath = new Path[1] { router.SelectedDirectory };
			this.listViewOther.ItemsSource = new ObservableCollection<Path>(otherPath);
			this.listViewOther.SelectedItem = listViewOther.Items[0];
		}

		private void AddFavorite(object sender, RoutedEventArgs e)
		{
			IsEnabled = false;
			var addFavoriteWindow = new AddFavorite();
			addFavoriteWindow.ShowDialog();
			IsEnabled = true;
			ChangeListView();
		}

		private void EditFavorite(object sender, RoutedEventArgs e)
		{
			IsEnabled = false;
			var pathToEdit = (Path)((ListViewItem)e.OriginalSource).Content;
			var addFavoriteWindow = new EditFavorite(pathToEdit.PathName, pathToEdit.FullPath);
			addFavoriteWindow.ShowDialog();
			IsEnabled = true;
			ChangeListView();
		}

		private void RemoveFavorite(object sender, RoutedEventArgs e)
		{
			var pathToRemove = (Path)((ListViewItem)e.OriginalSource).Content;
			RemoveFavorite(pathToRemove);
		}

		private void RemoveFavorite(Path pathToRemove)
		{
			FavoritePaths.Instance.RemoveFavorite(pathToRemove);
			ChangeListView();
		}

		private void Border_MouseDown(object sender, MouseButtonEventArgs e)
		{
			this.DragMove();
		}
		protected override void OnClosing(CancelEventArgs e)
		{
			e.Cancel = true;
			CloseWindow();
		}
	}
}
