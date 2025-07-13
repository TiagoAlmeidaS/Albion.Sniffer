using System;
using System.Threading.Tasks;

namespace AlbionOnlineSniffer.Queue.Interfaces
{
    public interface IQueuePublisher : IDisposable
    {
        Task PublishAsync(string topic, object message);
    }
} 