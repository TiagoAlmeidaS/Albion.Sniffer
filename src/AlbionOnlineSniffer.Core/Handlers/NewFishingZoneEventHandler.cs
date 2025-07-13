using System;
using System.Numerics;
using System.Threading.Tasks;
using AlbionOnlineSniffer.Core.Models.Events;

namespace AlbionOnlineSniffer.Core.Handlers
{
    public class NewFishingZoneEventHandler
    {
        private readonly IFishNodesManager _fishZoneManager;
        public event Action<NewFishingZoneParsedData>? OnFishingZoneParsed;

        public NewFishingZoneEventHandler(IFishNodesManager fishZoneManager)
        {
            _fishZoneManager = fishZoneManager;
        }

        public Task HandleAsync(NewFishingZoneEvent value)
        {
            _fishZoneManager.AddFishZone(value.Id, value.Position, value.Size, value.RespawnCount);

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

    public interface IFishNodesManager
    {
        void AddFishZone(int id, System.Numerics.Vector2 position, int size, int respawnCount);
        void Remove(int id);
        void Clear();
    }
} 