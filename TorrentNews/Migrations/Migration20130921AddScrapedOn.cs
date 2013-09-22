namespace TorrentNews.Migrations
{
    using System;
    using System.Threading;

    using MongoDB.Driver.Builders;

    using TorrentNews.Dal;
    using TorrentNews.Domain;

    public class Migration20130921AddScrapedOn : IMigration
    {
        public void Apply()
        {
            var repo = new TorrentsRepository();
            var collection = repo.GetCollection();
            var torrents = collection.FindAll()
                .SetSortOrder(SortBy<Torrent>.Ascending(t => t.AddedOn).Descending(t => t.Score).Ascending(t => t.Id));

            foreach (var torrent in torrents)
            {
                if (!torrent.ScrapedOn.HasValue)
                {
                    Thread.Sleep(1);
                    torrent.ScrapedOn = DateTime.UtcNow;
                    repo.Save(torrent);
                }
            }
        }

        public string GetName()
        {
            return this.GetType().Name;
        }
    }
}