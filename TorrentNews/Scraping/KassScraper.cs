namespace TorrentNews.Scraping
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;

    using CsQuery;

    using TorrentNews.Controllers;
    using TorrentNews.Domain;

    public class KassScraper
    {
        private const int MaxDaysToKeepTorrents = 7 * 4;

        public IEnumerable<Torrent> GetLatestTorrents(int minSeeds, OperationInfo operationInfo)
        {
            var client = new HttpClient(new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            });

            var nextPage = 1;
            var now = DateTime.UtcNow;

            while (nextPage > 0)
            {
                var response = client.GetAsync(string.Format(
                    "http://kickass.to/usearch/category%3Amovies%20seeds%3A{0}/{1}/?field=time_add&sorder=desc",
                    minSeeds,
                    nextPage)).Result;

                operationInfo.StatusInfo = string.Format("Scraping torrents page #{0}", nextPage);

                CQ document = response.Content.ReadAsStringAsync().Result;
                var rows = document[".data tr"];


                foreach (var row in rows)
                {
                    var rowCq = row.Cq();
                    if (rowCq.HasClass("firstr"))
                    {
                        continue;
                    }

                    var torrent = this.CreateTorrentFromRow(rowCq, now);
                    if (now.Date.Subtract(torrent.AddedOn).TotalDays > MaxDaysToKeepTorrents)
                    {
                        nextPage = -1;
                        break;
                    }

                    yield return torrent;
                }

                if (nextPage > 0)
                {
                    nextPage++;
                }
            }
        }

        private Torrent CreateTorrentFromRow(CQ rowCq, DateTime now)
        {
            var result = new Torrent();

            var torrentLink = rowCq.Find("div.torrentname > a.normalgrey");
            result.Title = torrentLink.Text();
            result.DetailsUrl = torrentLink.Attr("href");

            result.Id = result.DetailsUrl.GetHashCode();

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