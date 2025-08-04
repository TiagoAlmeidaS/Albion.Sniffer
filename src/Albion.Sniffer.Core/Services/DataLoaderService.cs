using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;
using Albion.Sniffer.Core.Models.ResponseObj;
using Albion.Sniffer.Core.Models.GameObjects.Localplayer;
using Albion.Sniffer.Core.Models.Dependencies.Harvestable;
using Albion.Sniffer.Core.Models.Dependencies.Mob;
using Albion.Sniffer.Core.Models.Dependencies.Item;

namespace Albion.Sniffer.Core.Services
{
    /// <summary>
    /// Serviço para carregar todos os dados XML necessários
    /// Baseado na estrutura do albion-radar
    /// </summary>
    public class DataLoaderService
    {
        private readonly ILogger<DataLoaderService> _logger;
        private readonly string _basePath;

        public DataLoaderService(ILogger<DataLoaderService> logger)
        {
            _logger = logger;
            _basePath = Directory.GetCurrentDirectory();
        }

        /// <summary>
        /// Carrega dados de clusters do arquivo JSON
        /// </summary>
        public Dictionary<string, Cluster> LoadClusters()
        {
            try
            {
                var clustersPath = Path.Combine(_basePath, "ao-bin-dumps", "clusters.json");
                if (!File.Exists(clustersPath))
                {
                    _logger.LogWarning("Arquivo de clusters não encontrado: {Path}", clustersPath);
                    return new Dictionary<string, Cluster>();
                }

                var json = File.ReadAllText(clustersPath);
                var clusters = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, Cluster>>(json);
                
                _logger.LogInformation("Carregados {Count} clusters", clusters?.Count ?? 0);
                return clusters ?? new Dictionary<string, Cluster>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar clusters");
                return new Dictionary<string, Cluster>();
            }
        }

        /// <summary>
        /// Carrega dados de itens do arquivo XML
        /// </summary>
        public List<PlayerItems> LoadItems()
        {
            try
            {
                var itemsPath = Path.Combine(_basePath, "ao-bin-dumps", "items.xml");
                if (!File.Exists(itemsPath))
                {
                    _logger.LogWarning("Arquivo de itens não encontrado: {Path}", itemsPath);
                    return new List<PlayerItems>();
                }

                var items = ItemData.Load(itemsPath);
                _logger.LogInformation("Carregados {Count} itens", items?.Count ?? 0);
                return items ?? new List<PlayerItems>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar itens");
                return new List<PlayerItems>();
            }
        }

        /// <summary>
        /// Carrega dados de mobs do arquivo XML
        /// </summary>
        public List<MobInfo> LoadMobs()
        {
            try
            {
                var mobsPath = Path.Combine(_basePath, "ao-bin-dumps", "mobs.xml");
                if (!File.Exists(mobsPath))
                {
                    _logger.LogWarning("Arquivo de mobs não encontrado: {Path}", mobsPath);
                    return new List<MobInfo>();
                }

                var mobs = MobData.Load(mobsPath);
                _logger.LogInformation("Carregados {Count} mobs", mobs?.Count ?? 0);
                return mobs ?? new List<MobInfo>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar mobs");
                return new List<MobInfo>();
            }
        }

        /// <summary>
        /// Carrega dados de harvestables do arquivo XML
        /// </summary>
        public Dictionary<int, string> LoadHarvestables()
        {
            try
            {
                var harvestablesPath = Path.Combine(_basePath, "ao-bin-dumps", "harvestables.xml");
                if (!File.Exists(harvestablesPath))
                {
                    _logger.LogWarning("Arquivo de harvestables não encontrado: {Path}", harvestablesPath);
                    return new Dictionary<int, string>();
                }

                var harvestables = HarvestableData.Load(harvestablesPath);
                _logger.LogInformation("Carregados {Count} harvestables", harvestables?.Count ?? 0);
                return harvestables ?? new Dictionary<int, string>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar harvestables");
                return new Dictionary<int, string>();
            }
        }
    }
} 