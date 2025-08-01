using System;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    /// <summary>
    /// Evento de resposta de saída
    /// Baseado no albion-radar-deatheye-2pc
    /// </summary>
    public class LeaveResponseEvent : GameEvent
    {
        public LeaveResponseEvent(bool success, string message)
        {
            EventType = "LeaveResponse";
            Success = success;
            Message = message;
        }

        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }
} 