namespace TorrentNews.Domain
{
    using System;

    using MongoDB.Bson.Serialization.Attributes;

    public class Migration
    {
        [BsonId]
        public string Id { get; set; }

        public DateTime AppliedOn { get; set; }
    }
}