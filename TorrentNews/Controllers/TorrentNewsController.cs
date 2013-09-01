namespace TorrentNews.Controllers
{
    using System;
    using System.Collections.Concurrent;
    using System.Globalization;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Http;
    using System.Web.Http.Routing;

    using TorrentNews.Dal;
    using TorrentNews.Domain;
    using TorrentNews.Filters;
    using TorrentNews.Scraping;

    public class TorrentNewsController : ApiController
    {
        private static readonly ConcurrentDictionary<string, OperationInfo> Operations = new ConcurrentDictionary<string, OperationInfo>();

        [HttpGet, SecretRequired]
        public HttpResponseMessage UpdateNews(string secret)
        {
            var operationId = Guid.NewGuid().ToString();

            var operation = new OperationInfo
                                {
                                    Id = operationId, 
                                    Status = OperationStatus.Pending, 
                                    CancellationTokenSource = new CancellationTokenSource()
                                };

            if (!Operations.TryAdd(operationId, operation))
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Cannot start operation");
            }

            Task.Factory.StartNew(
                    (op) =>
                        {
                            var op2 = (OperationInfo)op;
                            op2.CancellationTokenSource.Token.ThrowIfCancellationRequested();

                            op2.Status = OperationStatus.Running;
                            op2.StartedOn = DateTime.UtcNow;
                            op2.ExtraData.Add("removed", "N/A");
                            op2.ExtraData.Add("updated", "N/A");

                            var torrentsRepo = new TorrentsRepository();

                            op2.StatusInfo = "Removing old torrents";
                            var torrentsRemoved = torrentsRepo.RemoveOldTorrents();
                            op2.ExtraData["removed"] = torrentsRemoved.ToString(CultureInfo.InvariantCulture);

                            var counter = 0;
                            var scraper = new KassScraper();
                            var torrents = scraper.GetLatestTorrents(20, op2);
                            foreach (var t in torrents)
                            {
                                torrentsRepo.Save(t);

                                counter++;
                                op2.ExtraData["updated"] = counter.ToString(CultureInfo.InvariantCulture);

                                op2.CancellationTokenSource.Token.ThrowIfCancellationRequested();
                            }
                        }, 
                    operation, 
                    operation.CancellationTokenSource.Token)
                .ContinueWith(
                    (task, op) =>
                        {
                            var op2 = (OperationInfo)op;
                            op2.FinishedOn = DateTime.UtcNow;

                            switch (task.Status)
                            {
                                case TaskStatus.RanToCompletion:
                                    op2.Status = OperationStatus.Completed;
                                    op2.StatusInfo = string.Format("Scraping done. Torrents saved: {0}", op2.ExtraData);
                                    break;
                                case TaskStatus.Faulted:
                                    op2.Status = OperationStatus.Faulted;
                                    op2.Error = task.Exception != null ? 
                                        task.Exception.ToString() : "Unexpected error";
                                    break;
                                case TaskStatus.Canceled:
                                    op2.Status = OperationStatus.Cancelled;
                                    break;
                            }
                        },
                    operation);

            var cancellationUrl = Url.Link("DefaultApi", new { controller = "TorrentNews", action = "CancelOperation", id = operationId, secret = "secret_here" });
            var statusUrl = Url.Link("DefaultApi", new { controller = "TorrentNews", action = "RetrieveOperationStatus", id = operationId, secret = "secret_here" });

            var res = new OperationAcceptedResult
                          {
                              OperationId = operationId,
                              CancellationUrl = cancellationUrl,
                              StatusUrl = statusUrl
                          };
            
            return this.Request.CreateResponse(HttpStatusCode.Accepted, res);
        }

        [HttpGet, SecretRequired]
        public HttpResponseMessage RetrieveOperationStatus(string id, string secret)
        {
            OperationInfo op;
            if (!Operations.TryGetValue(id, out op))
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Cannot retrieve operation, try again later.");
            }

            return this.Request.CreateResponse(HttpStatusCode.OK, op);
        }

        [HttpGet, SecretRequired]
        public HttpResponseMessage CancelOperation(string id, string secret)
        {
            OperationInfo op;
            if (!Operations.TryGetValue(id, out op))
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Cannot retrieve operation, try again later.");
            }

            if (!op.CancellationTokenSource.IsCancellationRequested)
            {
                op.Status = OperationStatus.Cancelling;
                op.CancellationTokenSource.Cancel();
            }

            return this.Request.CreateResponse(HttpStatusCode.Accepted, "Cancellation requested");
        }
    }
}