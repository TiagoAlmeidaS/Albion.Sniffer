using Microsoft.Extensions.Logging;
using AlbionOnlineSniffer.Core.Models.GameObjectsV2;
using AlbionOnlineSniffer.Core.Models.GameObjects;

namespace AlbionOnlineSniffer.Core.Managers
{
    public class MobsManager
    {
        private readonly ILogger<MobsManager> _logger;

        public MobsManager(ILogger<MobsManager> logger)
        {
            _logger = logger;
        }

        public Mob? ProcessNewMob(Dictionary<string, object> parameters)
        {
            _logger.LogDebug("Processando NewMob");
            return new Mob();
        }

        public bool ProcessMobChangeState(Dictionary<string, object> parameters)
        {
            _logger.LogDebug("Processando MobChangeState");
            return true;
        }
    }
} 