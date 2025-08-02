using Microsoft.Extensions.Logging;
using AlbionOnlineSniffer.Core.Models.GameObjectsV2;
using AlbionOnlineSniffer.Core.Models.GameObjects;

namespace AlbionOnlineSniffer.Core.Managers
{
    public class GatedWispsManager
    {
        private readonly ILogger<GatedWispsManager> _logger;

        public GatedWispsManager(ILogger<GatedWispsManager> logger)
        {
            _logger = logger;
        }

        public GatedWisp? ProcessNewGatedWisp(Dictionary<string, object> parameters)
        {
            _logger.LogDebug("Processando NewGatedWisp");
            return new GatedWisp();
        }
    }
} 