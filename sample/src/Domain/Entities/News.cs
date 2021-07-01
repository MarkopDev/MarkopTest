using Domain.Common;

namespace Domain.Entities
{
    public class News : EntityBase
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string Preview { get; set; }
        public bool? IsHidden { get; set; }
        public string AuthorId { get; set; }

        public User Author { get; set; }
    }
}