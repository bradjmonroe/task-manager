namespace TaskTracker.Data.Models;

public class TaskItem
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public bool IsDone { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    //FK User
    public Guid CreatedBy { get; set; }
    public AppUser? User { get; set; }
}

