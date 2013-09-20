namespace TorrentNews.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.ServiceModel.Syndication;
    using System.Text;
    using System.Web;
    using System.Web.Mvc;

    using MongoDB.Driver;

    using TorrentNews.Dal;
    using TorrentNews.Domain;
    using TorrentNews.Filters;
    using TorrentNews.Models;
    using TorrentNews.Scraping;

    public class TorrentsController : Controller
    {
        public ActionResult Index()
        {
            return this.RedirectToAction("MostRecent");
        }

        [Authorize]
        public ActionResult Starred(int page = 1)
        {
            var helper = new UrlHelper(this.ControllerContext.RequestContext);
            var model = this.GetMostRecentModel(
                page,
                0,
                true,
                () => helper.Action("Starred", "Torrents", new { page = page + 1 }),
                () => helper.Action("Starred", "Torrents", new { page = page - 1 }));

            return this.View(model);
        }

        public ActionResult MostRecent(int page = 1, int minScore = 0)
        {
            var helper = new UrlHelper(this.ControllerContext.RequestContext);
            var model = this.GetMostRecentModel(
                page, 
                minScore, 
                false,
                () => helper.Action("MostRecent", "Torrents", new { page = page + 1, minScore }),
                () => helper.Action("MostRecent", "Torrents", new { page = page - 1, minScore }));

            return this.View(model);
        }

        private TorrentsListModel GetMostRecentModel(int page, int minScore, bool st, Func<string> getNextPageUrl, Func<string> getPreviousPageUrl)
        {
            var model = new TorrentsListModel();

            var torrentsRepo = new TorrentsRepository();
            var moviesRepo = new MoviesRepository();

            var starred = this.GetCurrentUserStarred();

            var moviesCache = new Dictionary<string, Movie>();

            IEnumerable<Torrent> torrents;

            if (!st)
            {
                var sortBy = new string[] { "-AddedOn", "Score" };
                torrents = torrentsRepo.GetPageMostRecentTorrents(page, sortBy, minScore);
            }
            else
            {
                torrents = torrentsRepo.GetPageStarredTorrents(page, starred);
            }

            foreach (var t in torrents)
            {
                Movie m = null;
                if (t.HasImdbId())
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

                this.AddRelatedTorrents(tm, torrentsRepo, Constants.RelatedTorrentsCount);

                if (starred != null)
                {
                    tm.IsStarred = starred.Any(item => item.ImdbId.Equals(t.ImdbId, StringComparison.OrdinalIgnoreCase));
                }

                model.Torrents.Add(tm);
            }

            if (model.Torrents.Count >= Constants.PageSize)
            {
                var url = getNextPageUrl();
                model.NextPageUrl = url;
            }

            if (page > 1)
            {
                var url = getPreviousPageUrl();
                model.PreviousPageUrl = url;
            }

            return model;
        }

        private IList<StarredMovie> GetCurrentUserStarred()
        {
            IList<StarredMovie> starred = null;
            if (this.User.Identity.IsAuthenticated)
            {
                var usersRepo = new UsersRepository();
                var user = usersRepo.FindByUsername(this.User.Identity.Name);
                if (user != null)
                {
                    starred = user.Starred;
                }
            }

            return starred;
        }

        public ActionResult Details(int id)
        {
            var torrentsRepo = new TorrentsRepository();
            var moviesRepo = new MoviesRepository();

            var torrent = torrentsRepo.Find(id);
            if (torrent == null)
            {
                throw new HttpException(404, "Torrent not found");
            }

            var tm = new TorrentDetailsModel();
            AutoMapper.Mapper.Map<Torrent, TorrentModel>(torrent, tm);

            this.AddRelatedTorrents(tm, torrentsRepo, -1);

            if (torrent.HasImdbId())
            {
                var movie = moviesRepo.Find(torrent.ImdbId);
                if (movie != null)
                {
                    AutoMapper.Mapper.Map<Movie, TorrentModel>(movie, tm);
                }
            }

            tm.TrailersInfo = this.GetTrailersModel(tm.MovieTitle, tm.Year, tm.Id);

            return this.View(tm);
        }

        [AjaxAuthorize, HttpPost]
        public ActionResult StarAdd(string imdbId)
        {
            return this.StarAction(
                imdbId,
                (user, id) =>
                    {
                        var wl = user.Starred.FirstOrDefault(item => item.ImdbId.Equals(imdbId, StringComparison.OrdinalIgnoreCase));
                        if (wl == null)
                        {
                            user.Starred.Add(new StarredMovie { ImdbId = imdbId, StarredOn = DateTime.UtcNow });
                        }
                    });
        }

        [AjaxAuthorize, HttpPost]
        public ActionResult StarRemove(string imdbId)
        {
            return this.StarAction(imdbId, (user, id) =>
                {
                    var aux = user.Starred.Single(
                        s => s.ImdbId.Equals(imdbId, StringComparison.OrdinalIgnoreCase));
                    user.Starred.Remove(aux);
                });
        }

        private ActionResult StarAction(string imdbId, Action<User, string> action)
        {
            try
            {
                if (string.IsNullOrEmpty(imdbId))
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "IMDB Id cannot be empty.");
                }

                var repo = new UsersRepository();
                var user = repo.FindByUsername(this.User.Identity.Name);

                if (user == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "User not found.");
                }

                action(user, imdbId);

                repo.Save(user);

                return this.Json("status: \"ok\"", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "An unexpected error has occurred. " + ex.Message);
            }
        }

        private void AddRelatedTorrents(TorrentModel tm, TorrentsRepository torrentsRepo, int maxTorrents)
        {
            if (!tm.HasImdbId())
            {
                return;
            }

            var torrents = torrentsRepo.FindByImdbId(tm.ImdbId, maxTorrents + 1); // maxTorrents + 1 just in case we get the current torrent and we have to dismiss it
            foreach (var t in torrents)
            {
                if (tm.RelatedTorrents.Count == maxTorrents)
                {
                    break;
                }

                if (t.Id == tm.Id)
                {
                    continue;
                }

                var relatedTorrent = new RelatedTorrentModel();
                AutoMapper.Mapper.Map(t, relatedTorrent);
                tm.RelatedTorrents.Add(relatedTorrent);
            }
        }

        public ActionResult Trailer(string title, string year, int torrentId)
        {
            var model = this.GetTrailersModel(title, year, torrentId);

            return this.PartialView("_Trailer", model);
        }

        private TrailersModel GetTrailersModel(string title, string year, int torrentId)
        {
            var searchString = this.Server.UrlEncode(title + " " + year + " trailer");
            var scraper = new YoutubeScraper();
            var trailerData = scraper.GetTrailersUrl(searchString).First();
            var searchUrl = scraper.GetTrailersSearchUrl(searchString);
            var model = new TrailersModel
                            {
                                TrailerTitle = trailerData.Item1, 
                                TrailerUrl = "//www.youtube.com/embed/" + trailerData.Item2, 
                                MoreTrailersUrl = searchUrl,
                                TorrentId = torrentId
                            };
            return model;
        }

        public ActionResult MostRecentFeed()
        {
            var torrentsRepo = new TorrentsRepository();
            var moviesRepo = new MoviesRepository();

            var moviesCache = new Dictionary<string, Movie>();

            var rssItems = new List<SyndicationItem>();

            var torrents = torrentsRepo.GetRssItems(new string[] { "-AddedOn", "Score" }).SetLimit(500);
            foreach (var t in torrents)
            {
                Movie m = null;
                if (t.HasImdbId())
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
                    new Uri(this.Request.Url.Scheme + "://" + this.Request.Url.Host + "/Torrents/Details/" + t.Id.ToString(CultureInfo.InvariantCulture)),
                    t.Id.ToString(CultureInfo.InvariantCulture),
                    t.AddedOn);
                
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
            
            if (model.HasImdbId())
            {
                html.Append("<div style=\"margin-bottom: 10px;\">" + model.Plot + "</div>");
                
                html.Append("<div><b>Genres: </b>" + model.Genres + "</div>");
                html.Append("<div><b>Director: </b>" + model.Directors + "</div>");
                html.Append("<div><b>Cast: </b>" + model.Cast + "</div>");
                html.Append("<div><b>Content rating: </b>" + (model.ContentRating ?? "N/A") + "</div>");
                ////html.AppendFormat("<div><b>IMDB link: </b><a href=\"{0}\">{0}</a></div>", "http://www.imdb.com/title/" + model.ImdbId);
            }

            if (model.Score > 0)
            {
                html.Append("<div><b>Score: </b>" + model.Score.ToString(CultureInfo.InvariantCulture) + "</div>");
            }

            html.Append("<div><b>Torrent: </b>" + model.Title + "</div>");
            html.Append("<div><b>Age: </b>" + model.Age + "</div>");

            return html.ToString();
        }
    }
}
