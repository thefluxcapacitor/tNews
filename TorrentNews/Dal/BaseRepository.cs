namespace TorrentNews.Dal
{
    using System.Configuration;

    using MongoDB.Driver;

    public class BaseRepository
    {
        protected MongoDatabase GetDatabase()
        {
            var url = new MongoUrl(/*ConfigurationManager.AppSettings["MONGOHQ_URL"] ??*/ ConfigurationManager.AppSettings["MONGOLAB_URI"] ?? "mongodb://127.0.0.1:27017/torrentnews");
            var mc = new MongoClient(url);
            var ms = mc.GetServer();
            var mdb = ms.GetDatabase(url.DatabaseName);
            return mdb;
        }
    }
}