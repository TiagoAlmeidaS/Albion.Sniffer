// Classe responsável por gerenciar o estado e a coleção de gated wisps do jogo.
// Não processa eventos diretamente, apenas mantém e manipula o estado dos gated wisps.
using System;
using System.Collections.Concurrent;
using System.Numerics;
using System.Threading.Tasks;
using AlbionOnlineSniffer.Core.Models.Events;
using AlbionOnlineSniffer.Core.Models.GameObjects;
using AlbionOnlineSniffer.Core.Interfaces;
using AlbionOnlineSniffer.Core.Services;
using Microsoft.Extensions.Logging;

namespace AlbionOnlineSniffer.Core.Handlers
{
    public class GatedWispsManager : IGatedWispsManager
    {
        private readonly ILogger<GatedWispsManager> _logger;
        private readonly EventDispatcher _eventDispatcher;

        public ConcurrentDictionary<int, GatedWisp> GatedWispsList { get; } = new();

        public GatedWispsManager(ILogger<GatedWispsManager> logger, EventDispatcher eventDispatcher)
        {
            _logger = logger;
            _eventDispatcher = eventDispatcher;
        }

        public void AddWispInGate(int id, Vector2 position)
        {
            lock (GatedWispsList)
            {
                if (GatedWispsList.ContainsKey(id))
                    GatedWispsList.TryRemove(id, out _);
                
                GatedWispsList.TryAdd(id, new GatedWisp(id, position));
                
                _logger.LogInformation("Gated Wisp detectado: ID {Id} em ({X}, {Y})", id, position.X, position.Y);
                
                // Disparar evento de gated wisp detectado
                _ = _eventDispatcher.DispatchEvent(new GatedWispDetectedEvent(id, position));
            }
        }

        public void Remove(int id)
        {
            lock (GatedWispsList)
            {
                if (GatedWispsList.TryRemove(id, out GatedWisp? wisp))
                {
                    _logger.LogInformation("Gated Wisp removido: ID {Id}", id);
                    
                    // Disparar evento de gated wisp coletado
                    _ = _eventDispatcher.DispatchEvent(new GatedWispCollectedEvent(id, wisp.Position));
                }
            }
        }

        public void Clear()
        {
            lock (GatedWispsList)
            {
                GatedWispsList.Clear();
                _logger.LogInformation("Todos os Gated Wisps foram removidos");
            }
        }
    }
} 