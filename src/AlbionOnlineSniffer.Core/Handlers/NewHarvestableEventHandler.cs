using System;
using System.Numerics;
using System.Threading.Tasks;

namespace AlbionOnlineSniffer.Core.Handlers
{
    /// <summary>
    /// Handler para eventos de novos harvestables detectados no protocolo Photon.
    /// </summary>
    public class NewHarvestableEventHandler
    {
        private readonly IHarvestablesHandler _harvestableHandler;

        public event Action<NewHarvestableParsedData>? OnHarvestableParsed;

        public NewHarvestableEventHandler(IHarvestablesHandler harvestableHandler)
        {
            _harvestableHandler = harvestableHandler;
        }

        public Task HandleAsync(NewHarvestableEvent value)
        {
            _harvestableHandler.AddHarvestable(value.Id, value.Type, value.Tier, value.Position, value.Count, value.Charge);

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

    public interface IHarvestablesHandler
    {
        void AddHarvestable(string id, int type, int tier, Vector2 position, int count, int charge);
    }
} 