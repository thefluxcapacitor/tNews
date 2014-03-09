namespace TorrentNews.Domain
{
    using System;
    using System.Globalization;

    using MongoDB.Bson.Serialization.Attributes;

    public class Torrent
    {
        [BsonId]
        public int Id { get; set; }

        public string Title { get; set; }

        public string Poster { get; set; }

        public string DetailsUrl { get; set; }

        public string Size { get; set; }

        public string Files { get; set; }

        public int Seed { get; set; }

        public int Leech { get; set; }

        public int CommentsCount { get; set; }

        public DateTime AddedOn { get; set; }

        public string ImdbId { get; set; }

        public bool ImdbAward { get; set; }

        public bool MetacriticAward { get; set; }

        public bool PopularityAward { get; set; }

        public bool SuperPopularityAward { get; set; }

        public int Score { get; set; }

        public bool Latest { get; set; }

        public ReleaseSource GetReleaseSource()
        {
            return TorrentHelper.GetReleaseSource(this.Title);
        }

        public string GetAge()
        {
            var ts = DateTime.UtcNow.Subtract(this.AddedOn);
            string unit;
            
            var value = ts.TotalDays;
            if (value >= 1)
            {
                if (value >= 7)
                {
                    unit = "weeks";
                    value = Math.Round(value / 7);
                }
                else
                {
                    unit = "days";
                }
            }
            else
            {
                value = ts.TotalHours;
                if (value >= 1)
                {
                    unit = "hours";
                }
                else
                {
                    value = ts.TotalMinutes;
                    unit = "minutes";
                }
            }

            value = Math.Round(value, 0);
            return value.ToString(CultureInfo.InvariantCulture) + " " + 
                (value == 1 ? unit.Remove(unit.Length - 1) : unit);
        }

        public bool HasImdbId()
        {
            return !string.IsNullOrEmpty(this.ImdbId) && this.ImdbId != "NA";
        }

        public void SetAddedOnFromAge(DateTime now, string age)
        {
            var span = this.GetAgeTimeSpan(age);
            this.AddedOn = now.Subtract(span);
        }

        private TimeSpan GetAgeTimeSpan(string age)
        {
            var splitted = age.Split(new string[] { "&nbsp;" }, StringSplitOptions.RemoveEmptyEntries);

            var value = int.Parse(splitted[0].Trim());
            var unit = splitted[1].ToLowerInvariant().Trim();

            if (unit == "min.")
            {
                return TimeSpan.FromMinutes(value);
            }

            if (unit == "hour" || unit == "hours")
            {
                return TimeSpan.FromHours(value);
            }

            if (unit == "day" || unit == "days")
            {
                return TimeSpan.FromDays(value);
            }

            if (unit == "week" || unit == "weeks")
            {
                return TimeSpan.FromDays(value * 7);
            }

            if (unit == "month" || unit == "months")
            {
                return TimeSpan.FromDays(value * 30);
            }

            throw new ArgumentException(string.Format("Invalid unit for Age field ({0})", age), "age");
        }
    }
}