namespace TorrentNews.Models
{
    using System.ComponentModel.DataAnnotations;

    public class RegisterExternalLoginModel
    {
        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }

        public string ExternalLoginData { get; set; }
    }

    public class ExternalLogin
    {
        public string Provider { get; set; }

        public string ProviderDisplayName { get; set; }
        
        public string ProviderUserId { get; set; }
    }
}
