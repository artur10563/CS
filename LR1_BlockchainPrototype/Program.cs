namespace LR1_BlockchainPrototype;


public sealed record Block();
public sealed record Transaction(string sender, string recipient, decimal amount);

public class BlockchainPrototype
{
    public readonly List<Block> blockchain = [];
    public readonly List<Transaction> transactions = [];

    public void CreateTransaction(Transaction transaction)
    {
        transactions.Add(transaction);
        
        
    }
}

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");
    }
}