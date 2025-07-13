using System;
using System.Numerics;
using System.Threading.Tasks;
using AlbionOnlineSniffer.Core.Models.Events;
using AlbionOnlineSniffer.Core.Models;

namespace AlbionOnlineSniffer.Core.Handlers
{
    /// <summary>
    /// Handler para eventos de novos mobs detectados no protocolo Photon.
    /// </summary>
    public class NewMobEventHandler
    {
        private readonly IMobsManager _mobManager;

        public event Action<NewMobParsedData>? OnMobParsed;

        public NewMobEventHandler(IMobsManager mobManager)
        {
            _mobManager = mobManager;
        }

        public Task HandleAsync(NewMobEvent value)
        {
            _mobManager.AddMob(value.Id, value.TypeId, value.Position, value.Health, value.Charge);

            OnMobParsed?.Invoke(new NewMobParsedData
            {
                Id = value.Id,
                TypeId = value.TypeId,
                Position = value.Position,
                Health = value.Health,
                Charge = value.Charge
            });

            return Task.CompletedTask;
        }
    }

    public class NewMobParsedData
    {
        public string Id { get; set; }
        public int TypeId { get; set; }
        public Vector2 Position { get; set; }
        public int Health { get; set; }
        public int Charge { get; set; }
    }

    public interface IMobsManager
    {
        void AddMob(int id, int typeId, System.Numerics.Vector2 position, Health health, byte enchLvl);
        void UpdateMobPosition(int id, byte[] positionBytes, byte[] newPositionBytes, float speed, System.DateTime time);
        void SyncMobsPositions();
        void Remove(int id);
        void Clear();
        void UpdateMobCharge(int mobId, int charge);
        void UpdateHealth(int id, int health);
    }
} 