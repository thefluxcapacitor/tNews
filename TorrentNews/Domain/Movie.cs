namespace TorrentNews.Domain
{
    using System;

    using MongoDB.Bson.Serialization.Attributes;

    public class Movie
    {
        [BsonId]
        public string Id { get; set; }

        public string Title { get; set; }

        public string Year { get; set; }

        public string Duration { get; set; }

        public string Plot { get; set; }

        public string[] Directors { get; set; }

        public string[] Genres { get; set; }

        public string[] Cast { get; set; }

        public string Poster { get; set; }

        public string Country { get; set; }

        public string Language { get; set; }

        public string ContentRating { get; set; }

        public int ImdbRating { get; set; }

        public int ImdbVotes { get; set; }

        public int McMetascore { get; set; }

        public int McCriticsCount { get; set; }

        public DateTime? FirstUpdatedOn { get; set; }
    }
}