namespace TorrentNews.Scraping
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;

    using CsQuery;

    public class YoutubeScraper
    {
        public string GetTrailersSearchUrl(string searchString)
        {
            return string.Format("http://www.youtube.com/results?search_query={0}", searchString);
        }

        public IEnumerable<Tuple<string, string>> GetTrailersUrl(string searchString)
        {
            var client = new HttpClient();
            var response = client.GetAsync(this.GetTrailersSearchUrl(searchString)).Result;
            
            CQ d = response.Content.ReadAsStringAsync().Result;
            var result = d[".yt-uix-sessionlink.yt-uix-tile-link"].First().Map( // remove .First if more than one trailer is needed
                item =>
                    {
                        var itemCq = item.Cq();
                        return Tuple.Create(
                            itemCq.Attr("data-context-item-title"), 
                            itemCq.Attr("data-context-item-id"));
                    });

            return result;
        }
    }
}