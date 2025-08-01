using System;
using System.Collections.Generic;
using Albion.Network;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    /// <summary>
    /// Evento ChangeFlaggingFinished compatível com Albion.Network.BaseEvent
    /// Baseado no albion-radar-deatheye-2pc
    /// </summary>
    public class AlbionNetworkChangeFlaggingFinishedEvent : BaseEvent
    {
        private readonly byte[] _offsets;
 
        public AlbionNetworkChangeFlaggingFinishedEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            var offsets = GetOffsets("ChangeFlaggingFinished");
            
            if (offsets.Length >= 2)
            {
                PlayerId = Convert.ToInt32(parameters[offsets[0]]);
                IsFlagged = Convert.ToBoolean(parameters[offsets[1]]);
            }
            else
            {
                PlayerId = 0;
                IsFlagged = false;
            }
        }

        public int PlayerId { get; }
        public bool IsFlagged { get; }
    }
} 