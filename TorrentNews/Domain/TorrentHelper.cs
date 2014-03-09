namespace TorrentNews.Domain
{
    using System.Text.RegularExpressions;

    public class TorrentHelper
    {
        public static ReleaseSource GetReleaseSource(string torrentTitle)
        {
            // don't change order, always put first worst quality and last best quality

            if (ReleaseFormatIs("cam|hdcam|camrip", torrentTitle))
            {
                return new ReleaseSource { DisplayValue = "Cam", Quality = 1 };
            }

            if (ReleaseFormatIs("ts|telesync|pdvd", torrentTitle))
            {
                return new ReleaseSource { DisplayValue = "Telesync", Quality = 1 };
            }

            if (ReleaseFormatIs("wp|workprint", torrentTitle))
            {
                return new ReleaseSource { DisplayValue = "Workprint", Quality = 1 };
            }

            if (ReleaseFormatIs("tc|telecine", torrentTitle))
            {
                return new ReleaseSource { DisplayValue = "Telecine", Quality = 1 };
            }

            if (ReleaseFormatIs("ppv|ppvrip", torrentTitle))
            {
                return new ReleaseSource { DisplayValue = "Pay-per-view", Quality = 2 };
            }

            if (ReleaseFormatIs("scr|screener|dvdscr|dvdscreener|bdscr|ddc", torrentTitle))
            {
                return new ReleaseSource { DisplayValue = "Screener", Quality = 2 };
            }

            if (ReleaseFormatIs(@"r5\.ac3\.5\.1\.hq|r5\.line|r5", torrentTitle))
            {
                return new ReleaseSource { DisplayValue = "R5", Quality = 2 };
            }

            if (ReleaseFormatIs(@"web\-rip|webrip|web rip", torrentTitle))
            {
                return new ReleaseSource { DisplayValue = "WEB Rip", Quality = 2 };
            }

            if (ReleaseFormatIs("dvdrip", torrentTitle))
            {
                return new ReleaseSource { DisplayValue = "DVDRip", Quality = 3 };
            }

            if (ReleaseFormatIs("dsr|dsrip|dthrip|dvbrip|hdtv|pdtv|tvrip|hdtvrip", torrentTitle))
            {
                return new ReleaseSource { DisplayValue = "HDTV", Quality = 3 };
            }

            if (ReleaseFormatIs("vodrip|vodr", torrentTitle))
            {
                return new ReleaseSource { DisplayValue = "VODRip", Quality = 3 };
            }

            if (ReleaseFormatIs(@"hdrip|bdrip|brrip|blu\-ray|bluray|bdr|bd5|bd9", torrentTitle))
            {
                return new ReleaseSource { DisplayValue = "BD/BRRip", Quality = 3 };
            }

            if (ReleaseFormatIs(@"webdl|web dl|web\-dl", torrentTitle))
            {
                return new ReleaseSource { DisplayValue = "WEB-DL", Quality = 3 };
            }

            return new ReleaseSource { DisplayValue = "Unknown", Quality = 0 };
        }

        private static bool ReleaseFormatIs(string formats, string torrentTitle)
        {
            var title = torrentTitle.ToLowerInvariant();
            var regex = new Regex(@"[\.,\-,\s](" + formats + @")($|[\.,\-,\s])");
            var match = regex.Match(title);
            return match.Success;
        }
    }
}