using System;
using System.Numerics;
using System.Threading.Tasks;
using AlbionOnlineSniffer.Core.Models.Events;
using AlbionOnlineSniffer.Core.Models;
using AlbionOnlineSniffer.Core.Interfaces;

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
            _mobManager.AddMob(
                int.TryParse(value.Id, out var id) ? id : 0,
                value.TypeId,
                value.Position,
                new Health { Value = value.Health },
                (byte)value.Charge
            );

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
} 