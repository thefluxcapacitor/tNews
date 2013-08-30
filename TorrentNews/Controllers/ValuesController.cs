namespace TorrentNews.Controllers
{
    using System.Linq;
    using System.Web.Http;

    using MongoDB.Bson;

    using TorrentNews.Dal;
    using TorrentNews.Scraping;

    public class ValuesController : ApiController
    {
        public string Get()
        {
            var torrentsRepo = new TorrentsRepository();

            torrentsRepo.RemoveAll();

            var kaHelper = new KassScraper();
            var torrents = kaHelper.GetTorrentsFromSearchResult(20, 1);
            foreach (var t in torrents)
            {
                torrentsRepo.Insert(t);
            }

            var result = torrentsRepo.FindAll().ToList();
            return result.ToJson();
        }
    }
}