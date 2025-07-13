using System;
using System.Numerics;
using System.Threading.Tasks;

namespace AlbionOnlineSniffer.Core.Handlers
{
    public class NewFishingZoneEventHandler
    {
        private readonly IFishNodesHandler _fishZoneHandler;
        public event Action<NewFishingZoneParsedData>? OnFishingZoneParsed;

        public NewFishingZoneEventHandler(IFishNodesHandler fishZoneHandler)
        {
            _fishZoneHandler = fishZoneHandler;
        }

        public Task HandleAsync(NewFishingZoneEvent value)
        {
            _fishZoneHandler.AddFishZone(value.Id, value.Position, value.Size, value.RespawnCount);

            OnFishingZoneParsed?.Invoke(new NewFishingZoneParsedData
            {
                Id = value.Id,
                Position = value.Position,
                Size = value.Size,
                RespawnCount = value.RespawnCount
            });

            return Task.CompletedTask;
        }
    }

    public class NewFishingZoneParsedData
    {
        public string Id { get; set; }
        public Vector2 Position { get; set; }
        public int Size { get; set; }
        public int RespawnCount { get; set; }
    }

    public interface IFishNodesHandler
    {
        void AddFishZone(string id, Vector2 position, int size, int respawnCount);
    }
} 