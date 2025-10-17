using Newtonsoft.Json;
using Shared;

namespace LR1_BlockchainPrototype;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine($"1 - Run new Blockchain \n2 - Load from Json File\n=> ");
        if (!int.TryParse(Console.ReadLine(), out var n))
        {
            Console.WriteLine("Invalid input, defaulting to 0");
        }

        BlockchainPrototype blockchain;
        int blocksToMine;

        switch (n)
        {
            case 1:
            {
                blockchain = new BlockchainPrototype();
                blocksToMine = 5;
                break;
            }
            case 2:
                Console.Write("Path to JSON file: ");
                var filePath = Console.ReadLine();

                try
                {
                    blockchain = BlockchainPrototype.LoadFromJson(filePath!);
                    Console.WriteLine("Blockchain loaded successfully!");
                    blocksToMine = 2;

                    foreach (var block in blockchain.GetChain())
                        Console.WriteLine(JsonConvert.SerializeObject(block, Formatting.Indented));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading blockchain: {ex.Message}");
                    return;
                }

                break;

            default:
                Console.WriteLine("Invalid option");
                return;
        }

        MineBlocks(blockchain, blocksToMine);

        var json = JsonConvert.SerializeObject(blockchain.GetChain(), Formatting.Indented);
        File.WriteAllText("save.json", json);
        Console.WriteLine("Blockchain saved to save.json");

        Console.Read();
    }

    static void MineBlocks(BlockchainPrototype blockchain, int count)
    {
        int startIndex = blockchain.GetChain().Count;

        for (int i = 0; i < count; i++)
        {
            Console.WriteLine($"\n=== Mining Block #{startIndex + i + 1} ===");

            blockchain.AddTransaction(new BlockchainPrototype.Transaction(
                sender: $"User{i}",
                recipient: $"User{i + 1}",
                amount: 10 + i
            ));

            var proof = blockchain.ProofOfWork();

            var newBlock = blockchain.AddBlock(proof);
            Console.WriteLine($"Block #{newBlock.Index} mined successfully!");
            Console.WriteLine($"Hash: {BlockchainPrototype.Hash(newBlock)}");
        }
    }
}