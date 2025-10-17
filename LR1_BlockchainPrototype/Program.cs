using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace LR1_BlockchainPrototype;

public class BlockchainPrototype
{
    public sealed record Transaction(string sender, string recipient, decimal amount);

    public sealed record Block(int Index, long Timestamp, IEnumerable<Transaction> Transactions, long Proof, string PreviousHash)
    {
        public static Block CreateGenesisBlock()
        {
            return new Block(1, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), [], 100, "1");
        }
    };

    private readonly ICollection<Block> _chain = [];
    private ICollection<Transaction> _transactions = [];
    private Block LastBlock => _chain.Last();

    public BlockchainPrototype()
    {
        _chain.Add(Block.CreateGenesisBlock());
    }

    public BlockchainPrototype(ICollection<Block> initialChain)
    {
        _chain = initialChain;

        if (_chain.Count == 0) _chain.Add(Block.CreateGenesisBlock());
    }


    public int AddTransaction(Transaction transaction)
    {
        _transactions.Add(transaction);

        return LastBlock.Index;
    }

    public Block AddBlock(long proof, string? previousHash = null)
    {
        var block = new Block(
            _chain.Count + 1,
            DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            this._transactions,
            proof,
            previousHash ?? Hash(LastBlock)
        );

        _transactions = [];

        _chain.Add(block);
        return block;
    }

    public long ProofOfWork()
    {
        long currentProof = 0;

        while (!IsValidPoW(LastBlock.Proof, currentProof))
        {
            currentProof++;
        }

        return currentProof;
    }

    //Знайдіть таке число p, що при хешуванні з рішенням
    //попереднього блоку створює хеш з чотирма заголовними нулями
    private static bool IsValidPoW(long lastProof, long proof)
    {
        var guess = $"{lastProof}{proof}";
        var guessBytes = Encoding.UTF8.GetBytes(guess);

        var hashBytes = SHA256.HashData(guessBytes);
        var hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();

        return hash.StartsWith("0000");
    }

    public static string Hash(object obj)
    {
        var json = JsonConvert.SerializeObject(obj, Formatting.None,
            new JsonSerializerSettings
            {
                StringEscapeHandling = StringEscapeHandling.EscapeNonAscii
            });

        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(json));

        return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
    }

    public static BlockchainPrototype LoadFromJson(string path)
    {
        if (!path.EndsWith("json"))
            throw new FileNotFoundException("File is not a json file");
        if (!File.Exists(path))
            throw new FileNotFoundException("File not found", path);


        var json = File.ReadAllText(path);
        var blocks = JsonConvert.DeserializeObject<List<Block>>(json);

        return new BlockchainPrototype(blocks);
    }

    public IReadOnlyList<Block> GetChain() => _chain.ToList().AsReadOnly();
}

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