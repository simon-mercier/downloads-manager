using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace DownloadsManager
{
    public static class Command
    {
        public static readonly RoutedUICommand Edit = new RoutedUICommand("Edit", "Edit", typeof(Command));
        public static readonly RoutedUICommand Remove = new RoutedUICommand("Remove", "Remove", typeof(Command));
        public static readonly RoutedUICommand Add = new RoutedUICommand("Add", "Add", typeof(Command));
        public static readonly RoutedUICommand OtherPath = new RoutedUICommand("OtherPath", "OtherPath", typeof(Command));
        public static readonly RoutedUICommand MovePath = new RoutedUICommand("MovePath", "MovePath", typeof(Command));
        public static readonly RoutedUICommand Cancel = new RoutedUICommand("Cancel", "Cancel", typeof(Command));
    }
}
