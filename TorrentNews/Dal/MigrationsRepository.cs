namespace TorrentNews.Dal
{
    using System.Collections.Generic;

    using MongoDB.Driver;

    using TorrentNews.Domain;

    public class MigrationsRepository : BaseRepository
    {
        private const string CollectionName = "migrations";

        private readonly MongoCollection<Migration> collection;

        public MigrationsRepository()
        {
            var mdb = this.GetDatabase();

            if (!mdb.CollectionExists(CollectionName))
            {
                mdb.CreateCollection(CollectionName);
            }

            this.collection = mdb.GetCollection<Migration>(CollectionName);
        }

        public IEnumerable<Migration> GetAll()
        {
            return this.collection.FindAll();
        }

        public void Save(Migration entity)
        {
            this.collection.Save(entity);
        }
    }
}