using System;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    /// <summary>
    /// Evento de resposta de join
    /// Baseado no albion-radar-deatheye-2pc
    /// </summary>
    public class JoinResponseEvent : GameEvent
    {
        public JoinResponseEvent(bool success, string message)
        {
            EventType = "JoinResponse";
            Success = success;
            Message = message;
        }

        public bool Success { get; set; }
        public string Message { get; set; }
    }
} 