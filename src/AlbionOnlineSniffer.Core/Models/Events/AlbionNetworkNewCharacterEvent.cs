using System;
using System.Collections.Generic;
using System.Numerics;
using Albion.Network;
using AlbionOnlineSniffer.Core.Models.GameObjects;
using AlbionOnlineSniffer.Core.Services;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    /// <summary>
    /// Evento NewCharacter compatível com Albion.Network.BaseEvent
    /// Baseado no albion-radar-deatheye-2pc
    /// </summary>
    public class AlbionNetworkNewCharacterEvent : BaseEvent
    {
        private readonly byte[] _offsets;

        public AlbionNetworkNewCharacterEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            _offsets = PacketOffsetsLoader.GlobalPacketOffsets?.NewCharacter ?? new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            
            Id = Convert.ToInt32(parameters[_offsets[0]]);
            Name = parameters[_offsets[1]] as string ?? string.Empty;
            Guild = parameters.ContainsKey(_offsets[2]) ? parameters[_offsets[2]] as string ?? string.Empty : string.Empty;
            Alliance = parameters.ContainsKey(_offsets[3]) ? parameters[_offsets[3]] as string ?? string.Empty : string.Empty;
            Faction = (Faction)parameters[_offsets[4]];
            
            EncryptedPosition = parameters[_offsets[5]] as byte[];
            Speed = parameters.ContainsKey(_offsets[6]) ? (float)parameters[_offsets[6]] : 5.5f;

            Health = parameters.ContainsKey(_offsets[7]) ?
                new Health(Convert.ToInt32(parameters[_offsets[7]]), Convert.ToInt32(parameters[_offsets[8]]))
                : new Health(Convert.ToInt32(parameters[_offsets[8]]));

            Equipments = ConvertArray(parameters[_offsets[9]]);
            Spells = ConvertArray(parameters[_offsets[10]]);
        }

        public int Id { get; }
        public string Name { get; }
        public string Guild { get; }
        public string Alliance { get; }
        public Faction Faction { get; }
        public Vector2 Position { get; }
        public byte[]? EncryptedPosition { get; }
        public float Speed { get; }
        public Health Health { get; }
        public int[]? Equipments { get; }
        public int[]? Spells { get; }

        /// <summary>
        /// Converte array de diferentes tipos para int[]
        /// Baseado no método ConvertArray do albion-radar-deatheye-2pc
        /// </summary>
        private int[]? ConvertArray(object value)
        {
            if (value == null) return null;

            return value switch
            {
                byte[] numArray2 => Array.ConvertAll(numArray2, b => (int)b),
                short[] numArray3 => Array.ConvertAll(numArray3, s => (int)s),
                int[] numArray1 => numArray1,
                _ => null
            };
        }
    }
} 