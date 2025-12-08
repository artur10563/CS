using Data.Infrastructure;
using LR6_GraphQL_ProductCatalog.Api.Domain;

namespace LR6_Api.Products;

public sealed record AddProductInput(string Name);

public sealed record AddProductTranslationInput(
    [ID<Product>] int ProductId,
    [ID<Language>] int LanguageId,
    string TranslationText);

[MutationType]
public static class ProductMutations
{
    [Error<ArgumentException>]
    public static async Task<Product> AddProduct(
        AppDbContext dbContext,
        AddProductInput input)
    {
        if (string.IsNullOrEmpty(input.Name))
        {
            throw new ArgumentException("Product name is required");
        }

        var product = new Product()
        {
            Name = input.Name
        };

        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        return product;
    }

    [Error<ArgumentException>]
    public static async Task<LR6_GraphQL_ProductCatalog.Api.Domain.Translations> AddProductTranslation(
        AppDbContext dbContext,
        AddProductTranslationInput input)
    {
        var product = dbContext.Products.FirstOrDefault(p => p.Id == input.ProductId) ??
                      throw new ArgumentException("Product not found");

        var language = dbContext.Languages.FirstOrDefault(l => l.Id == input.LanguageId) ??
                       throw new ArgumentException("Language not found");

        if (string.IsNullOrEmpty(input.TranslationText))
            throw new ArgumentException("Translation text is required");

        var translation = new LR6_GraphQL_ProductCatalog.Api.Domain.Translations()
        {
            ProductId = product.Id,
            LanguageId = language.Id,
            Text = input.TranslationText
        };

        dbContext.Translations.Add(translation);
        await dbContext.SaveChangesAsync();

        return translation;
    }
}