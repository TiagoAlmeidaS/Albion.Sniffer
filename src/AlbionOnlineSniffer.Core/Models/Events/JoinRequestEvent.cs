using System;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    /// <summary>
    /// Evento de requisição de entrada
    /// Baseado no albion-radar-deatheye-2pc
    /// </summary>
    public class JoinRequestEvent : GameEvent
    {
        public JoinRequestEvent(int playerId, int guildId)
        {
            EventType = "JoinRequest";
            PlayerId = playerId;
            GuildId = guildId;
        }

        public int PlayerId { get; set; }
        public int GuildId { get; set; }
    }
} 