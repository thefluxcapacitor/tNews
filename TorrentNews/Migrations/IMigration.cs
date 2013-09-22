namespace TorrentNews.Migrations
{
    public interface IMigration
    {
        void Apply();

        string GetName();
    }
}