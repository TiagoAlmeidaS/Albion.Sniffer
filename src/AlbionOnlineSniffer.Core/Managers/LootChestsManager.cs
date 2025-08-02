using Microsoft.Extensions.Logging;
using AlbionOnlineSniffer.Core.Models.GameObjectsV2;
using AlbionOnlineSniffer.Core.Models.GameObjects;

namespace AlbionOnlineSniffer.Core.Managers
{
    public class LootChestsManager
    {
        private readonly ILogger<LootChestsManager> _logger;

        public LootChestsManager(ILogger<LootChestsManager> logger)
        {
            _logger = logger;
        }

        public LootChest? ProcessNewLootChest(Dictionary<string, object> parameters)
        {
            _logger.LogDebug("Processando NewLootChest");
            return new LootChest();
        }
    }
} 