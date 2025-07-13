using System;
using System.Numerics;
using System.Threading.Tasks;

namespace AlbionOnlineSniffer.Core.Handlers
{
    /// <summary>
    /// Handler para eventos de novos loot chests detectados no protocolo Photon.
    /// </summary>
    public class NewLootChestEventHandler
    {
        private readonly ILootChestsHandler _worldChestHandler;

        public event Action<NewLootChestParsedData>? OnLootChestParsed;

        public NewLootChestEventHandler(ILootChestsHandler worldChestHandler)
        {
            _worldChestHandler = worldChestHandler;
        }

        public Task HandleAsync(NewLootChestEvent value)
        {
            _worldChestHandler.AddWorldChest(value.Id, value.Position, value.Name, value.EnchLvl);

            OnLootChestParsed?.Invoke(new NewLootChestParsedData
            {
                Id = value.Id,
                Position = value.Position,
                Name = value.Name,
                EnchLvl = value.EnchLvl
            });

            return Task.CompletedTask;
        }
    }

    public class NewLootChestParsedData
    {
        public string Id { get; set; }
        public Vector2 Position { get; set; }
        public string Name { get; set; }
        public int EnchLvl { get; set; }
    }

    public interface ILootChestsHandler
    {
        void AddWorldChest(string id, Vector2 position, string name, int enchLvl);
    }
} 