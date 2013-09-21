namespace TorrentNews.Domain
{
    using System;
    using System.Collections.Generic;

    using MongoDB.Bson.Serialization.Attributes;

    public class User
    {
        public User()
        {
            this.Starred = new List<StarredMovie>();
        }

        [BsonId]
        public int Id { get; set; }

        public string Username { get; set; }

        public string UsernameLower 
        {
            get
            {
                return string.IsNullOrEmpty(this.Username) ? null : this.Username.ToLowerInvariant();
            }

            set
            {
            }
        }

        public string Provider { get; set; }
        
        public string ProviderUserId { get; set; }

        public IList<StarredMovie> Starred { get; set; }

        public string FB_name { get; set; }

        public string FB_link { get; set; }

        public string GL_email { get; set; }

        public DateTime? LastMovieSeenAddedOn { get; set; }

        public static int GetFabricatedId(string provider, string providerUserId)
        {
            return (provider + "@" + providerUserId).GetHashCode();
        }
    }
}