using Data.Infrastructure;
using LR6_GraphQL_ProductCatalog.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace LR6_Api.Languages;

[QueryType]
public static class LanguageQueries
{
    [UsePaging]
    public static async Task<IEnumerable<Language>> GetLanguages(
        AppDbContext dbContext)
    {
        return dbContext.Languages.AsNoTracking().OrderBy(s => s.Name).ThenBy(s => s.Id);
    }
}