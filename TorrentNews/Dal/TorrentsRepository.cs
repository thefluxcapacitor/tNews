namespace TorrentNews.Dal
{
    using System;

    using MongoDB.Bson;
    using MongoDB.Driver;
    using MongoDB.Driver.Builders;

    using TorrentNews.Domain;

    public class TorrentsRepository : BaseRepository
    {
        private const string CollectionName = "torrents";

        private readonly MongoCollection<Torrent> torrentsCollection;

        public TorrentsRepository()
        {
            var mdb = this.GetDatabase();

            if (!mdb.CollectionExists(CollectionName))
            {
                mdb.CreateCollection(CollectionName);
            }

            this.torrentsCollection = mdb.GetCollection<Torrent>(CollectionName);
        }

        public void Save(Torrent entity)
        {
            this.torrentsCollection.Save(entity);
        }

        public Torrent Find(BsonValue id)
        {
            return this.torrentsCollection.FindOneById(id);
        }

        public long RemoveOldTorrents()
        {
            var fourWeeksAgo = DateTime.UtcNow.Subtract(TimeSpan.FromDays(7 * 4 + 1));
            var query = Query<Torrent>.LT(t => t.AddedOn, fourWeeksAgo);
            var result = this.torrentsCollection.Remove(query);
            return result.DocumentsAffected;
        }
    }
}