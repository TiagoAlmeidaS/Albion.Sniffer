using System;
using System.Linq;
using Albion.Events.V1;
using AlbionOnlineSniffer.Core.Contracts;
using AlbionOnlineSniffer.Core.Models.Events;
using System.Numerics;

namespace AlbionOnlineSniffer.Core.Contracts.Transformers
{
    public class LoadClusterObjectsToClusterObjectsLoadedV1 : IEventContractTransformer
    {
        private readonly PositionDecryptionService _positionService;

        public LoadClusterObjectsToClusterObjectsLoadedV1(PositionDecryptionService positionService)
        {
            _positionService = positionService;
        }

        public bool CanTransform(object gameEvent) => gameEvent is LoadClusterObjectsEvent;
        
        public (bool Success, string Topic, object Contract) TryTransform(object gameEvent)
        {
            if (gameEvent is not LoadClusterObjectsEvent e) return (false, string.Empty, null!);
            
            var objectives = e.ClusterObjectives?.Values.Select(o => new ClusterObjectiveV1
            {
                Id = o.Id,
                Charge = o.Charge,
                X = o.Position.X,
                Y = o.Position.Y,
                Type = o.Type,
                Timer = o.Timer
            }).ToArray() ?? Array.Empty<ClusterObjectiveV1>();
            
            var contract = new ClusterObjectsLoadedV1
            {
                EventId = Guid.NewGuid().ToString("n"),
                ObservedAt = DateTimeOffset.UtcNow,
                Objectives = objectives
            };
            
            return (true, "albion.event.cluster.objects.loaded.v1", contract);
        }
    }
}
