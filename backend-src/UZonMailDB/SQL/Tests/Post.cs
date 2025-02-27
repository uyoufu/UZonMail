namespace UZonMail.DB.SQL.Tests
{
    public class Post
    {
        public long Id { get; set; }
        public List<Tag> Tags { get; } = new List<Tag>();

    }
}
