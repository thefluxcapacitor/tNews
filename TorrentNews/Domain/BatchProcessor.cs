namespace TorrentNews.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;

    using TorrentNews.Dal;
    using TorrentNews.Scraping;

    public class BatchProcessor
    {
        private readonly bool updateProgressInDatabase;

        public BatchProcessor(bool updateProgressInDatabase)
        {
            this.updateProgressInDatabase = updateProgressInDatabase;
        }

        public void ProcessTorrentNews(int maxPages, string age, OperationInfo operation)
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
                        op2.ExtraData.Add("scoresUpdated", "N/A");

                        var torrentsRepo = new TorrentsRepository();
                        var moviesRepo = new MoviesRepository();
                        
                        var opsRepo = new OperationsRepository();
                        var o = new Operation { Id = op2.Id, Info = op2, AddedOn = DateTime.UtcNow };
                        opsRepo.Save(o);

                        this.RemoveOldTorrents(op2, torrentsRepo, opsRepo);
                        this.UpdateTorrents(maxPages, age, torrentsRepo, op2, moviesRepo, opsRepo);
                        this.UpdateMovies(moviesRepo, op2, opsRepo);
                        this.UpdateAwards(moviesRepo, torrentsRepo, op2, opsRepo);
                    },
                    operation,
                    operation.CancellationTokenSource.Token)
                .ContinueWith(
                    (task, op) => this.UpdateProcessStatus(op, task),
                    operation);
        }

        private void UpdateAwards(MoviesRepository moviesRepo, TorrentsRepository torrentsRepo, OperationInfo op, OperationsRepository opsRepo)
        {
            op.StatusInfo = "Updating awards";
            this.UpdateOperationInfo(op, opsRepo);

            //Logic for updating Latest
            torrentsRepo.UpdateLatestToFalse();

            var moviesCache = new Dictionary<string, Movie>();
            var torrents = torrentsRepo.GetAllSortedByImdbIdAndAddedOn();

            var i = 0;
            Torrent previous = null;
            Torrent current = null;

            foreach (var t in torrents)
            {
                current = t;

                //Logic for updating Latest
                if (previous == null)
                {
                    previous = current;
                }
                else
                {
                    if (previous.ImdbId != current.ImdbId)
                    {
                        previous.Latest = true;
                        torrentsRepo.Save(previous);
                    }
                }

                var isDirty = false;

                Movie movie = null;
                if (current.HasImdbId())
                {
                    if (!moviesCache.TryGetValue(current.ImdbId, out movie))
                    {
                        movie = moviesRepo.Find(current.ImdbId);
                        moviesCache.Add(current.ImdbId, movie);
                    }
                }

                if (movie != null)
                {
                    current.ImdbAward = this.GetValue(current.ImdbAward, movie.ImdbVotes >= 1000 && movie.ImdbRating >= 70, ref isDirty);
                    current.MetacriticAward = this.GetValue(current.MetacriticAward, movie.McMetascore >= 70, ref isDirty);
                }
                else
                {
                    current.ImdbAward = this.GetValue(current.ImdbAward, false, ref isDirty);
                    current.MetacriticAward = this.GetValue(current.MetacriticAward, false, ref isDirty);
                }

                current.SuperPopularityAward = this.GetValue(current.SuperPopularityAward, current.Seed >= 3000 || current.CommentsCount >= 100, ref isDirty);
                current.PopularityAward = this.GetValue(current.PopularityAward, !current.SuperPopularityAward && (current.Seed >= 1000 || current.CommentsCount >= 20), ref isDirty);

                current.Score = this.GetValue(current.Score, this.CalcScore(current), ref isDirty);

                if (isDirty)
                {
                    torrentsRepo.Save(current);
                }

                i++;

                op.ExtraData["scoresUpdated"] = i.ToString(CultureInfo.InvariantCulture);
                this.UpdateOperationInfo(op, opsRepo);
                op.CancellationTokenSource.Token.ThrowIfCancellationRequested();
            }

            //Logic for updating Latest
            if (current != null)
            {
                current.Latest = true;
                torrentsRepo.Save(current);
            }
        }

        private int CalcScore(Torrent t)
        {
            ////1000 seeds or 20 comments: popular -> 100 points
            ////3000 seeds or 100 comments: very popular -> 200 points
            ////7 imdb rating: imdb ribbon (minimum of 1000 votes) -> 400 points
            ////70 metacritic: metacritic ribbon (no minimum critics) -> 500 points
            
            return Convert.ToInt32(t.PopularityAward) * 100 +
                Convert.ToInt32(t.SuperPopularityAward) * 200 +
                Convert.ToInt32(t.ImdbAward) * 400 +
                Convert.ToInt32(t.MetacriticAward) * 500;
        }

        private T GetValue<T>(T originalValue, T newValue, ref bool isDirty)
        {
            if (!originalValue.Equals(newValue))
            {
                isDirty = true;
            }

            return newValue;
        }

        private void UpdateMovies(MoviesRepository moviesRepo, OperationInfo op, OperationsRepository opsRepo)
        {
            op.StatusInfo = "Scraping movies";
            this.UpdateOperationInfo(op, opsRepo);

            var scraper = new ImdbScraper();

            var moviesScraped = 0;
            var moviesToUpdate = moviesRepo.GetMoviesToUpdate();
            foreach (var movie in moviesToUpdate)
            {
                scraper.UpdateMovieDetails(movie);
                moviesRepo.Save(movie);

                moviesScraped++;
                op.ExtraData["moviesScraped"] = moviesScraped.ToString(CultureInfo.InvariantCulture);
                this.UpdateOperationInfo(op, opsRepo);

                op.CancellationTokenSource.Token.ThrowIfCancellationRequested();
            }
        }

        private void UpdateProcessStatus(object op, Task task)
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

            var opsRepo = new OperationsRepository();
            this.UpdateOperationInfo(op2, opsRepo, true);
        }

        private void UpdateTorrents(int maxPages, string age, TorrentsRepository torrentsRepo, OperationInfo op, MoviesRepository moviesRepo, OperationsRepository opsRepo)
        {
            var counter = 0;
            var scraper = new KassScraper(torrentsRepo, maxPages, age);
            var torrents = scraper.GetLatestTorrents(op, o => this.UpdateOperationInfo(o, opsRepo));
            foreach (var t in torrents)
            {
                torrentsRepo.Save(t);

                if (t.ImdbId != "NA")
                {
                    var movie = moviesRepo.Find(t.ImdbId);
                    if (movie == null)
                    {
                        var newMovie = new Movie { Id = t.ImdbId };
                        if (!string.IsNullOrEmpty(t.Poster))
                        {
                            newMovie.Poster = t.Poster;
                        }

                        moviesRepo.Save(newMovie);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(t.Poster) && !t.Poster.Equals(movie.Poster))
                        {
                            movie.Poster = t.Poster;
                            moviesRepo.Save(movie);
                        }
                    }
                }

                counter++;
                op.ExtraData["updated"] = counter.ToString(CultureInfo.InvariantCulture);
                this.UpdateOperationInfo(op, opsRepo);

                op.CancellationTokenSource.Token.ThrowIfCancellationRequested();
            }
        }

        private void RemoveOldTorrents(OperationInfo op, TorrentsRepository torrentsRepo, OperationsRepository opsRepo)
        {
            op.StatusInfo = "Removing old torrents";

            this.UpdateOperationInfo(op, opsRepo);

            var torrentsRemoved = torrentsRepo.RemoveOldTorrents();
            op.ExtraData["removed"] = torrentsRemoved.ToString(CultureInfo.InvariantCulture);

            this.UpdateOperationInfo(op, opsRepo);
        }

        private void UpdateOperationInfo(OperationInfo op, OperationsRepository opsRepo, bool forceUpdate = false)
        {
            if (this.updateProgressInDatabase || forceUpdate)
            {
                var o = opsRepo.Find(op.Id);
                o.Info = op;
                o.LastUpdatedOn = DateTime.UtcNow;
                opsRepo.Save(o);
            }
        }
    }
}