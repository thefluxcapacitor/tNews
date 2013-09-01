namespace TorrentNews.Domain
{
    public enum OperationStatus
    {
        Pending,
        Running,
        Cancelling,
        Completed,
        Faulted,
        Cancelled
    }
}