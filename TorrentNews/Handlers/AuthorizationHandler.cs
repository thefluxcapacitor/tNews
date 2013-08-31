namespace TorrentNews.Handlers
{
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web;

    public class AuthorizationHandler : DelegatingHandler
    {
        private readonly string storedSecret;

        public AuthorizationHandler(string secret)
        {
            this.storedSecret = secret;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var secret = HttpUtility.ParseQueryString(request.RequestUri.Query).Get("secret");
            if (!string.IsNullOrEmpty(secret))
            {
                if (!this.storedSecret.Equals(secret))
                {
                    var response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
                    var tsc = new TaskCompletionSource<HttpResponseMessage>();
                    tsc.SetResult(response);
                    return tsc.Task;
                }
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}