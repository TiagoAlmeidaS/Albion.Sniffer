using System;
using System.Numerics;
using System.Threading.Tasks;
using AlbionOnlineSniffer.Core.Models.Events;
using AlbionOnlineSniffer.Core.Interfaces;

namespace AlbionOnlineSniffer.Core.Handlers
{
    /// <summary>
    /// Handler para eventos de novos harvestables detectados no protocolo Photon.
    /// </summary>
    public class NewHarvestableEventHandler
    {
        private readonly IHarvestablesManager _harvestableManager;

        public event Action<NewHarvestableParsedData>? OnHarvestableParsed;

        public NewHarvestableEventHandler(IHarvestablesManager harvestableManager)
        {
            _harvestableManager = harvestableManager;
        }

        public Task HandleAsync(NewHarvestableEvent value)
        {
            _harvestableManager.AddHarvestable(
                int.TryParse(value.Id, out var id) ? id : 0,
                value.Type,
                value.Tier,
                value.Position,
                value.Count,
                value.Charge
            );

            OnHarvestableParsed?.Invoke(new NewHarvestableParsedData
            {
                Id = value.Id,
                Type = value.Type,
                Tier = value.Tier,
                Position = value.Position,
                Count = value.Count,
                Charge = value.Charge
            });

            return Task.CompletedTask;
        }
    }

    public class NewHarvestableParsedData
    {
        public string Id { get; set; }
        public int Type { get; set; }
        public int Tier { get; set; }
        public Vector2 Position { get; set; }
        public int Count { get; set; }
        public int Charge { get; set; }
    }
} 