namespace ServerPropaika.Models
{
    public class Case
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string? BeforeImage { get; set; }
        public string? AfterImage { get; set; }
        public decimal? Price { get; set; }
        public bool IsPublished { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}