using System;
using System.Threading.Tasks;

namespace AlbionOnlineSniffer.Core.Handlers
{
    public class WispGateOpenedEventHandler
    {
        private readonly IGatedWispsHandler _wispInGateHandler;
        public event Action<WispGateOpenedParsedData>? OnWispGateOpenedParsed;

        public WispGateOpenedEventHandler(IGatedWispsHandler wispInGateHandler)
        {
            _wispInGateHandler = wispInGateHandler;
        }

        public Task HandleAsync(WispGateOpenedEvent value)
        {
            if (value.isCollected)
            {
                _wispInGateHandler.Remove(value.Id);
            }

            OnWispGateOpenedParsed?.Invoke(new WispGateOpenedParsedData
            {
                Id = value.Id,
                IsCollected = value.isCollected
            });

            return Task.CompletedTask;
        }
    }

    public class WispGateOpenedParsedData
    {
        public string Id { get; set; }
        public bool IsCollected { get; set; }
    }

    public interface IGatedWispsHandler
    {
        void Remove(string id);
    }
} 