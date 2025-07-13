using System;
using System.Threading.Tasks;
using AlbionOnlineSniffer.Core.Interfaces;
using AlbionOnlineSniffer.Core.Models.Events;

namespace AlbionOnlineSniffer.Core.Handlers
{
    public class WispGateOpenedEventHandler
    {
        private readonly IGatedWispsManager _wispInGateManager;
        public event Action<WispGateOpenedParsedData>? OnWispGateOpenedParsed;

        public WispGateOpenedEventHandler(IGatedWispsManager wispInGateManager)
        {
            _wispInGateManager = wispInGateManager;
        }

        public Task HandleAsync(WispGateOpenedEvent value)
        {
            if (value.IsCollected)
            {
                _wispInGateManager.Remove(value.Id);
            }

            OnWispGateOpenedParsed?.Invoke(new WispGateOpenedParsedData
            {
                Id = value.Id,
                IsCollected = value.IsCollected
            });

            return Task.CompletedTask;
        }
    }

    public class WispGateOpenedParsedData
    {
        public string Id { get; set; }
        public bool IsCollected { get; set; }
    }
} 