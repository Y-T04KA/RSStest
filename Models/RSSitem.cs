namespace RSStest.Models
{
    public class RSSitem
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime pubDate { get; set; }
        public string Description { get; set; }
        public string Link { get; set; }
        public string Creator { get; set; }
        public string Source { get; set; }
        public bool isUnread { get; set; }
    }
}
