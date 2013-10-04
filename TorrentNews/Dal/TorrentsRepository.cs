namespace TorrentNews.Dal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

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

        public MongoCollection<Torrent> GetCollection()
        {
            return this.torrentsCollection;
        }

        public void Save(Torrent entity)
        {
            this.torrentsCollection.Save(entity);
        }

        public Torrent Find(BsonValue id)
        {
            return this.torrentsCollection.FindOneById(id);
        }

        public IEnumerable<Torrent> GetPageStarredTorrents(int page, IList<StarredMovie> starred)
        {
            var starredPage = starred
                .OrderByDescending(s => s.StarredOn)
                .Skip(Constants.PageSize * (page - 1))
                .Take(Constants.PageSize)
                .Select(s => s.ImdbId)
                .ToList();

            var filter = Query.And(
                Query<Torrent>.In(t => t.ImdbId, starredPage), 
                Query<Torrent>.EQ(t => t.Latest, true));

            var result = this.torrentsCollection.Find(filter);

            return result.OrderBy(t => t.ImdbId, new StarredOrderComparer(starredPage));
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

        public MongoCursor<Torrent> GetTorrentsNewerThan(DateTime date)
        {
            var sortOrder = SortBy<Torrent>.Descending(t => t.AddedOn).Descending(t => t.Score).Ascending(t => t.Id);
            
            var filter = Query.And(
                Query<Torrent>.GTE(t => t.AddedOn, date),
                Query<Torrent>.NE(t => t.ImdbId, "NA"), 
                Query<Torrent>.EQ(t => t.Latest, true));

            return this.torrentsCollection
                .Find(filter)
                .SetSortOrder(sortOrder);
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

        public void UpdateLatestToFalse()
        {
            this.torrentsCollection.Update(
                Query<Torrent>.NE(t => t.Latest, false), 
                Update<Torrent>.Set(t => t.Latest, false));
        }

        public MongoCursor<Torrent> GetAllSortedByImdbIdAndAddedOn()
        {
            return this.torrentsCollection
                .Find(Query<Torrent>.NE(t => t.ImdbId, "NA"))
                .SetSortOrder(SortBy<Torrent>.Ascending(t => t.ImdbId).Ascending(t => t.AddedOn).Ascending(t => t.Id));
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

        public Torrent FindBookmarkedTorrent(int id)
        {
            var filter = Query.And(
                Query<Torrent>.EQ(t => t.Id, id),
                Query<Torrent>.NE(t => t.ImdbId, "NA"), 
                Query<Torrent>.EQ(t => t.Latest, true));

            return this.torrentsCollection.FindOne(filter);
        }

        private class StarredOrderComparer : IComparer<string>
        {
            private readonly IList<string> starred;

            public StarredOrderComparer(IList<string> starred)
            {
                this.starred = starred;
            }

            public int Compare(string x, string y)
            {
                if (this.starred.IndexOf(x) > this.starred.IndexOf(y))
                {
                    return 1;
                }
                
                if (this.starred.IndexOf(x) < this.starred.IndexOf(y))
                {
                    return -1;
                }
                
                return 0;
            }
        }

        public IEnumerable<Torrent> Search(string searchTerm)
        {
            var q = Query.Or(Query<Torrent>.Matches(t => t.Title, searchTerm), 
                Query<Torrent>.Matches(t => t.ImdbId, searchTerm));

            return this.torrentsCollection.Find(q);
        }
    }
}