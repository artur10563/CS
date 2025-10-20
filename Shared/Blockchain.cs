using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Shared;

/// <summary>
/// Blockchain Specific Classes
/// </summary>
public partial class BlockchainPrototype
{
    public sealed record Transaction(string Sender, string Recipient, decimal Amount, long Timestamp)
    {
        public static Transaction Create(string sender, string recipient, decimal amount)
        {
            return new Transaction(sender, recipient, amount, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
        }
    };

    public sealed record Block(int Index, long Timestamp, IEnumerable<Transaction> Transactions, long Proof, string PreviousHash)
    {
        public static Block CreateGenesisBlock()
        {
            return new Block(1, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), [], 100, "1");
        }
    };

    public sealed record Node(string Address)
    {
        public static Node Create(string url)
        {
            return new Node(url);
        }
    }
}

public partial class BlockchainPrototype
{
    private List<Block> _chain = [];
    private List<Transaction> _transactions = [];
    private HashSet<Node> _nodes = [];
    private readonly object _lock = new();
    private Block LastBlock => _chain.Last();

    public BlockchainPrototype()
    {
        _chain.Add(Block.CreateGenesisBlock());
    }

    private BlockchainPrototype(IEnumerable<Block>? initialChain = null)
    {
        _chain = initialChain?.ToList() ?? [];

        if (_chain.Count == 0)
            _chain.Add(Block.CreateGenesisBlock());
    }

    public void AddNode(Node node)
    {
        _nodes.Add(node);
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

    private static bool IsValidChain(IEnumerable<Block> chain)
    {
        var blocks = chain.ToList();
        if (blocks.Count == 0) return false;

        for (var i = 1; i < blocks.Count; i++)
        {
            var lastBlock = blocks[i - 1];
            var block = blocks[i];

            if (block.PreviousHash != Hash(lastBlock)) return false;
            if (!IsValidPoW(lastBlock.Proof, block.Proof)) return false;
        }

        return true;
    }

    public async Task<bool> ResolveConflictsAsync()
    {
        using var httpClient = new HttpClient();

        var wasUpdated = false;

        foreach (var node in _nodes)
        {
            try
            {
                var response = await httpClient.GetAsync($"{node.Address}/chain");

                if (!response.IsSuccessStatusCode) continue;


                var json = await response.Content.ReadAsStringAsync();
                var root = JObject.Parse(json);
                var chain = root["chain"]?.ToObject<List<Block>>();

                if (chain?.Count > _chain.Count && IsValidChain(chain))
                {
                    _chain = chain;
                    wasUpdated = true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        return wasUpdated;
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