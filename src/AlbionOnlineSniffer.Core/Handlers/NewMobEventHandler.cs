using System;
using System.Numerics;
using System.Threading.Tasks;

namespace AlbionOnlineSniffer.Core.Handlers
{
    /// <summary>
    /// Handler para eventos de novos mobs detectados no protocolo Photon.
    /// </summary>
    public class NewMobEventHandler
    {
        private readonly IMobsHandler _mobHandler;

        public event Action<NewMobParsedData>? OnMobParsed;

        public NewMobEventHandler(IMobsHandler mobHandler)
        {
            _mobHandler = mobHandler;
        }

        public Task HandleAsync(NewMobEvent value)
        {
            _mobHandler.AddMob(value.Id, value.TypeId, value.Position, value.Health, value.Charge);

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

    public interface IMobsHandler
    {
        void AddMob(string id, int typeId, Vector2 position, int health, int charge);
    }
} 