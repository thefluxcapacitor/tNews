namespace TorrentNews.Models
{
    using System;

    public class TorrentRssModel
    {
        public string Title { get; set; }

        public int Score { get; set; }
        
        public string Plot { get; set; }

        public string Directors { get; set; }

        public string Genres { get; set; }

        public string Cast { get; set; }
        
        public string DetailsUrl { get; set; }

        public string Size { get; set; }
        
        public int Seed { get; set; }

        public string ContentRating { get; set; }

        public string ImdbId { get; set; }

        public string Age { get; set; }

        public bool HasImdbId()
        {
            return !string.IsNullOrEmpty(this.ImdbId) && this.ImdbId != "NA";
        }
    }
}