using System;
using System.Numerics;
using System.Threading.Tasks;
// using Albion.Network; // Supondo que NewCharacterEvent está disponível
// using AlbionOnlineSniffer.Core.Models; // Se necessário, para tipos de dados

namespace AlbionOnlineSniffer.Core.Handlers
{
    /// <summary>
    /// Handler para eventos de novos personagens detectados no protocolo Photon.
    /// </summary>
    public class NewCharacterEventHandler
    {
        // Dependências injetáveis (pode ser adaptado para interfaces)
        private readonly IPlayersHandler _playerHandler;
        private readonly ILocalPlayerHandler _localPlayerHandler;
        private readonly IConfigHandler _configHandler;

        /// <summary>
        /// Evento disparado quando um novo personagem é processado.
        /// </summary>
        public event Action<NewCharacterParsedData>? OnCharacterParsed;

        public NewCharacterEventHandler(IPlayersHandler playerHandler, ILocalPlayerHandler localPlayerHandler, IConfigHandler configHandler)
        {
            _playerHandler = playerHandler;
            _localPlayerHandler = localPlayerHandler;
            _configHandler = configHandler;
        }

        /// <summary>
        /// Processa o evento de novo personagem e dispara o evento OnCharacterParsed.
        /// </summary>
        public Task HandleAsync(NewCharacterEvent value)
        {
            Vector2 pos = Vector2.Zero;
            if (_playerHandler.XorCode != null && value.EncryptedPosition != null)
            {
                var coords = _playerHandler.Decrypt(value.EncryptedPosition);
                pos = new Vector2(coords[1], coords[0]);
            }
            else if (value.Position != Vector2.Zero)
            {
                pos = value.Position;
            }

            _playerHandler.AddPlayer(value.Id, value.Name, value.Guild, value.Alliance, pos, value.Health, value.Faction, value.Equipments, value.Spells);

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
    public interface IPlayersHandler
    {
        object XorCode { get; }
        int[] Decrypt(object encryptedPosition);
        void AddPlayer(string id, string name, string guild, string alliance, Vector2 pos, int health, int faction, object equipments, object spells);
    }

    public interface ILocalPlayerHandler
    {
        // Definir métodos/propriedades relevantes
    }

    public interface IConfigHandler
    {
        // Definir métodos/propriedades relevantes
    }
} 