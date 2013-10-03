namespace TorrentNews.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Threading;

    using Newtonsoft.Json;

    public class OperationInfo
    {
        public OperationInfo()
        {
            this.ExtraData = new Dictionary<string, string>();
        }

        public string Id { get; set; }

        public string OpId { get; set; }

        [IgnoreDataMember, JsonIgnore]
        public OperationStatus Status { get; set; }

        public string StatusText
        {
            get
            {
                return this.Status.ToString();
            }
        }

        public string StatusInfo { get; set; }

        [IgnoreDataMember, JsonIgnore]
        public CancellationTokenSource CancellationTokenSource { get; set; }

        public string Error { get; set; }

        public Dictionary<string, string> ExtraData { get; private set; }

        public DateTime StartedOn { get; set; }
        
        public DateTime? FinishedOn { get; set; }

        public double ElapsedSeconds 
        {
            get
            {
                if (this.FinishedOn.HasValue)
                {
                    return Math.Round(this.FinishedOn.Value.Subtract(this.StartedOn).TotalSeconds);
                }
                
                return Math.Round(DateTime.UtcNow.Subtract(this.StartedOn).TotalSeconds);
            }
        }
    }
}