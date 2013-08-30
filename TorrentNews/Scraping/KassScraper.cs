namespace TorrentNews.Scraping
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;

    using CsQuery;

    using TorrentNews.Domain;

    public class KassScraper
    {
        public IEnumerable<Torrent> GetTorrentsFromSearchResult(int minSeeds, int page)
        {
            var client = new HttpClient(new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            });

            var response = client.GetAsync(string.Format(
                "http://kickass.to/usearch/category%3Amovies%20seeds%3A{0}/{1}/?field=time_add&sorder=desc",
                minSeeds,
                page)).Result;
            
            CQ document = response.Content.ReadAsStringAsync().Result;
            var rows = document[".data tr"];

            var now = DateTime.UtcNow;

            foreach (var row in rows)
            {
                var rowCq = row.Cq();
                if (rowCq.HasClass("firstr"))
                {
                    continue;
                }

                var torrentDoc = this.CreateTorrentFromRow(rowCq, now);

                yield return torrentDoc;
            }
        }

        private Torrent CreateTorrentFromRow(CQ rowCq, DateTime now)
        {
            var result = new Torrent();

            var torrentLink = rowCq.Find("div.torrentname > a.normalgrey");
            result.Title = torrentLink.Text();
            result.DetailsUrl = torrentLink.Attr("href");

            var rowCells = rowCq.Find("td");
            result.Size = rowCells[1].Cq().Text();
            result.Files = rowCells[2].Cq().Text();
            result.SetAddedOnFromAge(now, rowCells[3].InnerText);
            result.Seed = rowCells[4].Cq().Text();
            result.Leech = rowCells[5].Cq().Text();

            return result;
        }
    }
}