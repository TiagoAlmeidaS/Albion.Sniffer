using System;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    /// <summary>
    /// Evento de informação de jogador entrando nos mists
    /// Baseado no albion-radar-deatheye-2pc
    /// </summary>
    public class MistsPlayerJoinedInfoEvent : GameEvent
    {
        public MistsPlayerJoinedInfoEvent(DateTime timeCycle)
        {
            EventType = "MistsPlayerJoinedInfo";
            TimeCycle = timeCycle;
        }

        public DateTime TimeCycle { get; set; }
    }
} 