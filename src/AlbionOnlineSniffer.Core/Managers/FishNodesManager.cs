using Microsoft.Extensions.Logging;
using AlbionOnlineSniffer.Core.Models.GameObjectsV2;
using AlbionOnlineSniffer.Core.Models.GameObjects;

namespace AlbionOnlineSniffer.Core.Managers
{
    public class FishNodesManager
    {
        private readonly ILogger<FishNodesManager> _logger;

        public FishNodesManager(ILogger<FishNodesManager> logger)
        {
            _logger = logger;
        }

        public FishNode? ProcessNewFishingZone(Dictionary<string, object> parameters)
        {
            _logger.LogDebug("Processando NewFishingZone");
            return new FishNode();
        }
    }
} 