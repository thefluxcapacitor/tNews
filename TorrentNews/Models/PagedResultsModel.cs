namespace TorrentNews.Models
{
    public class PagedResultsModel
    {
        public string NextPageUrl { get; set; }

        public string PreviousPageUrl { get; set; }

        public bool ShowGoToBookmark { get; set; }

        public bool BookmarkSet { get; set; }
    }
}