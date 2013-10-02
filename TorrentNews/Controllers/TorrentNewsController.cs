namespace TorrentNews.Controllers
{
    using System;
    using System.Collections.Concurrent;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Web.Http;

    using TorrentNews.Domain;
    using TorrentNews.Filters;

    public class TorrentNewsController : ApiController
    {
        private static readonly ConcurrentDictionary<string, OperationInfo> Operations = new ConcurrentDictionary<string, OperationInfo>();

        [HttpGet]
        [SecretRequired]
        public HttpResponseMessage UpdateNews(string secret)
        {
            return this.PrvUpdateNews(-1, "week");
        }

        [HttpGet, SecretRequired]
        public HttpResponseMessage UpdateNews(string secret, int maxPages, string age)
        {
            return this.PrvUpdateNews(maxPages, age);
        }

        private HttpResponseMessage PrvUpdateNews(int maxPages, string age)
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

            BatchProcessor.ProcessTorrentNews(maxPages, age, operation);

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