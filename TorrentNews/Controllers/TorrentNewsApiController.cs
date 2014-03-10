namespace TorrentNews.Controllers
{
    using System.Net;
    using System.Net.Http;
    using System.Web;
    using System.Web.Http;

    using CsQuery;

    using TorrentNews.Domain;

    public class TorrentNewsApiController : ApiController
    {
        public ReleaseSource GetBestReleaseSource(string title, int year)
        {
            HttpContext.Current.Response.AppendHeader("Access-Control-Allow-Origin", "http://www.imdb.com");

            var bestRelease = new ReleaseSource() { Quality = -1 };
            
            var client = new HttpClient(new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            });

            var url = string.Format(
                "http://kickass.to/usearch/{0}%20{1}%20category%3Amovies/?field=seeders&sorder=desc",
                HttpUtility.UrlEncode(title),
                year);

            var response = client.GetAsync(url).Result;

            CQ document = response.Content.ReadAsStringAsync().Result;
            var rows = document[".data tr"];

            foreach (var row in rows)
            {
                var rowCq = row.Cq();
                if (rowCq.HasClass("firstr"))
                {
                    continue;
                }

                var torrentLink = rowCq.Find("div.torrentname > a.normalgrey");
                var torrentTitle = torrentLink.Text();

                var releaseSource = TorrentHelper.GetReleaseSource(torrentTitle);
                if (releaseSource.Quality > bestRelease.Quality)
                {
                    bestRelease = releaseSource;
                }
            }

            return bestRelease;
        }
    }
}
