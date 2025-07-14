using System;
using System.Numerics;
using System.Threading.Tasks;
using AlbionOnlineSniffer.Core.Models.Events;
using AlbionOnlineSniffer.Core.Models;
using AlbionOnlineSniffer.Core.Interfaces;

namespace AlbionOnlineSniffer.Core.Handlers
{
    /// <summary>
    /// Handler para eventos de novos personagens detectados no protocolo Photon.
    /// </summary>
    public class NewCharacterEventHandler
    {
        // Dependências injetáveis (pode ser adaptado para interfaces)
        private readonly IPlayersManager _playerManager;
        private readonly ILocalPlayerHandler _localPlayerHandler;
        private readonly IConfigHandler _configHandler;

        /// <summary>
        /// Evento disparado quando um novo personagem é processado.
        /// </summary>
        public event Action<NewCharacterParsedData>? OnCharacterParsed;

        public NewCharacterEventHandler(IPlayersManager playerManager, ILocalPlayerHandler localPlayerHandler, IConfigHandler configHandler)
        {
            _playerManager = playerManager;
            _localPlayerHandler = localPlayerHandler;
            _configHandler = configHandler;
        }

        /// <summary>
        /// Processa o evento de novo personagem e dispara o evento OnCharacterParsed.
        /// </summary>
        public Task HandleAsync(NewCharacterEvent value)
        {
            Vector2 pos = Vector2.Zero;
            if (_playerManager.XorCode != null && value.EncryptedPosition != null)
            {
                var coords = _playerManager.Decrypt(value.EncryptedPosition as byte[] ?? Array.Empty<byte>());
                pos = new Vector2(coords[1], coords[0]);
            }
            else if (value.Position != Vector2.Zero)
            {
                pos = value.Position;
            }

            _playerManager.AddPlayer(
                int.TryParse(value.Id, out var id) ? id : 0,
                value.Name,
                value.Guild,
                value.Alliance,
                pos,
                new Health { Value = value.Health },
                (Faction)value.Faction,
                value.Equipments as int[] ?? Array.Empty<int>(),
                value.Spells as int[] ?? Array.Empty<int>()
            );

            // Lógica de listas customizadas, facções, etc. pode ser mantida, mas sem UI/som
            // Em vez disso, pode-se disparar eventos ou logs
            // Exemplo: se for um inimigo, disparar evento específico

            // Exemplo de uso do evento para publicação
            OnCharacterParsed?.Invoke(new NewCharacterParsedData
            {
                Id = value.Id,
                Name = value.Name,
                Guild = value.Guild,
                Alliance = value.Alliance,
                Position = pos,
                Health = value.Health,
                Faction = value.Faction,
                Equipments = value.Equipments,
                Spells = value.Spells
            });

            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Dados relevantes extraídos do evento de novo personagem, prontos para publicação.
    /// </summary>
    public class NewCharacterParsedData
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Guild { get; set; }
        public string Alliance { get; set; }
        public Vector2 Position { get; set; }
        public int Health { get; set; }
        public int Faction { get; set; }
        public object Equipments { get; set; } // Tipar conforme necessário
        public object Spells { get; set; } // Tipar conforme necessário
    }

    // Interfaces para handlers (pode ser implementado conforme necessidade)
    // Removidas as interfaces ILocalPlayerHandler e IConfigHandler duplicadas
} 