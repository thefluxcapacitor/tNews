namespace TorrentNews.Domain
{
    public class OperationAcceptedResult
    {
        public string OperationId { get; set; }
        
        public string StatusUrl { get; set; }

        public string CancellationUrl { get; set; }
    }
}