namespace TorrentNews.Dal
{
    using System;
    using System.Configuration;

    using MongoDB.Driver;

    using TorrentNews.Domain;

    public class TorrentsRepository
    {
        private readonly MongoCollection<Torrent> torrentsCollection;

        public TorrentsRepository()
        {
            var url = new MongoUrl(ConfigurationManager.AppSettings["MONGOLAB_URI"]);
            var mc = new MongoClient(url);
            var ms = mc.GetServer();
            
            var mdb = ms.GetDatabase("torrentnews");
            
            if (!mdb.CollectionExists("torrents"))
            {
                mdb.CreateCollection("torrents");
            }

            this.torrentsCollection = mdb.GetCollection<Torrent>("torrents");
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