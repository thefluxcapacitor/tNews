namespace TorrentNews.App_Start
{
    using Microsoft.Web.WebPages.OAuth;

    public static class AuthConfig
    {
        public static void RegisterAuth()
        {
            // To let users of this site log in using their accounts from other sites such as Microsoft, Facebook, and Twitter,
            // you must update this site. For more information visit http://go.microsoft.com/fwlink/?LinkID=252166

            OAuthWebSecurity.RegisterMicrosoftClient(
                clientId: "a",
                clientSecret: "a");

            OAuthWebSecurity.RegisterTwitterClient(
                consumerKey: "a",
                consumerSecret: "a");

            OAuthWebSecurity.RegisterFacebookClient(
                appId: "a",
                appSecret: "a");

            OAuthWebSecurity.RegisterGoogleClient();
        }
    }
}
