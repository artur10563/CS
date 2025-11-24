using HotChocolate.Execution.Processing;
using LR6_GraphQL_ProductCatalog.Api.Domain;
using GreenDonut.Data;
using LR6_Api.Products.DataLoaders;

namespace LR6_Api.Products;

[ObjectType<Product>]
public static partial class ProductType
{
    static partial void Configure(IObjectTypeDescriptor<Product> descriptor)
    {
        descriptor
            .Field(t => t.Name)
            .ParentRequires(nameof(Product.Name));
    }

    [BindMember(nameof(Product.Translations))]
    internal static async Task<IEnumerable<Translations>> GetTranslationsAsync(
        [Parent] Product product,
        ITranslationByProductIdDataLoader translationsByProductId,
        ISelection selection,
        CancellationToken cancellationToken)
    {
        return await translationsByProductId
            .Select(selection)
            .LoadRequiredAsync(product.Id, cancellationToken);
    }
}