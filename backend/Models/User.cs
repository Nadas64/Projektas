namespace backend.Models;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Portfolio Portfolio { get; set; } = null!;
}