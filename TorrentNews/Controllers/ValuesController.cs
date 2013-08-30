namespace TorrentNews.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;

    using CsQuery;

    using MongoDB.Bson;
    using MongoDB.Bson.Serialization;
    using MongoDB.Bson.Serialization.Attributes;
    using MongoDB.Bson.Serialization.IdGenerators;
    using MongoDB.Driver;

    public class ValuesController : ApiController
    {
        public string Get()
        {
            if (!BsonClassMap.IsClassMapRegistered(typeof(Torrent)))
            {
                BsonClassMap.RegisterClassMap<Torrent>();
            }

            const int MinSeeds = 20;
            var page = 1;
            var client = new HttpClient(new HttpClientHandler
                {
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
                });

            var response = client.GetAsync(string.Format(
                "http://kickass.to/usearch/category%3Amovies%20seeds%3A{0}/{1}/?field=time_add&sorder=desc", 
                MinSeeds, 
                page)).Result;
            CQ document = response.Content.ReadAsStringAsync().Result;
            var rows = document[".data tr"];

            MongoClient mc = new MongoClient("mongodb://127.0.0.1:27017");
            var ms = mc.GetServer();
            var mdb = ms.GetDatabase("torrentnews");
            if (!mdb.CollectionExists("torrents"))
            {
                mdb.CreateCollection("torrents");
            }

            var torrents = mdb.GetCollection<Torrent>("torrents");
            var x = torrents.FindAll().ToList();
            foreach (var row in rows)
            {
                if (row.Cq().HasClass("firstr"))
                {
                    continue;
                }

                var mres = torrents.Insert(new Torrent
                                               {
                                                   Title = row.Cq().Find("div.torrentname > a.normalgrey").Text()
                                               });
            }

            return rows.Text();
        }
    }

    public class Torrent
    {
        [BsonId(IdGenerator = typeof(ObjectIdGenerator))]
        public ObjectId Id { get; set; }

        public string Title { get; set; }
    }
}