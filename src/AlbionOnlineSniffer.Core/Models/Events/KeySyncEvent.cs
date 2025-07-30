using System;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    /// <summary>
    /// Evento de sincronização de chave XOR
    /// Baseado no albion-radar-deatheye-2pc
    /// </summary>
    public class KeySyncEvent : GameEvent
    {
        public KeySyncEvent(byte[] code)
        {
            EventType = "KeySync";
            Code = code;
        }

        public byte[] Code { get; set; }
    }
} 