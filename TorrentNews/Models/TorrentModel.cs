namespace TorrentNews.Models
{
    using System;

    public class TorrentModel
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string DetailsUrl { get; set; }

        public string Size { get; set; }

        public string Files { get; set; }

        public int Seed { get; set; }

        public int Leech { get; set; }

        public int CommentsCount { get; set; }

        public DateTime AddedOn { get; set; }

        public string ImdbId { get; set; }

        public bool ImdbAward { get; set; }

        public bool MetacriticAward { get; set; }

        public bool PopularityAward { get; set; }

        public bool SuperPopularityAward { get; set; }

        public int Score { get; set; }

        public string MovieTitle { get; set; }

        public string Year { get; set; }

        public string Duration { get; set; }

        public string Plot { get; set; }

        public string[] Directors { get; set; }

        public string Genres { get; set; }

        public string[] Cast { get; set; }

        public string Poster { get; set; }

        public string ContentRating { get; set; }

        public int ImdbRating { get; set; }

        public int ImdbVotes { get; set; }

        public int McMetascore { get; set; }

        public int McCriticsCount { get; set; }
    }
}