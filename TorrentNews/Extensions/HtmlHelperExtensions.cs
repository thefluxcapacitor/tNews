namespace TorrentNews.Extensions
{
    using System.Linq;
    using System.Web;
    using System.Web.Mvc;

    public static class HtmlHelperExtensions
    {
        public static MvcHtmlString ImdbNames(this HtmlHelper helper, string[] names)
        {
            string result;
            if (names == null || names.Length == 0)
            {
                result = string.Empty;
            }
            else if (names.Length == 1)
            {
                result = GetImdbNameLink(names[0]);
            }
            else
            {
                result = names.Aggregate((c, n) => GetImdbNameLink(c) + ", " + GetImdbNameLink(n));
            }
            
            return new MvcHtmlString(result);
        }

        private static string GetImdbNameLink(string name)
        {
            return string.Format("<a href=\"http://www.imdb.com/find?q={0}&s=nm&exact=true&ref_=fn_nm_ex\" target=\"_blank\">{1}</a>", HttpUtility.UrlEncode(name), name);
        }
    }
}