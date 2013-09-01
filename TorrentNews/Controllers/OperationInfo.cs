namespace TorrentNews.Controllers
{
    using System;
    using System.Threading;

    public class OperationInfo
    {
        public string Id { get; set; }

        public OperationStatus Status { get; set; }

        public string StatusInfo { get; set; }

        public CancellationTokenSource CancellationTokenSource { get; set; }

        public string Error { get; set; }

        public string ExtraData { get; set; }

        public DateTime StartedOn { get; set; }
        
        public DateTime FinishedOn { get; set; }

        public double ElapsedSeconds 
        {
            get
            {
                return this.FinishedOn.Subtract(this.StartedOn).TotalSeconds;
            }
        }
    }
}