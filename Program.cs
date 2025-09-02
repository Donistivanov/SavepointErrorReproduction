using Microsoft.EntityFrameworkCore;

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