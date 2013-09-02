namespace TorrentNews.Scraping
{
    using System;
    using System.Linq;
    using System.Net.Http;

    using CsQuery;

    using TorrentNews.Domain;

    public class ImdbScraper
    {
        public void UpdateMovieDetails(Movie movie)
        {
            //movie = new Movie { Id = "tt0088763" }; //back to the future
            movie = new Movie { Id = "tt0133093" }; //the matrix

            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en"); 
            
            var response = client.GetAsync(string.Format(
                "http://www.imdb.com/title/{0}/",
                movie.Id)).Result;
            CQ document = response.Content.ReadAsStringAsync().Result;

            if (string.IsNullOrEmpty(movie.Title))
            {
                this.UpdateMovieBasicDetails(movie, document);
            }
        }

        private void UpdateMovieBasicDetails(Movie movie, CQ d)
        {
            movie.Title = d["h1.header > span.itemprop[itemprop=name]"].First().Text();
            movie.Year = d["h1.header a[href^=\"/year/\"]"].First().Text();
            movie.Plot = d["p[itemprop=description]"].First().Text();
            movie.Directors = d["div[itemprop=director] span.itemprop[itemprop=name]"]
                .Map(item => item.Cq().Text()).ToArray();
            movie.Genres = d["span.itemprop[itemprop=genre]"]
                .Map(item => item.Cq().Text()).ToArray();
            movie.Cast = d["div[itemprop=actors] span.itemprop[itemprop=name]"]
                .Map(item => item.Cq().Text()).ToArray();
            
            foreach (var image in d["img[itemprop=image]"])
            {
                var aux = image.Cq();
                if (aux.Attr("title").ToLowerInvariant().Contains("poster"))
                {
                    movie.Poster = aux.Attr("src");
                    break;
                }
            }

            foreach (var h4 in d["h4"])
            {
                if (h4.InnerText.Equals("Runtime:", StringComparison.OrdinalIgnoreCase))
                {
                    var aux = h4.ParentNode.Cq().Find("time[itemprop=duration]").First().Text();
                    movie.Duration = aux.Substring(0, aux.IndexOf(" ", StringComparison.OrdinalIgnoreCase));
                    break;
                }
            }
        }
    }
}