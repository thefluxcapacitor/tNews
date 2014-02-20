namespace TorrentNews
{
    using System;
    using System.Linq;
    using System.Net.Http.Headers;
    using System.Web;
    using System.Web.Http;
    using System.Web.Mvc;
    using System.Web.Optimization;
    using System.Web.Routing;

    using Newtonsoft.Json.Converters;

    using TorrentNews.App_Start;
    using TorrentNews.Domain;
    using TorrentNews.Models;

    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            AuthConfig.RegisterAuth();

            ConfigureWebApiFormatters();

            ConfigureMappings();
        }

        protected void Application_EndRequest(object sender, EventArgs e)
        {
            var cookie = this.Request.Cookies[".ASPXAUTH"];
            if (cookie != null)
            {
                var respCookie = new HttpCookie(".ASPXAUTH", cookie.Value);
                respCookie.Expires = DateTime.Now.AddDays(7);
                this.Request.Cookies.Set(respCookie);
            }
        } 

        private static void ConfigureMappings()
        {
            AutoMapper.Mapper.CreateMap<Torrent, TorrentModel>();

            AutoMapper.Mapper.CreateMap<Movie, TorrentModel>()
                .ForMember(
                    dest => dest.Poster, opt => opt.Ignore())
                .ForMember(
                    dest => dest.Id, opt => opt.Ignore())
                .ForMember(
                    dest => dest.Title, opt => opt.Ignore())
                .ForMember(
                    dest => dest.MovieTitle, opt => opt.MapFrom(src => src.Title))
                .ForMember(
                    dest => dest.Genres,
                    opt => opt.MapFrom(src => src.Genres.Any() ? src.Genres.Aggregate((c, n) => c + ", " + n) : string.Empty));

            AutoMapper.Mapper.CreateMap<Torrent, RelatedTorrentModel>();
        }

        private static void ConfigureWebApiFormatters()
        {
            var jsonFormatter = GlobalConfiguration.Configuration.Formatters.JsonFormatter;
            jsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/html"));
            jsonFormatter.SerializerSettings.Converters.Add(new IsoDateTimeConverter { DateTimeFormat = "yyyy-MM-dd HH:mm:ss" });
        }

    }
}