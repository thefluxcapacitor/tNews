namespace TorrentNews.Scraping
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;

    using CsQuery;

    public class YoutubeScraper
    {
        public IEnumerable<Tuple<string, string>> GetTrailersUrl(string searchString)
        {
            var client = new HttpClient();
            var response = client.GetAsync(string.Format(
                "http://www.youtube.com/results?search_query={0}",
                searchString)).Result;
            
            CQ d = response.Content.ReadAsStringAsync().Result;
            var result = d[".context-data-item"].Map(
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