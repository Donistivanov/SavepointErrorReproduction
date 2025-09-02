using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

DiagnosticListener.AllListeners.Subscribe(new DiagnosticObserver());

var builder = WebApplication.CreateBuilder(args);

var connectionString = "server=localhost;user=root;password=catwoman;database=ef";
builder.Services.AddDbContext<ApplicationDbContext>(
    dbContextOptions => dbContextOptions
        .UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
        .UseSeeding((context, _) =>
        {
            var testBlog = context.Set<Blog>().FirstOrDefault(b => b.Url == "http://test.com");
            if (testBlog == null)
            {
                context.Set<Blog>().Add(new Blog { Url = "http://test.com" });
                context.SaveChanges();
            }
        })
        .UseAsyncSeeding(async (context, _, cancellationToken) =>
        {
            var testBlog = await context.Set<Blog>().FirstOrDefaultAsync(b => b.Url == "http://test.com", cancellationToken);
            if (testBlog == null)
            {
                context.Set<Blog>().Add(new Blog { Url = "http://test.com" });
                await context.SaveChangesAsync(cancellationToken);
            }
        })
);

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Blog> Blogs { get; set; }
}

public class Blog
{
    public int BlogId { get; set; }
    public required string Url { get; set; }
}

public class DiagnosticObserver : IObserver<DiagnosticListener>
{
    public void OnCompleted()
        => throw new NotImplementedException();

    public void OnError(Exception error)
        => throw new NotImplementedException();

    public void OnNext(DiagnosticListener value)
    {
        if (value.Name == DbLoggerCategory.Name) // "Microsoft.EntityFrameworkCore"
        {
            value.Subscribe(new KeyValueObserver());
        }
    }
}


public class KeyValueObserver : IObserver<KeyValuePair<string, object?>>
{
    public void OnCompleted()
        => throw new NotImplementedException();

    public void OnError(Exception error)
        => throw new NotImplementedException();

    public void OnNext(KeyValuePair<string, object?> value)
    {
        if (value.Key == RelationalEventId.TransactionError.Name)
        {
            if (value.Value is not null)
            {                
                var payload = (TransactionErrorEventData)value.Value;
                Console.WriteLine($"{payload.Exception} ");
            }
        }
    }
}
