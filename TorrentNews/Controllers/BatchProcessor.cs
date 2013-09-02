namespace TorrentNews.Controllers
{
    using System;
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
                    },
                    operation,
                    operation.CancellationTokenSource.Token)
                .ContinueWith(
                    (task, op) => BatchProcessor.UpdateProcessStatus(op, task),
                    operation);
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

                if (t.ImdbUrl != "NA")
                {
                    var movie = moviesRepo.Find(t.ImdbUrl);
                    if (movie == null)
                    {
                        moviesRepo.Save(new Movie { Id = t.ImdbUrl });
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