using GreenDonut.Data;
using LR6_GraphQL_ProductCatalog.Api.Domain;
using LR6_GraphQL_ProductCatalog.Api.GraphQL.Types;
using LR6_GraphQL_ProductCatalog.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

public class ProductsQuery
{
    [UseFirstOrDefault]
    [UseProjection]
    [UseFiltering]
    [GraphQLType(typeof(ProductType))]
    public IQueryable<Product> GetProductById(int id, [Service] AppDbContext db)
        => db.Products.Where(x => x.Id == id);
}
