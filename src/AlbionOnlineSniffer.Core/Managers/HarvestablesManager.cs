using Microsoft.Extensions.Logging;
using AlbionOnlineSniffer.Core.Models.GameObjectsV2;
using AlbionOnlineSniffer.Core.Models.GameObjects;

namespace AlbionOnlineSniffer.Core.Managers
{
    public class HarvestablesManager
    {
        private readonly ILogger<HarvestablesManager> _logger;

        public HarvestablesManager(ILogger<HarvestablesManager> logger)
        {
            _logger = logger;
        }

        public Harvestable? ProcessNewHarvestable(Dictionary<string, object> parameters)
        {
            _logger.LogDebug("Processando NewHarvestable");
            return new Harvestable();
        }

        public bool ProcessHarvestableChangeState(Dictionary<string, object> parameters)
        {
            _logger.LogDebug("Processando HarvestableChangeState");
            return true;
        }
    }
} 