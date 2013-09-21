namespace TorrentNews.Models
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class RegisterExternalLoginModel
    {
        [Required]
        [Display(Name = "User name")]
        [RegularExpression(@"[a-z0-9A-Z\.@_]*", ErrorMessage = "Only letters, numbers and special characters such as _ . @ are allowed")]
        public string UserName { get; set; }

        public string ExternalLoginData { get; set; }

        public string FB_name { get; set; }

        public string FB_link { get; set; }

        public string GL_email { get; set; }
    }

    public class ExternalLogin
    {
        public string Provider { get; set; }

        public string ProviderDisplayName { get; set; }
        
        public string ProviderUserId { get; set; }
    }
}
