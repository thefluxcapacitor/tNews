namespace TorrentNews.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Web;
    using System.Web.Mvc;

    using TorrentNews.Dal;
    using TorrentNews.Domain;
    using TorrentNews.Exceptions;
    using TorrentNews.Filters;
    using TorrentNews.Models;
    using TorrentNews.Scraping;

    public class TorrentsController : Controller
    {
        private readonly UsersRepository usersRepo;

        private readonly TorrentsRepository torrentsRepo;

        private readonly MoviesRepository moviesRepo;

        public TorrentsController()
        {
            this.usersRepo = new UsersRepository();
            this.torrentsRepo = new TorrentsRepository();
            this.moviesRepo = new MoviesRepository();
        }

        public ActionResult Index()
        {
            return this.RedirectToAction("MostRecent");
        }

        public ActionResult Search(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm) || searchTerm.Length < 3)
            {
                throw new Exception("Search term must be at least 3 characters long");
            }

            var torrents = this.torrentsRepo.Search(searchTerm).ToList();

            var movies = this.moviesRepo.Search(searchTerm);
            foreach (var m in movies)
            {
                var t = this.torrentsRepo.FindByImdbId(m.Id, 0);
                torrents.AddRange(t);
            }

            return this.Json(torrents, JsonRequestBehavior.AllowGet);
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
            this.CheckAndUpdateBookmark();

            var helper = new UrlHelper(this.ControllerContext.RequestContext);
            var model = this.GetMostRecentModel(
                page, 
                minScore, 
                false,
                () => helper.Action("MostRecent", "Torrents", new { page = page + 1, minScore }),
                () => helper.Action("MostRecent", "Torrents", new { page = page - 1, minScore }));

            return this.View(model);
        }

        private void CheckAndUpdateBookmark()
        {
            var user = this.GetCurrentUser();
            if (user != null && user.BookmarkPosition != null)
            {
                var t = this.torrentsRepo.FindBookmarkedTorrent(user.BookmarkPosition.BookmarkedTorrentId);
                if (t == null)
                {
                    var newestTorrents = this.torrentsRepo.GetTorrentsNewerThan(user.BookmarkPosition.BookmarkedTorrentAddedOn);
                    var newBookmarkedTorrent = newestTorrents.LastOrDefault();
                    if (newBookmarkedTorrent != null)
                    {
                        user.BookmarkPosition.BookmarkedTorrentAddedOn = newBookmarkedTorrent.AddedOn;
                        user.BookmarkPosition.BookmarkedTorrentId = newBookmarkedTorrent.Id;
                        this.usersRepo.Save(user);
                    }
                }
            }
        }

        private TorrentsListModel GetMostRecentModel(int page, int minScore, bool st, Func<string> getNextPageUrl, Func<string> getPreviousPageUrl)
        {
            var model = new TorrentsListModel();

            var currentUser = this.GetCurrentUser();
            var starred = this.GetUserStarred(currentUser);

            var moviesCache = new Dictionary<string, Movie>();

            IEnumerable<Torrent> torrents;

            if (!st)
            {
                var sortBy = new string[] { "-AddedOn", "_id" };
                torrents = this.torrentsRepo.GetPageMostRecentTorrents(page, sortBy, minScore);
                model.ShowGoToBookmark = true;
                model.ShowBookmarks = true;
                model.BookmarkSet = currentUser == null || currentUser.BookmarkPosition != null;
            }
            else
            {
                torrents = this.torrentsRepo.GetPageStarredTorrents(page, starred);
            }

            foreach (var t in torrents)
            {
                Movie m = null;
                if (t.HasImdbId())
                {
                    if (!moviesCache.TryGetValue(t.ImdbId, out m))
                    {
                        m = this.moviesRepo.Find(t.ImdbId);
                        moviesCache.Add(t.ImdbId, m);
                    }
                }

                var tm = new TorrentModel();
                AutoMapper.Mapper.Map(t, tm);
                if (m != null)
                {
                    AutoMapper.Mapper.Map(m, tm);

                    if (string.IsNullOrEmpty(tm.Poster))
                    {
                        tm.Poster = m.Poster;
                    }
                }

                this.AddRelatedTorrents(tm, Constants.RelatedTorrentsCount);

                if (starred != null)
                {
                    tm.IsStarred = starred.Any(item => item.ImdbId.Equals(t.ImdbId, StringComparison.OrdinalIgnoreCase));
                }

                if (currentUser != null && currentUser.BookmarkPosition != null)
                {
                    tm.IsBookmarked = currentUser.BookmarkPosition.BookmarkedTorrentId == t.Id;
                    tm.IsNew = (tm.AddedOn > currentUser.BookmarkPosition.BookmarkedTorrentAddedOn) || (tm.AddedOn == currentUser.BookmarkPosition.BookmarkedTorrentAddedOn && tm.Id < currentUser.BookmarkPosition.BookmarkedTorrentId);
                }
                else
                {
                    tm.IsNew = currentUser != null;
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

        private User GetCurrentUser()
        {
            if (this.User.Identity.IsAuthenticated)
            {
                return this.usersRepo.FindByUsername(this.User.Identity.Name);
            }

            return null;
        }

        private IList<StarredMovie> GetUserStarred(User user)
        {
            IList<StarredMovie> starred = null;
            if (user != null)
            {
                starred = user.Starred;
            }

            return starred;
        }

        public ActionResult Details(int id)
        {
            var torrent = this.torrentsRepo.Find(id);
            if (torrent == null)
            {
                throw new HttpException(404, "Torrent not found");
            }

            var tm = new TorrentDetailsModel();
            AutoMapper.Mapper.Map<Torrent, TorrentModel>(torrent, tm);

            this.AddRelatedTorrents(tm, -1);

            if (torrent.HasImdbId())
            {
                var movie = this.moviesRepo.Find(torrent.ImdbId);
                if (movie != null)
                {
                    AutoMapper.Mapper.Map<Movie, TorrentModel>(movie, tm);

                    if (string.IsNullOrEmpty(tm.Poster))
                    {
                        tm.Poster = movie.Poster;
                    }
                }
            }

            tm.TrailersInfo = this.GetTrailersModel(tm.MovieTitle, tm.Year, tm.Id);

            return this.View(tm);
        }

        [Authorize]
        public ActionResult GoToBookmark()
        {
            var user = this.GetCurrentUser();

            var sortBy = new string[] { "-AddedOn", "_id" };

            var page = 1;
            var torrents = this.torrentsRepo.GetPageMostRecentTorrents(page, sortBy, -1);
            Torrent bmTorrent = null;

            if (user.BookmarkPosition == null || !torrents.Any())
            {
                return this.RedirectToAction("MostRecent");
            }

            while (torrents.Any())
            {
                bmTorrent = torrents.SingleOrDefault(t => t.Id == user.BookmarkPosition.BookmarkedTorrentId);
                if (bmTorrent != null)
                {
                    break;
                }

                page++;
                torrents = this.torrentsRepo.GetPageMostRecentTorrents(page, sortBy, -1);
            }

            if (bmTorrent != null)
            {
                var actionUrl = Url.RouteUrl(new { controller = "Torrents", action = "MostRecent", page });
                return this.Redirect(actionUrl + "#bm" + bmTorrent.Id.ToString(CultureInfo.InvariantCulture));
            }

            return this.RedirectToAction("MostRecent");
        }

        [AjaxAuthorize, HttpPost]
        public ActionResult SetBookmark(DateTime date, int id)
        {
            try
            {
                var user = this.usersRepo.FindByUsername(this.User.Identity.Name);

                if (user == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "User not found.");
                }

                user.BookmarkPosition = new BookmarkPosition
                {
                    BookmarkedTorrentAddedOn = date.ToUniversalTime(),
                    BookmarkedTorrentId = id
                };

                this.usersRepo.Save(user);

                return this.Json("status: \"ok\"", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "An unexpected error has occurred. " + ex.Message);
            }
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
                            const int Limit = 200;
                            if (user.Starred.Count > Limit)
                            {
                                throw new LimitStarredMoviesExceededException(Limit);
                            }

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

                var user = this.usersRepo.FindByUsername(this.User.Identity.Name);

                if (user == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "User not found.");
                }

                action(user, imdbId);

                this.usersRepo.Save(user);

                return this.Json("status: \"ok\"", JsonRequestBehavior.AllowGet);
            }
            catch (LimitStarredMoviesExceededException limitEx)
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, limitEx.Message);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "An unexpected error has occurred. " + ex.Message);
            }
        }

        private void AddRelatedTorrents(TorrentModel tm, int maxTorrents)
        {
            if (!tm.HasImdbId())
            {
                return;
            }

            var torrents = this.torrentsRepo.FindByImdbId(tm.ImdbId, maxTorrents + 1); // maxTorrents + 1 just in case we get the current torrent and we have to dismiss it
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
    }
}
