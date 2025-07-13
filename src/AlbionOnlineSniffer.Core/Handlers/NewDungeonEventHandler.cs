using System;
using System.Numerics;
using System.Threading.Tasks;
using AlbionOnlineSniffer.Core.Models.Events;

namespace AlbionOnlineSniffer.Core.Handlers
{
    public class NewDungeonEventHandler
    {
        private readonly IDungeonsManager _dungeonsManager;
        public event Action<NewDungeonParsedData>? OnDungeonParsed;

        public NewDungeonEventHandler(IDungeonsManager dungeonsManager)
        {
            _dungeonsManager = dungeonsManager;
        }

        public Task HandleAsync(NewDungeonEvent value)
        {
            _dungeonsManager.AddDungeon(value.Id, value.Type, value.Position, value.Charges);

            OnDungeonParsed?.Invoke(new NewDungeonParsedData
            {
                Id = value.Id,
                Type = value.Type,
                Position = value.Position,
                Charges = value.Charges
            });

            return Task.CompletedTask;
        }
    }

    public class NewDungeonParsedData
    {
        public string Id { get; set; }
        public int Type { get; set; }
        public Vector2 Position { get; set; }
        public int Charges { get; set; }
    }

    public interface IDungeonsManager
    {
        void AddDungeon(int id, string type, System.Numerics.Vector2 position, int charges);
        void Remove(int id);
        void Clear();
    }
} 