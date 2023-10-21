using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using WorkSchedule.Infra.Persistence;

namespace WorkSchedule.Test;

public class TestDbHelper
{
    public static MyDb NewDb()
    {
        var options = new DbContextOptionsBuilder<MyDb>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .UseLazyLoadingProxies()
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        var db = new MyDb(options);
        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();
        return db;
    }
}
