namespace TorrentNews.Dal
{
    using System.Linq;

    using MongoDB.Bson;
    using MongoDB.Driver;
    using MongoDB.Driver.Builders;

    using TorrentNews.Domain;

    public class UsersRepository : BaseRepository
    {
        private const string CollectionName = "users";

        private readonly MongoCollection<User> collection;

        public UsersRepository()
        {
            var mdb = this.GetDatabase();

            if (!mdb.CollectionExists(CollectionName))
            {
                mdb.CreateCollection(CollectionName);
            }

            this.collection = mdb.GetCollection<User>(CollectionName);
        }

        public void Save(User entity)
        {
            this.collection.Save(entity);
        }

        public User Find(BsonValue id)
        {
            return this.collection.FindOneById(id);
        }

        public User FindByUsername(string username)
        {
            return this.collection.Find(Query<User>.EQ(u => u.UsernameLower, username.ToLowerInvariant())).FirstOrDefault();
        }
    }
}