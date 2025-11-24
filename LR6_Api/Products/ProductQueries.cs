using Data.Infrastructure;
using GreenDonut.Data;
using HotChocolate.Execution.Processing;
using LR6_GraphQL_ProductCatalog.Api.Domain;
using Microsoft.EntityFrameworkCore;
using LR6_Api.Products.DataLoaders;

namespace LR6_Api.Products;

# region Node required for code generation

[Node]
public class PingNode
{
    public int Id { get; set; }
}

[QueryType]
public static class DummyNodeQueries
{
    [NodeResolver]
    public static PingNode PingNode(PingNode pingNode)
    {
        return pingNode;
    }
}

#endregion

[QueryType]
public static class ProductQueries
{
    public static async Task<Product?> GetProductByIdAsync(
        int id,
        IProductByIdDataLoader productById,
        ISelection selection,
        CancellationToken cancellationToken)
    {
        return await productById.Select(selection).LoadAsync(id, cancellationToken);
    }

    public static async Task<IEnumerable<Product>> GetProductsByIdAsync(
        int[] ids,
        IProductByIdDataLoader productById,
        ISelection selection,
        CancellationToken cancellationToken)
    {
        return await productById.Select(selection).LoadRequiredAsync(ids, cancellationToken);
    }
}