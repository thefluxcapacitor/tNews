namespace TorrentNews.Models
{
    using TorrentNews.Domain;

    public class RelatedTorrentModel
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Age { get; set; }

        public string DetailsUrl { get; set; }

        public int Seed { get; set; }

        public string Size { get; set; }

        public int Score { get; set; }

        public ReleaseSource ReleaseSource { get; set; }
    }
}