// var builder = WebApplication.CreateBuilder(args);

// // Add services to the container.
// // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
// builder.Services.AddOpenApi();

// var app = builder.Build();

// // Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
//     app.MapOpenApi();
// }

// app.UseHttpsRedirection();

// var summaries = new[]
// {
//     "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
// };

// app.MapGet("/weatherforecast", () =>
// {
//     var forecast =  Enumerable.Range(1, 5).Select(index =>
//         new WeatherForecast
//         (
//             DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
//             Random.Shared.Next(-20, 55),
//             summaries[Random.Shared.Next(summaries.Length)]
//         ))
//         .ToArray();
//     return forecast;
// })
// .WithName("GetWeatherForecast");

// app.Run();

// record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
// {
//     public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
// }

using Microsoft.EntityFrameworkCore;
using MyApp.Data;
using MyApp.GraphQL;

var builder = WebApplication.CreateBuilder(args);

var connStr = builder.Configuration.GetConnectionString("DefaultConnection") ??
              Environment.GetEnvironmentVariable("DATABASE_URL");

builder.Services.AddPooledDbContextFactory<AppDbContext>(
    options => options.UseNpgsql(connStr));

builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>()
    .AddFiltering()
    .AddSorting();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGraphQL(); // GraphQL endpoint → /graphql
app.MapFallbackToFile("/index.html");

// Migrate & Seed
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider
        .GetRequiredService<IDbContextFactory<AppDbContext>>()
        .CreateDbContext();

    dbContext.Database.Migrate();

    if (!dbContext.Samples.Any())
    {
        dbContext.Samples.Add(new MyApp.Models.Sample { Name = "GraphQL Hello" });
        dbContext.SaveChanges();
    }
}

app.Run();