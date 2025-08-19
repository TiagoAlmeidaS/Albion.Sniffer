using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;
using AlbionOnlineSniffer.Core.Models.ResponseObj;
using AlbionOnlineSniffer.Core.Models.GameObjects.Localplayer;
using AlbionOnlineSniffer.Core.Models.Dependencies.Harvestable;
using AlbionOnlineSniffer.Core.Models.Dependencies.Mob;
using AlbionOnlineSniffer.Core.Models.Dependencies.Item;
using System.Linq;

namespace AlbionOnlineSniffer.Core.Services
{
    /// <summary>
    /// Serviço para carregar todos os dados JSON necessários
    /// Baseado na estrutura do albion-radar, mas usando arquivos do Core
    /// </summary>
    public class DataLoaderService
    {
        private readonly ILogger<DataLoaderService> _logger;

        public DataLoaderService(ILogger<DataLoaderService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Resolve o caminho para os arquivos JSON do Core
        /// </summary>
        private string? ResolveCoreJsonPath(string fileName)
        {
            var probePaths = new List<string>
            {
                Path.Combine(AppContext.BaseDirectory, "src/AlbionOnlineSniffer.Core/Data/jsons", fileName),
                Path.Combine(Directory.GetCurrentDirectory(), "src/AlbionOnlineSniffer.Core/Data/jsons", fileName),
                Path.Combine(AppContext.BaseDirectory, fileName),
                Path.Combine(Directory.GetCurrentDirectory(), fileName)
            };

            // Tenta resolver via ContentRoot (Web/App) para o caminho do projeto Core
            var currentDir = Directory.GetCurrentDirectory();
            var baseDir = AppContext.BaseDirectory;
            
            // Caminho relativo de sibling project (../AlbionOnlineSniffer.Core/...)
            var siblingCorePath = Path.GetFullPath(Path.Combine(currentDir, "../AlbionOnlineSniffer.Core/Data/jsons", fileName));
            probePaths.Add(siblingCorePath);

            // Caminho relativo via src (caso ContentRoot seja a raiz do repositório)
            var srcCorePath = Path.GetFullPath(Path.Combine(currentDir, "src/AlbionOnlineSniffer.Core/Data/jsons", fileName));
            probePaths.Add(srcCorePath);

            return probePaths.FirstOrDefault(File.Exists);
        }

        /// <summary>
        /// Carrega dados de clusters do arquivo JSON do Core
        /// </summary>
        public Dictionary<string, Cluster> LoadClusters()
        {
            try
            {
                var clustersPath = ResolveCoreJsonPath("clusters.json");
                if (string.IsNullOrEmpty(clustersPath))
                {
                    _logger.LogWarning("Arquivo de clusters não encontrado. Verificando caminhos: {CurrentDir}, {BaseDir}", 
                        Directory.GetCurrentDirectory(), AppContext.BaseDirectory);
                    return new Dictionary<string, Cluster>();
                }

                var json = File.ReadAllText(clustersPath);
                var clusters = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, Cluster>>(json);
                
                _logger.LogInformation("Carregados {Count} clusters de: {Path}", clusters?.Count ?? 0, clustersPath);
                return clusters ?? new Dictionary<string, Cluster>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar clusters");
                return new Dictionary<string, Cluster>();
            }
        }

        /// <summary>
        /// Carrega dados de itens do arquivo XML (mantido para compatibilidade)
        /// </summary>
        public List<PlayerItems> LoadItems()
        {
            try
            {
                var itemsPath = Path.Combine(Directory.GetCurrentDirectory(), "ao-bin-dumps", "items.xml");
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
        /// Carrega dados de mobs do arquivo XML (mantido para compatibilidade)
        /// </summary>
        public List<MobInfo> LoadMobs()
        {
            try
            {
                var mobsPath = Path.Combine(Directory.GetCurrentDirectory(), "ao-bin-dumps", "mobs.xml");
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
        /// Carrega dados de harvestables do arquivo XML (mantido para compatibilidade)
        /// </summary>
        public Dictionary<int, string> LoadHarvestables()
        {
            try
            {
                var harvestablesPath = Path.Combine(Directory.GetCurrentDirectory(), "ao-bin-dumps", "harvestables.xml");
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