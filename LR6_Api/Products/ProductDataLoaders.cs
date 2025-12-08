using Data.Infrastructure;
using GreenDonut.Data;
using LR6_GraphQL_ProductCatalog.Api.Domain;
using Microsoft.EntityFrameworkCore;
using TranslationsEntity = LR6_GraphQL_ProductCatalog.Api.Domain.Translations;

namespace LR6_Api.Products.DataLoaders;

public static class ProductDataLoaders
{
    [DataLoader]
    public static async Task<IReadOnlyDictionary<int, Product>> ProductByIdAsync(
        IReadOnlyList<int> ids,
        AppDbContext dbContext,
        ISelectorBuilder selector,
        CancellationToken cancellationToken)
    {
        return await dbContext.Products
            .AsNoTracking()
            .Where(s => ids.Contains(s.Id))
            .Select(s => s.Id, selector)
            .ToDictionaryAsync(s => s.Id, cancellationToken);
    }

    [DataLoader]
    public static async Task<IReadOnlyDictionary<int, TranslationsEntity[]>> TranslationByProductIdAsync(
        IReadOnlyList<int> productIds,
        AppDbContext dbContext,
        ISelectorBuilder selector,
        CancellationToken cancellationToken)
    {
        return await dbContext.Products
            .AsNoTracking()
            .Where(s => productIds.Contains(s.Id))
            .Select(s => s.Id, s => s.Translations, selector)
            .ToDictionaryAsync(r => r.Key, r => r.Value.ToArray(), cancellationToken);
    }
}