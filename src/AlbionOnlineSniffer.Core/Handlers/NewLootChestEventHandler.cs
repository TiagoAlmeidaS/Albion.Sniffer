using System;
using System.Numerics;
using System.Threading.Tasks;
using AlbionOnlineSniffer.Core.Models.Events;

namespace AlbionOnlineSniffer.Core.Handlers
{
    /// <summary>
    /// Handler para eventos de novos loot chests detectados no protocolo Photon.
    /// </summary>
    public class NewLootChestEventHandler
    {
        private readonly ILootChestsManager _worldChestManager;

        public event Action<NewLootChestParsedData>? OnLootChestParsed;

        public NewLootChestEventHandler(ILootChestsManager worldChestManager)
        {
            _worldChestManager = worldChestManager;
        }

        public Task HandleAsync(NewLootChestEvent value)
        {
            _worldChestManager.AddWorldChest(value.Id, value.Position, value.Name, value.EnchLvl);

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

    public interface ILootChestsManager
    {
        void AddWorldChest(int id, System.Numerics.Vector2 position, string name, int enchLvl);
        void Remove(int id);
        void Clear();
        int GetCharge(string name, int enchLvl);
    }
} 