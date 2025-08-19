using System;

namespace AlbionOnlineSniffer.Core.Pipeline;

/// <summary>
/// Represents an item in the event processing pipeline
/// </summary>
public class PipelineItem
{
    /// <summary>
    /// Type of the event
    /// </summary>
    public string EventType { get; }
    
    /// <summary>
    /// Event data
    /// </summary>
    public object EventData { get; }
    
    /// <summary>
    /// Timestamp when the item was created
    /// </summary>
    public DateTime CreatedAt { get; }
    
    /// <summary>
    /// Correlation ID for tracking
    /// </summary>
    public string CorrelationId { get; }
    
    /// <summary>
    /// Priority of the event (higher = more important)
    /// </summary>
    public int Priority { get; }
    
    /// <summary>
    /// Initializes a new instance of the PipelineItem class
    /// </summary>
    /// <param name="eventType">Type of the event</param>
    /// <param name="eventData">Event data</param>
    /// <param name="priority">Priority of the event</param>
    /// <param name="correlationId">Correlation ID for tracking</param>
    public PipelineItem(string eventType, object eventData, int priority = 5, string? correlationId = null)
    {
        EventType = eventType ?? throw new ArgumentNullException(nameof(eventType));
        EventData = eventData ?? throw new ArgumentNullException(nameof(eventData));
        Priority = priority;
        CorrelationId = correlationId ?? Guid.NewGuid().ToString();
        CreatedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Gets the age of the item in milliseconds
    /// </summary>
    public long AgeMs => (long)(DateTime.UtcNow - CreatedAt).TotalMilliseconds;
    
    /// <summary>
    /// Creates a copy of this item with updated data
    /// </summary>
    /// <param name="newEventData">New event data</param>
    /// <returns>New pipeline item with updated data</returns>
    public PipelineItem WithUpdatedData(object newEventData)
    {
        return new PipelineItem(EventType, newEventData, Priority, CorrelationId);
    }
}
