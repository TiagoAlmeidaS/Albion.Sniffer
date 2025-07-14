using System;
using System.Numerics;
using System.Threading.Tasks;
using AlbionOnlineSniffer.Core.Interfaces;
using AlbionOnlineSniffer.Core.Models.Events;

namespace AlbionOnlineSniffer.Core.Handlers
{
    public class NewGatedWispEventHandler
    {
        private readonly IGatedWispsManager _wispInGateManager;
        public event Action<NewGatedWispParsedData>? OnGatedWispParsed;

        public NewGatedWispEventHandler(IGatedWispsManager wispInGateManager)
        {
            _wispInGateManager = wispInGateManager;
        }

        public Task HandleAsync(NewGatedWispEvent value)
        {
            if (!value.IsCollected)
                _wispInGateManager.AddWispInGate(int.TryParse(value.Id, out var id) ? id : 0, value.Position);

            OnGatedWispParsed?.Invoke(new NewGatedWispParsedData
            {
                Id = value.Id,
                Position = value.Position,
                IsCollected = value.IsCollected
            });

            return Task.CompletedTask;
        }
    }

    public class NewGatedWispParsedData
    {
        public string Id { get; set; }
        public Vector2 Position { get; set; }
        public bool IsCollected { get; set; }
    }
} 