using Albion.Network;
using Albion.Events.V1;
using AlbionOnlineSniffer.Core.Models.Events;
using AlbionOnlineSniffer.Core.Models.GameObjects.Harvestables;
using AlbionOnlineSniffer.Core.Services;
using System.Numerics;

namespace AlbionOnlineSniffer.Core.Handlers
{
    public class NewHarvestablesListEventHandler : EventPacketHandler<NewHarvestablesListEvent>
    {
        private readonly HarvestablesHandler harvestableHandler;
        private readonly EventDispatcher eventDispatcher;
        public NewHarvestablesListEventHandler(HarvestablesHandler harvestableHandler, EventDispatcher eventDispatcher) : base(PacketIndexesLoader.GlobalPacketIndexes?.NewHarvestableList ?? 0)
        {
            this.harvestableHandler = harvestableHandler;
            this.eventDispatcher = eventDispatcher;
        }

        protected override async Task OnActionAsync(NewHarvestablesListEvent value)
        {
            var harvestablePositions = new List<(int Id, int TypeId, int Tier, float X, float Y, int Charges)>();
            
            foreach (var harvestableObject in value.HarvestableObjects)
            {
                Vector2 position = Vector2.Zero;
                if (harvestableObject.PositionBytes != null && harvestableObject.PositionBytes.Length >= 8)
                {
                    position = new Vector2(BitConverter.ToSingle(harvestableObject.PositionBytes, 4), BitConverter.ToSingle(harvestableObject.PositionBytes, 0));
                }

                harvestableHandler.AddHarvestable(harvestableObject.Id, harvestableObject.TypeId, harvestableObject.Tier, position, count: 0, charge: harvestableObject.Charges);
                
                harvestablePositions.Add((harvestableObject.Id, harvestableObject.TypeId, harvestableObject.Tier, position.X, position.Y, harvestableObject.Charges));
            }
            
                            // 🚀 CRIAR E DESPACHAR EVENTO V1
                var harvestablesListFoundV1 = new HarvestablesListFoundV1
                {
                    EventId = Guid.NewGuid().ToString("n"),
                    ObservedAt = DateTimeOffset.UtcNow,
                    Harvestables = harvestablePositions.Select(pos => new HarvestableFoundV1
                    {
                        EventId = Guid.NewGuid().ToString("n"),
                        ObservedAt = DateTimeOffset.UtcNow,
                        Id = pos.Id,
                        TypeId = pos.TypeId,
                        Tier = pos.Tier,
                        X = pos.X,
                        Y = pos.Y,
                        Charges = pos.Charges
                    }).ToArray()
                };

            // Emitir evento Core para handlers legados - DISABLED
            // await eventDispatcher.DispatchEvent(value);
            
            // Emitir evento V1 para contratos
            await eventDispatcher.DispatchEvent(harvestablesListFoundV1);
        }
    }
}
