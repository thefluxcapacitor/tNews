namespace TorrentNews.Dal
{
    using System;
    using System.Collections.Generic;

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

        public MongoCursor<Torrent> GetPageMostRecentTorrents(int page, string[] sortBy, int minScore)
        {
            var sortOrder = GetSortOrder(sortBy);

            var filter = Query.And(Query<Torrent>.NE(t => t.ImdbId, "NA"), Query<Torrent>.EQ(t => t.Latest, true));

            if (minScore > 0)
            {
                filter = Query.And(filter, Query<Torrent>.GTE(t => t.Score, minScore));
            }

            return this.torrentsCollection
                .Find(filter)
                .SetSortOrder(sortOrder)
                .SetSkip(Constants.PageSize * (page - 1))
                .SetLimit(Constants.PageSize);
        }

        private static SortByBuilder GetSortOrder(IEnumerable<string> sortBy)
        {
            var builder = new SortByBuilder();
            foreach (var field in sortBy)
            {
                if (field[0] == '-')
                {
                    builder.Descending(field.Substring(1));
                }
                else
                {
                    builder.Ascending(field);
                }
            }

            return builder;
        }

        public long RemoveOldTorrents()
        {
            var fourWeeksAgo = DateTime.UtcNow.Subtract(TimeSpan.FromDays(7 * 4 + 1));
            var query = Query<Torrent>.LT(t => t.AddedOn, fourWeeksAgo);
            var result = this.torrentsCollection.Remove(query);
            return result.DocumentsAffected;
        }

        public MongoCursor<Torrent> GetAllSortedByImdbIdAndAddedOn()
        {
            return this.torrentsCollection
                .FindAll()
                .SetSortOrder(SortBy<Torrent>.Ascending(t => t.ImdbId).Ascending(t => t.AddedOn));
        }

        public MongoCursor<Torrent> GetRssItems(string[] sortBy)
        {
            var sortOrder = GetSortOrder(sortBy);
            return this.torrentsCollection
                .Find(Query<Torrent>.NE(t => t.ImdbId, "NA"))
                .SetSortOrder(sortOrder);
        }

        public MongoCursor<Torrent> FindByImdbId(string imdbId, int maxTorrents)
        {
            var result = this.torrentsCollection
                .Find(Query<Torrent>.EQ(t => t.ImdbId, imdbId))
                .SetSortOrder(SortBy<Torrent>.Descending(t => t.AddedOn));
            if (maxTorrents > 0)
            {
                result = result.SetLimit(maxTorrents);
            }

            return result;
        }
    }
}