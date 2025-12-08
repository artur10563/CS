using HotChocolate.Execution.Processing;
using LR6_GraphQL_ProductCatalog.Api.Domain;
using GreenDonut.Data;
using LR6_Api.Products.DataLoaders;
using LR6_Api.Translations;

namespace LR6_Api.Products;

[ObjectType<Product>]
public static partial class ProductType
{
    static partial void Configure(IObjectTypeDescriptor<Product> descriptor)
    {
        descriptor.Field(x => x.Id).ID().Type<IdType>();

        descriptor.Field("intId")
            .Type<NonNullType<IntType>>()
            .Resolve(ctx => ctx.Parent<Product>().Id);

        descriptor
            .Field(t => t.Name)
            .ParentRequires(nameof(Product.Name));

        descriptor
            .Field(x => x.Translations)
            .Resolve(async ctx =>
            {
                var product = ctx.Parent<Product>();
                var dataLoader = ctx.DataLoader<ITranslationByProductIdDataLoader>();
                var selection = ctx.Selection;
                return await dataLoader
                    .Select(selection)
                    .LoadRequiredAsync(product.Id, ctx.RequestAborted);
            });
    }
}