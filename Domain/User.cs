namespace Domain;

public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = default!;
    public Guid Token { get; set; }
    public string Name { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
}