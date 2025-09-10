namespace TaskTracker.Data.Models;

public class AppUser
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Email { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    //Task References
    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
}