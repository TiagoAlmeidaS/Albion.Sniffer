// Classe responsável por gerenciar o estado e a coleção de dungeons do jogo.
// Não processa eventos diretamente, apenas mantém e manipula o estado das dungeons.
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using AlbionOnlineSniffer.Core.Models.Events;
using AlbionOnlineSniffer.Core.Models.GameObjects;
using AlbionOnlineSniffer.Core.Interfaces;
using AlbionOnlineSniffer.Core.Services;
using Microsoft.Extensions.Logging;

namespace AlbionOnlineSniffer.Core.Handlers
{
    public class DungeonsManager : IDungeonsManager
    {
        private readonly ILogger<DungeonsManager> _logger;
        private readonly EventDispatcher _eventDispatcher;
        // Handler agora gerenciado pelo AlbionNetworkHandlerManager
        public ConcurrentDictionary<int, Dungeon> DungeonsList { get; } = new();

        public DungeonsManager(ILogger<DungeonsManager> logger, EventDispatcher eventDispatcher)
        {
            _logger = logger;
            _eventDispatcher = eventDispatcher;
            // Handler agora gerenciado pelo AlbionNetworkHandlerManager
        }

        public void AddDungeon(int id, string type, Vector2 position, int charges)
        {
            lock (DungeonsList)
            {
                if (DungeonsList.ContainsKey(id))
                    DungeonsList.TryRemove(id, out _);
                
                var dungeon = new Dungeon(id, type, position, charges);
                DungeonsList.TryAdd(id, dungeon);
                
                _logger.LogInformation("Dungeon adicionado: {Type} (ID: {Id})", type, id);
                
                // Disparar evento de dungeon detectado
                _ = _eventDispatcher.DispatchEvent(new DungeonDetectedEvent(dungeon));
            }
        }

        // Processamento de eventos agora gerenciado pelo AlbionNetworkHandlerManager
        public void Remove(int id)
        {
            lock (DungeonsList)
                DungeonsList.TryRemove(id, out _);
        }
        public void Clear()
        {
            lock (DungeonsList)
                DungeonsList.Clear();
        }
    }
} 