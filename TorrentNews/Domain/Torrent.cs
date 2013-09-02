﻿namespace TorrentNews.Domain
{
    using System;

    using MongoDB.Bson;
    using MongoDB.Bson.Serialization.Attributes;
    using MongoDB.Bson.Serialization.IdGenerators;

    public class Torrent
    {
        [BsonId]
        public int Id { get; set; }

        public string Title { get; set; }

        public string DetailsUrl { get; set; }

        public string Size { get; set; }

        public string Files { get; set; }

        public string Seed { get; set; }

        public string Leech { get; set; }

        public DateTime AddedOn { get; set; }

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

        public string ImdbUrl { get; set; }
    }
}