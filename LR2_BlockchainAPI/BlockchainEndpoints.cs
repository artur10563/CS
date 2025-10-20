using Shared;
using Shared.DTOs;

namespace LR2_BlockchainAPI;

public static class BlockchainEndpoints
{
    public static IEndpointRouteBuilder RegisterBlockchainEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/chain", (BlockchainPrototype blockchain) =>
        {
            var chain = blockchain.GetChain();
            return Results.Ok(new { chain, length = chain.Count });
        });

        app.MapPost("/transactions/new", (BlockchainPrototype blockchain,
            CreateTransactionDTO transaction) =>
        {
            if (string.IsNullOrWhiteSpace(transaction.Sender) || string.IsNullOrWhiteSpace(transaction.Recipient))
                return Results.BadRequest("Invalid transaction");

            var blockIndex = blockchain.AddTransaction(BlockchainPrototype.Transaction.Create(transaction.Sender, transaction.Recipient, transaction.Amount));
            return Results.Created($"/transactions/new", new { message = $"Transaction will be added to Block {blockIndex}" });
        });

        app.MapPost("/mine", (BlockchainPrototype blockchain) =>
        {
            var proof = blockchain.ProofOfWork();

            blockchain.AddTransaction(BlockchainPrototype.Transaction.Create("0", Program.NodeIdentifier, 1));

            var newBlock = blockchain.AddBlock(proof);

            return Results.Ok(new
            {
                message = "New block forged",
                block = newBlock
            });
        });

        return app;
    }
}