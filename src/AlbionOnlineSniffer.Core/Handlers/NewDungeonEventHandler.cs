using System;
using System.Numerics;
using System.Threading.Tasks;

namespace AlbionOnlineSniffer.Core.Handlers
{
    public class NewDungeonEventHandler
    {
        private readonly IDungeonsHandler _dungeonsHandler;
        public event Action<NewDungeonParsedData>? OnDungeonParsed;

        public NewDungeonEventHandler(IDungeonsHandler dungeonsHandler)
        {
            _dungeonsHandler = dungeonsHandler;
        }

        public Task HandleAsync(NewDungeonEvent value)
        {
            _dungeonsHandler.AddDungeon(value.Id, value.Type, value.Position, value.Charges);

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

    public interface IDungeonsHandler
    {
        void AddDungeon(string id, int type, Vector2 position, int charges);
    }
} 