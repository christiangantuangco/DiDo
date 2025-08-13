namespace DiDo.Models;

public class Entry
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }

    public Entry()
    {
        CreatedAt = DateTime.UtcNow;
    }
}