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
        private readonly NewWispGateEventHandler _newWispGateHandler;
        private readonly WispGateOpenedEventHandler _wispGateOpenedHandler;

        public ConcurrentDictionary<int, GatedWisp> GatedWispsList { get; } = new();

        public GatedWispsManager(ILogger<GatedWispsManager> logger, EventDispatcher eventDispatcher, 
            NewWispGateEventHandler newWispGateHandler, WispGateOpenedEventHandler wispGateOpenedHandler)
        {
            _logger = logger;
            _eventDispatcher = eventDispatcher;
            _newWispGateHandler = newWispGateHandler;
            _wispGateOpenedHandler = wispGateOpenedHandler;
        }

        public void AddWispInGate(int id, Vector2 position)
        {
            lock (GatedWispsList)
            {
                if (GatedWispsList.ContainsKey(id))
                    GatedWispsList.TryRemove(id, out _);
                
                var wisp = new GatedWisp(id, position);
                GatedWispsList.TryAdd(id, wisp);
                
                _logger.LogInformation("Gated Wisp detectado: ID {Id} em ({X}, {Y})", id, position.X, position.Y);
                
                // Disparar evento de gated wisp detectado
                _ = _eventDispatcher.DispatchEvent(new GatedWispDetectedEvent(id, position));
            }
        }

        /// <summary>
        /// Processa um evento NewWispGate
        /// </summary>
        public async Task<GatedWisp?> ProcessNewGatedWisp(Dictionary<byte, object> parameters)
        {
            try
            {
                var gatedWisp = await _newWispGateHandler.HandleNewWispGate(parameters);
                if (gatedWisp != null)
                {
                    AddWispInGate(gatedWisp.Id, gatedWisp.Position);
                    
                    _logger.LogInformation("Novo gated wisp processado: ID {Id}", gatedWisp.Id);
                    return gatedWisp;
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar NewGatedWisp: {Message}", ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Processa um evento WispGateOpened
        /// </summary>
        public async Task<GatedWisp?> ProcessWispGateOpened(Dictionary<byte, object> parameters)
        {
            try
            {
                var gatedWisp = await _wispGateOpenedHandler.HandleWispGateOpened(parameters);
                if (gatedWisp != null)
                {
                    // Remover o wisp quando o gate é aberto
                    Remove(gatedWisp.Id);
                    
                    _logger.LogInformation("Wisp Gate aberto processado: ID {Id}", gatedWisp.Id);
                    return gatedWisp;
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar WispGateOpened: {Message}", ex.Message);
                return null;
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