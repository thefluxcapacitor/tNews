namespace TorrentNews.Domain
{
    using System.Collections.Generic;

    using MongoDB.Bson.Serialization.Attributes;

    public class User
    {
        public User()
        {
            this.Watchlist = new List<string>();
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

        public string AuthProvider { get; set; }

        public string Provider { get; set; }
        
        public string ProviderUserId { get; set; }

        public IList<string> Watchlist { get; set; }

        public static int GetFabricatedId(string provider, string providerUserId)
        {
            return (provider + "@" + providerUserId).GetHashCode();
        }
    }
}