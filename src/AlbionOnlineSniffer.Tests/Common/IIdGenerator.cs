namespace AlbionOnlineSniffer.Tests.Common;

/// <summary>
/// Abstração para gerar IDs únicos, permitindo testes determinísticos
/// </summary>
public interface IIdGenerator
{
    Guid NewGuid();
    string NewUlid();
}

/// <summary>
/// Implementação real do gerador de IDs
/// </summary>
public class SystemIdGenerator : IIdGenerator
{
    public Guid NewGuid() => Guid.NewGuid();
    public string NewUlid() => Ulid.NewUlid().ToString();
}

/// <summary>
/// Implementação fake do gerador de IDs para testes determinísticos
/// </summary>
public class FakeIdGenerator : IIdGenerator
{
    private readonly Queue<Guid> _guidQueue = new();
    private readonly Queue<string> _ulidQueue = new();
    private int _guidCounter = 0;
    private int _ulidCounter = 0;

    public FakeIdGenerator(params Guid[] predefinedGuids)
    {
        foreach (var guid in predefinedGuids)
        {
            _guidQueue.Enqueue(guid);
        }
    }

    public Guid NewGuid()
    {
        if (_guidQueue.Count > 0)
            return _guidQueue.Dequeue();

        // Gera GUIDs determinísticos incrementais
        _guidCounter++;
        var bytes = new byte[16];
        BitConverter.GetBytes(_guidCounter).CopyTo(bytes, 0);
        return new Guid(bytes);
    }

    public string NewUlid()
    {
        if (_ulidQueue.Count > 0)
            return _ulidQueue.Dequeue();

        // Gera ULIDs determinísticos
        _ulidCounter++;
        return $"01HK{_ulidCounter:D10}AAAAAAAAAA";
    }

    public void EnqueueGuid(Guid guid)
    {
        _guidQueue.Enqueue(guid);
    }

    public void EnqueueUlid(string ulid)
    {
        _ulidQueue.Enqueue(ulid);
    }
}