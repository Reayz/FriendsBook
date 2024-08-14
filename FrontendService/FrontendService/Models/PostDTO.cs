namespace FrontendService.Models
{
    public class PostDTO
    {
        public int PostId { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public string AuthorName { get; set; }
        public int AuthorId { get; set; }
        public DateTime PublishDate { get; set; }
        public DateTime? LastEditedDate { get; set; }
    }
}
