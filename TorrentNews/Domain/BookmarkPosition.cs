namespace TorrentNews.Domain
{
    using System;

    public class BookmarkPosition
    {
        public DateTime BookmarkedTorrentAddedOn { get; set; }

        public int BookmarkedTorrentId { get; set; }
    }
}