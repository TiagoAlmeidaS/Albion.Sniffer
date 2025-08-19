using System.Collections.Concurrent;

namespace AlbionOnlineSniffer.Tests.Common.Fakes;

/// <summary>
/// Publisher em memória para testes
/// </summary>
public class InMemoryPublisher : IEventPublisher
{
    private readonly ConcurrentBag<PublishedMessage> _messages = new();
    private readonly List<Func<PublishedMessage, Task>> _subscribers = new();
    private int _publishCount = 0;
    private bool _shouldFail = false;
    private Exception? _failureException;

    public IReadOnlyCollection<PublishedMessage> PublishedMessages => _messages.ToList().AsReadOnly();
    public int PublishCount => _publishCount;

    public async Task PublishAsync<T>(string topic, T message, CancellationToken cancellationToken = default)
        where T : class
    {
        if (_shouldFail && _failureException != null)
        {
            throw _failureException;
        }

        var publishedMessage = new PublishedMessage(
            Topic: topic,
            Message: message,
            MessageType: typeof(T),
            Timestamp: DateTimeOffset.UtcNow,
            Headers: new Dictionary<string, string>
            {
                ["MessageType"] = typeof(T).Name,
                ["PublisherId"] = Guid.NewGuid().ToString()
            }
        );

        _messages.Add(publishedMessage);
        Interlocked.Increment(ref _publishCount);

        // Notifica subscribers
        foreach (var subscriber in _subscribers)
        {
            await subscriber(publishedMessage);
        }
    }

    public void Subscribe(Func<PublishedMessage, Task> handler)
    {
        _subscribers.Add(handler);
    }

    public void Clear()
    {
        _messages.Clear();
        _publishCount = 0;
    }

    public void SimulateFailure(Exception exception)
    {
        _shouldFail = true;
        _failureException = exception;
    }

    public void ResetFailure()
    {
        _shouldFail = false;
        _failureException = null;
    }

    public bool HasPublishedMessage<T>(string topic) where T : class
    {
        return _messages.Any(m => m.Topic == topic && m.MessageType == typeof(T));
    }

    public T? GetLastPublishedMessage<T>(string topic) where T : class
    {
        return _messages
            .Where(m => m.Topic == topic && m.MessageType == typeof(T))
            .OrderByDescending(m => m.Timestamp)
            .Select(m => m.Message as T)
            .FirstOrDefault();
    }

    public List<T> GetAllPublishedMessages<T>(string? topic = null) where T : class
    {
        var query = _messages.Where(m => m.MessageType == typeof(T));
        
        if (!string.IsNullOrEmpty(topic))
            query = query.Where(m => m.Topic == topic);

        return query
            .Select(m => m.Message as T)
            .Where(m => m != null)
            .Cast<T>()
            .ToList();
    }
}

public record PublishedMessage(
    string Topic,
    object Message,
    Type MessageType,
    DateTimeOffset Timestamp,
    Dictionary<string, string> Headers
);

/// <summary>
/// Interface para publicação de eventos
/// </summary>
public interface IEventPublisher
{
    Task PublishAsync<T>(string topic, T message, CancellationToken cancellationToken = default) where T : class;
}