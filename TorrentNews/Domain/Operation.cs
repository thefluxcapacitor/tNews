namespace TorrentNews.Domain
{
    using System;

    using MongoDB.Bson.Serialization.Attributes;

    public class Operation
    {
        [BsonId]
        public string Id { get; set; }

        public OperationInfo Info { get; set; }

        public DateTime AddedOn { get; set; }
        
        public DateTime? LastUpdatedOn { get; set; }
    }
}