namespace TorrentNews.App_Start
{
    using System.Configuration;

    using Microsoft.Web.WebPages.OAuth;

    public static class AuthConfig
    {
        public static void RegisterAuth()
        {
            // To let users of this site log in using their accounts from other sites such as Microsoft, Facebook, and Twitter,
            // you must update this site. For more information visit http://go.microsoft.com/fwlink/?LinkID=252166

            //var msAppSecret = ConfigurationManager.AppSettings["MsAppSecret"];
            //if (!string.IsNullOrEmpty(msAppSecret))
            //{
            //    OAuthWebSecurity.RegisterMicrosoftClient(
            //        clientId: "a",
            //        clientSecret: msAppSecret);
            //}

            var twitterAppSecret = ConfigurationManager.AppSettings["TwitterAppSecret"];
            if (!string.IsNullOrEmpty(twitterAppSecret))
            {
                OAuthWebSecurity.RegisterTwitterClient(
                    consumerKey: "d6tePBz3jD7nGnk0E50Qg", 
                    consumerSecret: twitterAppSecret);
            }

            var fbAppSecret = ConfigurationManager.AppSettings["FacebookAppSecret"];
            if (!string.IsNullOrEmpty(fbAppSecret))
            {
                OAuthWebSecurity.RegisterFacebookClient(
                    appId: "513621042063202", 
                    appSecret: fbAppSecret);
            }

            OAuthWebSecurity.RegisterGoogleClient();
        }
    }
}
