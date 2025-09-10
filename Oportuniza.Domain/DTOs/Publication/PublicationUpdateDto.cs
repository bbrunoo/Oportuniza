namespace Oportuniza.Domain.DTOs.Publication
{
    public class PublicationUpdateDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Salary { get; set; }
        public string Shift { get; set; }
        public string Contract { get; set; }
        public string Local { get; set; }
        public DateTime ExpirationDate { get; set; }
        public Guid AuthorUserId { get; set; }
    }
}
