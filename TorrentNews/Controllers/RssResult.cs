namespace TorrentNews.Controllers
{
    using System.Collections.Generic;
    using System.ServiceModel.Syndication;
    using System.Web;
    using System.Web.Mvc;
    using System.Xml;

    ////public class RssResult : FileResult
    ////{
    ////    private readonly SyndicationFeed feed;

    ////    public RssResult(SyndicationFeed feed) : base("application/rss+xml")
    ////    {
    ////        this.feed = feed;
    ////    }

    ////    protected override void WriteFile(HttpResponseBase response)
    ////    {
    ////        using (var writer = XmlWriter.Create(response.OutputStream))
    ////        {
    ////            this.feed.GetRss20Formatter().WriteTo(writer);
    ////        }
    ////    }
    ////}
}