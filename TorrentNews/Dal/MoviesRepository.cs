namespace TorrentNews.Dal
{
    using System;
    using System.Collections.Generic;

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
            var twoWeeksAgo = DateTime.UtcNow.Subtract(TimeSpan.FromDays(7 * 2));
            var query = Query.And(
                Query<Movie>.LT(m => m.ImdbVotes, 1000),
                Query.Or(
                    Query<Movie>.GT(m => m.FirstUpdatedOn, twoWeeksAgo),
                    Query<Movie>.EQ(m => m.FirstUpdatedOn, null)));
            
            return this.moviesCollection.Find(query);
        }

        public IEnumerable<Movie> Search(string searchTerm)
        {
            var q = Query<Movie>.Matches(m => m.Title, searchTerm);
            
            return this.moviesCollection.Find(q);
        }

        public IEnumerable<Movie> GetUnwantedMovies()
        {
            return this.moviesCollection.Find(
                Query.Or(
                    Query<Movie>.EQ(m => m.Country, "India"),
                    Query<Movie>.EQ(m => m.Language, "Telugu"),
                    Query<Movie>.EQ(m => m.Language, "Hindi"),
                    Query<Movie>.EQ(m => m.Language, "Tamil"),
                    Query<Movie>.EQ(m => m.Language, "Panjabi")));
        }

        public void Remove(string id)
        {
            this.moviesCollection.Remove(Query<Movie>.EQ(m => m.Id, id));
        }
    }
}