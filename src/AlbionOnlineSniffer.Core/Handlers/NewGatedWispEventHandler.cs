using System;
using System.Numerics;
using System.Threading.Tasks;

namespace AlbionOnlineSniffer.Core.Handlers
{
    public class NewGatedWispEventHandler
    {
        private readonly IGatedWispsHandler _wispInGateHandler;
        public event Action<NewGatedWispParsedData>? OnGatedWispParsed;

        public NewGatedWispEventHandler(IGatedWispsHandler wispInGateHandler)
        {
            _wispInGateHandler = wispInGateHandler;
        }

        public Task HandleAsync(NewGatedWispEvent value)
        {
            if (!value.isCollected)
                _wispInGateHandler.AddWispInGate(value.Id, value.Position);

            OnGatedWispParsed?.Invoke(new NewGatedWispParsedData
            {
                Id = value.Id,
                Position = value.Position,
                IsCollected = value.isCollected
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

    public interface IGatedWispsHandler
    {
        void AddWispInGate(string id, Vector2 position);
    }
} 