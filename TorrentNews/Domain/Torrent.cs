namespace TorrentNews.Domain
{
    using System;
    using System.Globalization;
    using System.Text.RegularExpressions;

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
            // don't change order, always put first worst quality and last best quality

            if (this.ReleaseFormatIs("cam|hdcam|camrip"))
            {
                return new ReleaseSource { DisplayValue = "Cam", Quality = 1 };
            }

            if (this.ReleaseFormatIs("ts|telesync|pdvd"))
            {
                return new ReleaseSource { DisplayValue = "Telesync", Quality = 1 };
            }

            if (this.ReleaseFormatIs("wp|workprint"))
            {
                return new ReleaseSource { DisplayValue = "Workprint", Quality = 1 };
            }

            if (this.ReleaseFormatIs("tc|telecine"))
            {
                return new ReleaseSource { DisplayValue = "Telecine", Quality = 1 };
            }

            if (this.ReleaseFormatIs("ppv|ppvrip"))
            {
                return new ReleaseSource { DisplayValue = "Pay-per-view", Quality = 2 };
            }

            if (this.ReleaseFormatIs("scr|screener|dvdscr|dvdscreener|bdscr|ddc"))
            {
                return new ReleaseSource { DisplayValue = "Screener", Quality = 2 };
            }

            if (this.ReleaseFormatIs(@"r5\.ac3\.5\.1\.hq|r5\.line|r5"))
            {
                return new ReleaseSource { DisplayValue = "R5", Quality = 2 };
            }

            if (this.ReleaseFormatIs(@"web\-rip|webrip|web rip"))
            {
                return new ReleaseSource { DisplayValue = "WEB Rip", Quality = 2 };
            }

            if (this.ReleaseFormatIs("dvdrip"))
            {
                return new ReleaseSource { DisplayValue = "DVDRip", Quality = 3 };
            }

            if (this.ReleaseFormatIs("dsr|dsrip|dthrip|dvbrip|hdtv|pdtv|tvrip|hdtvrip"))
            {
                return new ReleaseSource { DisplayValue = "HDTV", Quality = 3 };
            }

            if (this.ReleaseFormatIs("vodrip|vodr"))
            {
                return new ReleaseSource { DisplayValue = "VODRip", Quality = 3 };
            }

            if (this.ReleaseFormatIs(@"hdrip|bdrip|brrip|blu\-ray|bluray|bdr|bd5|bd9"))
            {
                return new ReleaseSource { DisplayValue = "BD/BRRip", Quality = 3 };
            }

            if (this.ReleaseFormatIs(@"webdl|web dl|web\-dl"))
            {
                return new ReleaseSource { DisplayValue = "WEB-DL", Quality = 3 };
            }

            return new ReleaseSource { DisplayValue = "Unknown", Quality = 0 };
        }

        private bool ReleaseFormatIs(string formats)
        {
            var title = this.Title.ToLowerInvariant();
            var regex = new Regex(@"[\.,\-,\s](" + formats + @")($|[\.,\-,\s])");
            var match = regex.Match(title);
            return match.Success;
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

            throw new ArgumentException("Invalid unit for Age field", "age");
        }
    }
}