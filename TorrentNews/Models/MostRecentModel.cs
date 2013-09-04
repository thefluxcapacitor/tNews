namespace TorrentNews.Models
{
    using System.Collections.Generic;

    public class TorrentsListModel : PagedResultsModel
    {
        public TorrentsListModel()
        {
            this.Torrents = new List<TorrentModel>();
        }

        public IList<TorrentModel> Torrents { get; private set; }
    }
}