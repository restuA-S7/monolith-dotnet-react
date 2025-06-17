using HotChocolate;
using HotChocolate.Data;
using MyApp.Data;
using MyApp.Models;

namespace MyApp.GraphQL;

public class Query
{
    [UseFiltering]
    [UseSorting]
    public IQueryable<Sample> GetSamples([Service] AppDbContext context) =>
        context.Samples;
}
