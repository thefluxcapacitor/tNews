namespace TorrentNews.Controllers
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