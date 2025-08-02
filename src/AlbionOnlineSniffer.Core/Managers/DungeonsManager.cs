using Microsoft.Extensions.Logging;
using AlbionOnlineSniffer.Core.Models.GameObjectsV2;
using AlbionOnlineSniffer.Core.Models.GameObjects;

namespace AlbionOnlineSniffer.Core.Managers
{
    public class DungeonsManager
    {
        private readonly ILogger<DungeonsManager> _logger;

        public DungeonsManager(ILogger<DungeonsManager> logger)
        {
            _logger = logger;
        }

        public Dungeon? ProcessNewDungeon(Dictionary<string, object> parameters)
        {
            _logger.LogDebug("Processando NewDungeon");
            return new Dungeon();
        }
    }
} 