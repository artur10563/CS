using Data.Infrastructure;
using GreenDonut.Data;
using LR6_GraphQL_ProductCatalog.Api.Domain;
using Microsoft.EntityFrameworkCore;
using TranslationsEntity = LR6_GraphQL_ProductCatalog.Api.Domain.Translations;

namespace LR6_Api.Languages.DataLoaders;

public static class LanguageDataLoaders
{
    [DataLoader]
    public static async Task<IReadOnlyDictionary<int, Language>> LanguageByIdAsync(
        IReadOnlyList<int> ids,
        AppDbContext dbContext,
        ISelectorBuilder selector,
        CancellationToken cancellationToken)
    {
        return await dbContext.Languages
            .AsNoTracking()
            .Where(l => ids.Contains(l.Id))
            .Select(l => l.Id, selector)
            .ToDictionaryAsync(l => l.Id, cancellationToken);
    }

    [DataLoader]
    public static async Task<IReadOnlyDictionary<int, TranslationsEntity[]>> TranslationsByLanguageIdAsync(
        IReadOnlyList<int> languageIds,
        AppDbContext dbContext,
        ISelectorBuilder selector,
        CancellationToken cancellationToken)
    {
        return await dbContext.Languages
            .AsNoTracking()
            .Where(l => languageIds.Contains(l.Id))
            .Select(l => l.Id, l => l.Translations, selector)
            .ToDictionaryAsync(r => r.Key, r => r.Value.ToArray(), cancellationToken);
    }
}

