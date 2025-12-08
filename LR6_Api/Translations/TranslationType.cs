using LR6_GraphQL_ProductCatalog.Api.Domain;
using GreenDonut.Data;
using HotChocolate.Execution.Processing;
using LR6_Api.Languages.DataLoaders;
using LR6_Api.Products.DataLoaders;
using TranslationsEntity = LR6_GraphQL_ProductCatalog.Api.Domain.Translations;

namespace LR6_Api.Translations;

[ObjectType<TranslationsEntity>]
public static partial class TranslationType
{
    static partial void Configure(IObjectTypeDescriptor<TranslationsEntity> descriptor)
    {
        descriptor.BindFieldsExplicitly();

        descriptor.Field(x => x.Id).Type<NonNullType<IntType>>();
        descriptor.Field(x => x.Text).Type<NonNullType<StringType>>();
        descriptor.Field(x => x.LanguageId).ID<Language>().Type<IdType>();
        descriptor.Field(x => x.ProductId).ID<Product>().Type<IdType>();

        descriptor
            .Field(x => x.Language)
            .Resolve(async ctx =>
            {
                var translation = ctx.Parent<TranslationsEntity>();
                var dataLoader = ctx.DataLoader<ILanguageByIdDataLoader>();
                var selection = ctx.Selection;
                return await dataLoader
                    .Select(selection)
                    .LoadAsync(translation.LanguageId, ctx.RequestAborted);
            });

        descriptor
            .Field(x => x.Product)
            .Resolve(async ctx =>
            {
                var translation = ctx.Parent<TranslationsEntity>();
                var dataLoader = ctx.DataLoader<IProductByIdDataLoader>();
                var selection = ctx.Selection;
                return await dataLoader
                    .Select(selection)
                    .LoadAsync(translation.ProductId, ctx.RequestAborted);
            });
    }
}

