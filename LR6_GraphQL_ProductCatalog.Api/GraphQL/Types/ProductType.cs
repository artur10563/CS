using LR6_GraphQL_ProductCatalog.Api.Domain;
using LR6_GraphQL_ProductCatalog.Api.GraphQL.Types;
using LR6_GraphQL_ProductCatalog.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

public class ProductType : ObjectType<Product>
{
    protected override void Configure(IObjectTypeDescriptor<Product> descriptor)
    {
        descriptor.BindFieldsExplicitly();

        descriptor.Field(p => p.Id);
        descriptor.Field(p => p.Name);

        descriptor.Field(p => p.Translations)
            .Type<ListType<TranslationType>>()
            .UseFiltering<TranslationFilterType>()
            .ResolveWith<ProductResolvers>(r => r.GetTranslations(default!, default!));
    }
}

public class ProductResolvers
{
    public IQueryable<Translations> GetTranslations(
        [Parent] Product product,
        [Service] AppDbContext db
       )
    {
        var q = db.Translations
            .Include(t => t.Language)
            .Where(t => t.ProductId == product.Id);

        return q;
    }
}