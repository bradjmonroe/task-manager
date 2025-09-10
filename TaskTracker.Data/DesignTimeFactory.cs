using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TaskTracker.Data;

public class DesignTimeFactory : IDesignTimeDbContextFactory<TasksDb>
{
    public TasksDb CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<TasksDb>()
            .UseSqlite("Data Source=tasktracker.db")
            .Options;

        return new TasksDb(options);
    }
}
