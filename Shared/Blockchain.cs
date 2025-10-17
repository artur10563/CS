using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace Shared;

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
    private readonly object _lock = new();
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
        lock (_lock)
        {
            _transactions.Add(transaction);
            return LastBlock.Index;
        }
    }

    public Block AddBlock(long proof, string? previousHash = null)
    {
        lock (_lock)
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
    }

    public long ProofOfWork()
    {
        long currentProof = 0;
        long lastProof;

        lock (_lock)
        {
            lastProof = LastBlock.Proof;
        }

        while (!IsValidPoW(lastProof, currentProof))
        {
            currentProof++;
        }

        return currentProof;
    }

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
        var blocks = JsonConvert.DeserializeObject<List<Block>>(json) ?? [];

        return new BlockchainPrototype(blocks);
    }

    public IReadOnlyList<Block> GetChain()
    {
        lock (_lock)
        {
            return _chain.ToList().AsReadOnly();
        }
    }
}