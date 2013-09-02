namespace TorrentNews.Scraping
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Net;
    using System.Net.Http;
    using System.Text;

    using CsQuery;

    using TorrentNews.Dal;
    using TorrentNews.Domain;

    public class KassScraper
    {
        private const int MaxDaysToKeepTorrents = 7 * 4;

        private const int MinSeeds = 20;

        private readonly TorrentsRepository repo;

        private readonly int maxPages;

        public KassScraper(TorrentsRepository repo, int maxPages)
        {
            this.repo = repo;
            this.maxPages = maxPages;
        }

        public IEnumerable<Torrent> GetLatestTorrents(OperationInfo operationInfo)
        {
            var torrentsScraped = 0;

            var client = new HttpClient(new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            });

            var nextPage = 1;
            var now = DateTime.UtcNow;

            while (nextPage > 0 && (this.maxPages < 0 || nextPage < this.maxPages))
            {
                var response = client.GetAsync(string.Format(
                    "http://kickass.to/usearch/category%3Amovies%20seeds%3A{0}/{1}/?field=time_add&sorder=desc",
                    MinSeeds,
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

                    var torrent = this.GetTorrentFromRow(rowCq, now);
                    if (now.Date.Subtract(torrent.AddedOn).TotalDays > MaxDaysToKeepTorrents)
                    {
                        nextPage = -1;
                        break;
                    }

                    if (string.IsNullOrEmpty(torrent.ImdbUrl))
                    {
                        torrentsScraped++;
                        operationInfo.ExtraData["scraped"] = torrentsScraped.ToString(CultureInfo.InvariantCulture);

                        torrent.ImdbUrl = this.GetImdbUrl(torrent.DetailsUrl, client);
                    }

                    yield return torrent;
                }

                if (nextPage > 0)
                {
                    nextPage++;
                }
            }
        }

        private string GetImdbUrl(string torrentDetailsUrl, HttpClient client)
        {
            const string ImdbPrefix = "http://www.imdb.com/title/";

            if (torrentDetailsUrl[0] != '/')
            {
                torrentDetailsUrl = "/" + torrentDetailsUrl;
            }

            var response = client.GetAsync(string.Format("http://kickass.to{0}", torrentDetailsUrl)).Result;
            var document = response.Content.ReadAsStringAsync().Result;

            var pos = document.IndexOf(ImdbPrefix, StringComparison.OrdinalIgnoreCase);
            if (pos < 0)
            {
                return "NA";
            }

            var result = new StringBuilder();
            
            pos = pos + ImdbPrefix.Length;
            var docLength = document.Length;
            var currChar = document[pos];
            while (char.IsLetterOrDigit(currChar) && pos < docLength)
            {
                result.Append(currChar);
                
                pos++;
                currChar = document[pos];
            }

            if (pos >= docLength)
            {
                return "ERROR";
            }

            return result.ToString();
        }

        private Torrent GetTorrentFromRow(CQ rowCq, DateTime now)
        {
            var torrentLink = rowCq.Find("div.torrentname > a.normalgrey");
            var torrentDetailsUrl = torrentLink.Attr("href");
            var torrentId = torrentDetailsUrl.GetHashCode();

            var torrent = this.repo.Find(torrentId);
            if (torrent == null)
            {
                torrent = new Torrent();
                torrent.Id = torrentId;
                torrent.DetailsUrl = torrentDetailsUrl;
            }

            torrent.Title = torrentLink.Text();

            var rowCells = rowCq.Find("td");
            torrent.Size = rowCells[1].Cq().Text();
            torrent.Files = rowCells[2].Cq().Text();
            torrent.SetAddedOnFromAge(now, rowCells[3].InnerText);
            torrent.Seed = rowCells[4].Cq().Text();
            torrent.Leech = rowCells[5].Cq().Text();

            return torrent;
        }
    }
}