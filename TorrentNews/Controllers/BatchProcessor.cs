namespace TorrentNews.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Threading.Tasks;

    using TorrentNews.Dal;
    using TorrentNews.Domain;
    using TorrentNews.Scraping;

    public static class BatchProcessor
    {
        public static void ProcessTorrentNews(int maxPages, OperationInfo operation)
        {
            Task.Factory.StartNew(
                    (op) =>
                    {
                        var op2 = (OperationInfo)op;
                        op2.CancellationTokenSource.Token.ThrowIfCancellationRequested();

                        op2.Status = OperationStatus.Running;
                        op2.StartedOn = DateTime.UtcNow;
                        op2.ExtraData.Add("removed", "N/A");
                        op2.ExtraData.Add("updated", "N/A");
                        op2.ExtraData.Add("scraped", "N/A");
                        op2.ExtraData.Add("moviesScraped", "N/A");

                        var torrentsRepo = new TorrentsRepository();
                        var moviesRepo = new MoviesRepository();

                        BatchProcessor.RemoveOldTorrents(op2, torrentsRepo);
                        BatchProcessor.UpdateTorrents(maxPages, torrentsRepo, op2, moviesRepo);
                        BatchProcessor.UpdateMovies(moviesRepo, op2);
                        BatchProcessor.UpdateAwards(moviesRepo, torrentsRepo, op2);
                    },
                    operation,
                    operation.CancellationTokenSource.Token)
                .ContinueWith(
                    (task, op) => BatchProcessor.UpdateProcessStatus(op, task),
                    operation);
        }

        private static void UpdateAwards(MoviesRepository moviesRepo, TorrentsRepository torrentsRepo, OperationInfo op)
        {
            ////1000 seeds or 20 comments: popular -> 100 points
            ////3000 seeds or 100 comments: very popular -> 200 points
            ////7 imdb rating: imdb ribbon (minimum of 1000 votes) -> 400 points
            ////70 metacritic: metacritic ribbon (no minimum critics) -> 500 points

            op.StatusInfo = "Updating awards";

            var moviesCache = new Dictionary<string, Movie>();
            var torrents = torrentsRepo.FindAll();
            foreach (var t in torrents)
            {
                var isDirty = false;

                Movie movie = null;
                if (!string.IsNullOrEmpty(t.ImdbId) && t.ImdbId != "NA")
                {
                    if (!moviesCache.TryGetValue(t.ImdbId, out movie))
                    {
                        movie = moviesRepo.Find(t.ImdbId);
                        moviesCache.Add(t.ImdbId, movie);
                    }
                }

                if (movie != null)
                {
                    t.ImdbAward = GetValue(t.ImdbAward, movie.ImdbVotes >= 1000 && movie.ImdbRating >= 70, ref isDirty);
                    t.MetacriticAward = GetValue(t.MetacriticAward, movie.McMetascore >= 70, ref isDirty);
                }
                else
                {
                    t.ImdbAward = GetValue(t.ImdbAward, false, ref isDirty);
                    t.MetacriticAward = GetValue(t.MetacriticAward, false, ref isDirty);
                }

                t.SuperPopularityAward = GetValue(t.SuperPopularityAward, t.Seed >= 3000 || t.CommentsCount >= 100, ref isDirty);
                t.PopularityAward = GetValue(t.PopularityAward, !t.SuperPopularityAward && (t.Seed >= 1000 || t.CommentsCount >= 20), ref isDirty);

                if (isDirty)
                {
                    torrentsRepo.Save(t);
                }
            }
        }

        private static bool GetValue(bool originalValue, bool newValue, ref bool isDirty)
        {
            if (originalValue != newValue)
            {
                isDirty = true;
            }

            return newValue;
        }

        private static void UpdateMovies(MoviesRepository moviesRepo, OperationInfo op)
        {
            op.StatusInfo = "Scraping movies";

            var scraper = new ImdbScraper();

            var moviesScraped = 0;
            var moviesToUpdate = moviesRepo.GetMoviesToUpdate();
            foreach (var movie in moviesToUpdate)
            {
                scraper.UpdateMovieDetails(movie);
                moviesRepo.Save(movie);

                moviesScraped++;
                op.ExtraData["moviesScraped"] = moviesScraped.ToString(CultureInfo.InvariantCulture);
            }
        }

        private static void UpdateProcessStatus(object op, Task task)
        {
            var op2 = (OperationInfo)op;
            op2.FinishedOn = DateTime.UtcNow;

            switch (task.Status)
            {
                case TaskStatus.RanToCompletion:
                    op2.Status = OperationStatus.Completed;
                    op2.StatusInfo = string.Format("Scraping done. Torrents saved: {0}", op2.ExtraData["updated"]);
                    break;
                case TaskStatus.Faulted:
                    op2.Status = OperationStatus.Faulted;
                    op2.Error = task.Exception != null ? task.Exception.ToString() : "Unexpected error";
                    break;
                case TaskStatus.Canceled:
                    op2.Status = OperationStatus.Cancelled;
                    break;
            }
        }

        private static void UpdateTorrents(int maxPages, TorrentsRepository torrentsRepo, OperationInfo op, MoviesRepository moviesRepo)
        {
            var counter = 0;
            var scraper = new KassScraper(torrentsRepo, maxPages);
            var torrents = scraper.GetLatestTorrents(op);
            foreach (var t in torrents)
            {
                torrentsRepo.Save(t);

                if (t.ImdbId != "NA")
                {
                    var movie = moviesRepo.Find(t.ImdbId);
                    if (movie == null)
                    {
                        moviesRepo.Save(new Movie { Id = t.ImdbId });
                    }
                }

                counter++;
                op.ExtraData["updated"] = counter.ToString(CultureInfo.InvariantCulture);

                op.CancellationTokenSource.Token.ThrowIfCancellationRequested();
            }
        }

        private static void RemoveOldTorrents(OperationInfo op, TorrentsRepository torrentsRepo)
        {
            op.StatusInfo = "Removing old torrents";
            var torrentsRemoved = torrentsRepo.RemoveOldTorrents();
            op.ExtraData["removed"] = torrentsRemoved.ToString(CultureInfo.InvariantCulture);
        }
    }
}