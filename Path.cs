namespace DownloadsManager
{
    public class Path
    {
        public static Path Empty = new Path("");
        public string FullPath { get; set; }
        public string PathName { get; set; }

        public Path(string path, string name = "")
        {
            FullPath = path;
            PathName = name;
        }

        public override bool Equals(object obj)
        {
            return obj is Path other && FullPath == other.FullPath && PathName == other.PathName;
        }
    }
}
