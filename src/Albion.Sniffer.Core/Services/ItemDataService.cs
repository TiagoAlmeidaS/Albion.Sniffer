using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using Microsoft.Extensions.Logging;

namespace Albion.Sniffer.Core.Services
{
    /// <summary>
    /// Serviço para carregar e gerenciar dados de itens
    /// Baseado no albion-radar-deatheye-2pc
    /// </summary>
    public class ItemDataService
    {
        private readonly ILogger<ItemDataService> _logger;
        private readonly Dictionary<int, ItemInfo> _items = new();

        public ItemDataService(ILogger<ItemDataService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Carrega os dados de itens do arquivo XML
        /// </summary>
        /// <param name="filePath">Caminho para o arquivo items.xml</param>
        public void LoadItems(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    _logger.LogWarning("Arquivo de itens não encontrado: {FilePath}", filePath);
                    return;
                }

                var serializer = new XmlSerializer(typeof(ItemsRoot));
                using var reader = new StreamReader(filePath);
                var itemsRoot = (ItemsRoot?)serializer.Deserialize(reader);

                if (itemsRoot?.Items != null)
                {
                    _items.Clear();
                    foreach (var item in itemsRoot.Items)
                    {
                        _items[item.Id] = item;
                    }

                    _logger.LogInformation("Carregados {ItemCount} itens", _items.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar itens: {Message}", ex.Message);
            }
        }

        /// <summary>
        /// Obtém informações de um item pelo ID
        /// </summary>
        /// <param name="itemId">ID do item</param>
        /// <returns>Informações do item ou null se não encontrado</returns>
        public ItemInfo? GetItem(int itemId)
        {
            return _items.TryGetValue(itemId, out var item) ? item : null;
        }

        /// <summary>
        /// Obtém o nome de um item pelo ID
        /// </summary>
        /// <param name="itemId">ID do item</param>
        /// <returns>Nome do item ou string vazia se não encontrado</returns>
        public string GetItemName(int itemId)
        {
            return GetItem(itemId)?.Name ?? string.Empty;
        }

        /// <summary>
        /// Obtém o tier de um item pelo ID
        /// </summary>
        /// <param name="itemId">ID do item</param>
        /// <returns>Tier do item ou 0 se não encontrado</returns>
        public int GetItemTier(int itemId)
        {
            return GetItem(itemId)?.Tier ?? 0;
        }

        /// <summary>
        /// Verifica se um item existe
        /// </summary>
        /// <param name="itemId">ID do item</param>
        /// <returns>True se o item existe</returns>
        public bool HasItem(int itemId)
        {
            return _items.ContainsKey(itemId);
        }
    }

    /// <summary>
    /// Informações de um item
    /// </summary>
    public class ItemInfo
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Tier { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
    }

    /// <summary>
    /// Raiz do XML de itens
    /// </summary>
    [XmlRoot("Items")]
    public class ItemsRoot
    {
        [XmlElement("Item")]
        public List<ItemInfo> Items { get; set; } = new();
    }
} 