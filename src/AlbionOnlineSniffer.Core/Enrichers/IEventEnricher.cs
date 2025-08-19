using System.Threading;
using System.Threading.Tasks;
using AlbionOnlineSniffer.Options;

namespace AlbionOnlineSniffer.Core.Enrichers;

/// <summary>
/// Interface for event enrichers that transform event data
/// </summary>
public interface IEventEnricher
{
    /// <summary>
    /// Name of the enricher
    /// </summary>
    string Name { get; }
    
    /// <summary>
    /// Priority of the enricher (lower = higher priority)
    /// </summary>
    int Priority { get; }
    
    /// <summary>
    /// Whether this enricher is enabled
    /// </summary>
    bool IsEnabled { get; }
    
    /// <summary>
    /// Enriches an event with additional data or transformations
    /// </summary>
    /// <param name="eventData">The event data to enrich</param>
    /// <param name="profile">Current profile configuration</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Enriched event data</returns>
    Task<object> EnrichAsync(object eventData, ProfileOptions profile, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Checks if this enricher can process the given event type
    /// </summary>
    /// <param name="eventType">Type of the event</param>
    /// <returns>True if the enricher can process this event type</returns>
    bool CanProcess(string eventType);
}
