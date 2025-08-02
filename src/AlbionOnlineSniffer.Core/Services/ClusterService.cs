using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using AlbionOnlineSniffer.Core.Models;
using AlbionOnlineSniffer.Core.Models.GameObjects.Localplayer;

namespace AlbionOnlineSniffer.Core.Services
{
    /// <summary>
    /// Serviço para gerenciar informações de clusters/mapas
    /// Baseado no albion-radar-deatheye-2pc
    /// </summary>
    public class ClusterService
    {
        private readonly ILogger<ClusterService> _logger;
        private readonly Dictionary<string, Cluster> _clusters = new();

        public ClusterService(ILogger<ClusterService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Carrega os dados de clusters do arquivo JSON
        /// </summary>
        /// <param name="filePath">Caminho para o arquivo clusters.json</param>
        public void LoadClusters(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    _logger.LogWarning("Arquivo de clusters não encontrado: {FilePath}", filePath);
                    return;
                }

                var jsonContent = File.ReadAllText(filePath);
                var clustersData = JsonSerializer.Deserialize<Dictionary<string, ClusterData>>(jsonContent);

                if (clustersData != null)
                {
                    _clusters.Clear();
                    foreach (var kvp in clustersData)
                    {
                        var cluster = new Cluster
                        {
                            Id = kvp.Value.Id,
                            ClusterColor = (ClusterColor)kvp.Value.ClusterColor,
                            DisplayName = kvp.Value.DisplayName
                        };
                        _clusters[kvp.Key] = cluster;
                    }

                    _logger.LogInformation("Carregados {ClusterCount} clusters", _clusters.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar clusters: {Message}", ex.Message);
            }
        }

        /// <summary>
        /// Obtém um cluster pelo ID
        /// </summary>
        /// <param name="clusterId">ID do cluster</param>
        /// <returns>Cluster ou null se não encontrado</returns>
        public Cluster? GetCluster(string clusterId)
        {
            return _clusters.TryGetValue(clusterId, out var cluster) ? cluster : null;
        }

        /// <summary>
        /// Obtém todos os clusters
        /// </summary>
        /// <returns>Dicionário com todos os clusters</returns>
        public Dictionary<string, Cluster> GetAllClusters()
        {
            return new Dictionary<string, Cluster>(_clusters);
        }

        /// <summary>
        /// Verifica se um cluster existe
        /// </summary>
        /// <param name="clusterId">ID do cluster</param>
        /// <returns>True se o cluster existe</returns>
        public bool HasCluster(string clusterId)
        {
            return _clusters.ContainsKey(clusterId);
        }

        /// <summary>
        /// Obtém a cor de um cluster
        /// </summary>
        /// <param name="clusterId">ID do cluster</param>
        /// <returns>Cor do cluster ou Unknown se não encontrado</returns>
        public ClusterColor GetClusterColor(string clusterId)
        {
            return GetCluster(clusterId)?.ClusterColor ?? ClusterColor.Unknown;
        }

        /// <summary>
        /// Obtém o nome de exibição de um cluster
        /// </summary>
        /// <param name="clusterId">ID do cluster</param>
        /// <returns>Nome de exibição ou string vazia se não encontrado</returns>
        public string GetClusterDisplayName(string clusterId)
        {
            return GetCluster(clusterId)?.DisplayName ?? string.Empty;
        }

        /// <summary>
        /// Verifica se um cluster é uma cidade
        /// </summary>
        /// <param name="clusterId">ID do cluster</param>
        /// <returns>True se é uma cidade</returns>
        public bool IsCity(string clusterId)
        {
            return GetClusterColor(clusterId) == ClusterColor.City;
        }

        /// <summary>
        /// Verifica se um cluster é uma zona PvP
        /// </summary>
        /// <param name="clusterId">ID do cluster</param>
        /// <returns>True se é uma zona PvP</returns>
        public bool IsPvpZone(string clusterId)
        {
            var color = GetClusterColor(clusterId);
            return color == ClusterColor.Black;
        }
    }

    /// <summary>
    /// Dados de cluster para deserialização JSON
    /// </summary>
    public class ClusterData
    {
        public string Id { get; set; } = string.Empty;
        public int ClusterColor { get; set; }
        public string DisplayName { get; set; } = string.Empty;
    }
} 