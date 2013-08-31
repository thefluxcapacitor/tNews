namespace TorrentNews.Dal
{
    using System;
    using System.Configuration;

    using MongoDB.Driver;

    using TorrentNews.Domain;

    public class TorrentsRepository
    {
        private const string CollectionName = "torrents";

        private readonly MongoCollection<Torrent> torrentsCollection;

        public TorrentsRepository()
        {
            var url = new MongoUrl(ConfigurationManager.AppSettings["MONGOLAB_URI"]);
            var mc = new MongoClient(url);
            var ms = mc.GetServer();
            var mdb = ms.GetDatabase(url.DatabaseName);

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

        public void RemoveAll()
        {
            this.torrentsCollection.RemoveAll();
        }

        public MongoCursor<Torrent> FindAll()
        {
            return this.torrentsCollection.FindAll();
        }
    }
}