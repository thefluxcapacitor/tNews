namespace TorrentNews.Dal
{
    using MongoDB.Bson;
    using MongoDB.Driver;
    using MongoDB.Driver.Builders;

    using TorrentNews.Domain;

    public class MoviesRepository : BaseRepository
    {
        private const string CollectionName = "movies";

        private readonly MongoCollection<Movie> moviesCollection;

        public MoviesRepository()
        {
            var mdb = this.GetDatabase();

            if (!mdb.CollectionExists(CollectionName))
            {
                mdb.CreateCollection(CollectionName);
            }

            this.moviesCollection = mdb.GetCollection<Movie>(CollectionName);
        }

        public void Save(Movie entity)
        {
            this.moviesCollection.Save(entity);
        }

        public Movie Find(BsonValue id)
        {
            return this.moviesCollection.FindOneById(id);
        }

        public MongoCursor<Movie> GetMoviesToUpdate()
        {
            var query = Query<Movie>.LT(m => m.ImdbVotes, 1000);
            return this.moviesCollection.Find(query);
        }
    }
}