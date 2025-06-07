namespace WebApp.DTO;

public class RegisterRequest
{
    public string Username { get; set; } = default!;
    public Guid Token { get; set; } = default!;
}