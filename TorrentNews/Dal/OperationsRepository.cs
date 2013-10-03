namespace TorrentNews.Dal
{
    using System.Collections.Generic;

    using MongoDB.Bson;
    using MongoDB.Driver;

    using TorrentNews.Domain;

    public class OperationsRepository : BaseRepository
    {
        private const string CollectionName = "operations";

        private readonly MongoCollection<Operation> collection;

        public OperationsRepository()
        {
            var mdb = this.GetDatabase();

            if (!mdb.CollectionExists(CollectionName))
            {
                mdb.CreateCollection(CollectionName);
            }

            this.collection = mdb.GetCollection<Operation>(CollectionName);
        }

        public void Save(Operation entity)
        {
            this.collection.Save(entity);
        }

        public Operation Find(BsonValue id)
        {
            return this.collection.FindOneById(id);
        }

        public IEnumerable<Operation> FindAll()
        {
            return this.collection.FindAll();
        }
    }
}