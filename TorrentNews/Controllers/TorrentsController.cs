namespace TorrentNews.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.ServiceModel.Syndication;
    using System.Text;
    using System.Web.Mvc;

    using TorrentNews.Dal;
    using TorrentNews.Domain;
    using TorrentNews.Models;

    public class TorrentsController : Controller
    {
        public ActionResult Index()
        {
            return this.RedirectToAction("MostRecent");
        }

        public ActionResult HighestScores(int page = 1)
        {
            return this.GetTorrentsActionResult(page, new string[] { "-Score", "ImdbId", "-AddedOn" }, "HighestScores");
        }

        public ActionResult MostRecent(int page = 1)
        {
            return this.GetTorrentsActionResult(page, new string[] { "-AddedOn", "Score" }, "MostRecent");
        }

        public ActionResult MostRecentFeed()
        {
            var torrentsRepo = new TorrentsRepository();
            var moviesRepo = new MoviesRepository();

            var moviesCache = new Dictionary<string, Movie>();

            var rssItems = new List<SyndicationItem>();

            var torrents = torrentsRepo.FindAll(new string[] { "-AddedOn", "Score" }).SetLimit(500);
            foreach (var t in torrents)
            {
                Movie m = null;
                if (!string.IsNullOrEmpty(t.ImdbId) && t.ImdbId != "NA")
                {
                    if (!moviesCache.TryGetValue(t.ImdbId, out m))
                    {
                        m = moviesRepo.Find(t.ImdbId);
                        moviesCache.Add(t.ImdbId, m);
                    }
                }

                var model = new TorrentRssModel();
                AutoMapper.Mapper.Map(t, model);
                if (m != null)
                {
                    AutoMapper.Mapper.Map(m, model);
                }

                string rssItemTitle;
                if (m != null)
                {
                    var metacritic = 
                        m.McMetascore == 0 ? 
                        string.Empty : 
                        string.Format("- MC {0}/{1} ", m.McMetascore, m.McCriticsCount);

                    rssItemTitle = string.Format(
                        "{0} ({1}) - IMDB {2}/{3} {4}- Seeds {5}",
                        m.Title, 
                        m.Year, 
                        Math.Round((decimal)m.ImdbRating / 10, 1), 
                        m.ImdbVotes, 
                        metacritic, 
                        t.Seed);
                }
                else
                {
                    rssItemTitle = string.Format("{0} - Seeds {1}", t.Title, t.Seed);
                }

                var rssItem = new SyndicationItem(
                    rssItemTitle, 
                    SyndicationContent.CreateHtmlContent(this.GetSyndicationItemHtml(model)),
                    new Uri("http://kickass.to" + t.DetailsUrl),
                    t.Id.ToString(CultureInfo.InvariantCulture),
                    DateTime.UtcNow);
                
                rssItems.Add(rssItem);
            }

            var feed = new SyndicationFeed(
                "Torrent news",
                "Latest torrents",
                new Uri(this.Request.Url.Scheme + "://" + this.Request.Url.Host),
                this.Request.Url.Scheme + "://" + this.Request.Url.Host + "/Torrents/MostRecentFeed",
                DateTime.UtcNow);

            feed.Items = rssItems;

            return new RssResult(feed);
        }

        private string GetSyndicationItemHtml(TorrentRssModel model)
        {
            var html = new StringBuilder();
            
            if (!string.IsNullOrEmpty(model.ImdbId) && model.ImdbId != "NA")
            {
                html.Append("<div style=\"margin-bottom: 10px;\">" + model.Plot + "</div>");
                
                html.Append("<div><b>Genres: </b>" + model.Genres + "</div>");
                html.Append("<div><b>Director: </b>" + model.Directors + "</div>");
                html.Append("<div><b>Cast: </b>" + model.Cast + "</div>");
                html.Append("<div><b>Content rating: </b>" + (model.ContentRating ?? "N/A") + "</div>");
                html.AppendFormat("<div><b>IMDB link: </b><a href=\"{0}\">{0}</a></div>", "http://www.imdb.com/title/" + model.ImdbId);
            }

            if (model.Score > 0)
            {
                html.Append("<div><b>Score: </b>" + model.Score.ToString(CultureInfo.InvariantCulture) + "</div>");
            }

            return html.ToString();
        }

        private ActionResult GetTorrentsActionResult(int page, string[] sortBy, string action)
        {
            var model = new TorrentsListModel();

            var torrentsRepo = new TorrentsRepository();
            var moviesRepo = new MoviesRepository();

            var moviesCache = new Dictionary<string, Movie>();

            var torrents = torrentsRepo.GetPage(page, sortBy);
            foreach (var t in torrents)
            {
                Movie m = null;
                if (!string.IsNullOrEmpty(t.ImdbId) && t.ImdbId != "NA")
                {
                    if (!moviesCache.TryGetValue(t.ImdbId, out m))
                    {
                        m = moviesRepo.Find(t.ImdbId);
                        moviesCache.Add(t.ImdbId, m);
                    }
                }

                var tm = new TorrentModel();
                AutoMapper.Mapper.Map(t, tm);
                if (m != null)
                {
                    AutoMapper.Mapper.Map(m, tm);
                }

                model.Torrents.Add(tm);
            }

            var helper = new UrlHelper(this.ControllerContext.RequestContext);
            if (model.Torrents.Count >= Constants.PageSize)
            {
                var url = helper.Action(action, "Torrents", new { page = page + 1 });
                model.NextPageUrl = url;
            }

            if (page > 1)
            {
                var url = helper.Action(action, "Torrents", new { page = page - 1 });
                model.PreviousPageUrl = url;
            }

            return this.View(model);
        }
    }
}
