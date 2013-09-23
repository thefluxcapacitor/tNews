namespace TorrentNews.Exceptions
{
    using System;

    public class LimitStarredMoviesExceededException : Exception
    {
        public LimitStarredMoviesExceededException(int limit)
            : base(string.Format("Limit of starred movies exceeded ({0})", limit))
        {
        }
    }
}
