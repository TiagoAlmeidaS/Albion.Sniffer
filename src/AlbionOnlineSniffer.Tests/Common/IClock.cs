namespace AlbionOnlineSniffer.Tests.Common;

/// <summary>
/// Abstração para obter o tempo atual, permitindo testes determinísticos
/// </summary>
public interface IClock
{
    DateTimeOffset UtcNow { get; }
    DateTime UtcNowDateTime { get; }
}

/// <summary>
/// Implementação real do relógio usando o tempo do sistema
/// </summary>
public class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
    public DateTime UtcNowDateTime => DateTime.UtcNow;
}

/// <summary>
/// Implementação fake do relógio para testes
/// </summary>
public class FakeClock : IClock
{
    private DateTimeOffset _currentTime;

    public FakeClock(DateTimeOffset? initialTime = null)
    {
        _currentTime = initialTime ?? DateTimeOffset.Parse("2025-01-01T00:00:00Z");
    }

    public DateTimeOffset UtcNow => _currentTime;
    public DateTime UtcNowDateTime => _currentTime.UtcDateTime;

    public void Advance(TimeSpan timeSpan)
    {
        _currentTime = _currentTime.Add(timeSpan);
    }

    public void SetTime(DateTimeOffset time)
    {
        _currentTime = time;
    }
}