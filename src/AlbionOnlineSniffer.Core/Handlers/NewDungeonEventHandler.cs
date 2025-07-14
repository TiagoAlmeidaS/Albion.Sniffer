using System;
using System.Numerics;
using System.Threading.Tasks;
using AlbionOnlineSniffer.Core.Models.Events;
using AlbionOnlineSniffer.Core.Interfaces;

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
            _dungeonsManager.AddDungeon(
                int.TryParse(value.Id, out var id) ? id : 0,
                value.Type.ToString(),
                value.Position,
                value.Charges
            );

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
} 