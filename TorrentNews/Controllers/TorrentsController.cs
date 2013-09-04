namespace TorrentNews.Controllers
{
    using System.Collections.Generic;
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
            return this.GetTorrentsList(page, new string[] { "-Score", "ImdbId", "-AddedOn" }, "HighestScores");
        }

        public ActionResult MostRecent(int page = 1)
        {
            return this.GetTorrentsList(page, new string[] { "-AddedOn", "Score" }, "MostRecent");
        }

        private ActionResult GetTorrentsList(int page, string[] sortBy, string action)
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
