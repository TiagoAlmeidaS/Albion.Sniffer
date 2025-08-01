using System;
using System.Collections.Generic;
using System.Numerics;
using Albion.Network;
using AlbionOnlineSniffer.Core.Models.GameObjects;
using AlbionOnlineSniffer.Core.Models;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    /// <summary>
    /// Evento NewCharacter compatível com Albion.Network.BaseEvent
    /// Baseado no albion-radar-deatheye-2pc
    /// </summary>
    public class AlbionNetworkNewCharacterEvent : BaseAlbionNetworkEvent
    {
        public AlbionNetworkNewCharacterEvent(Dictionary<byte, object> parameters, PacketOffsets packetOffsets) : base(parameters, packetOffsets)
        {
            var offsets = GetOffsets("NewCharacter");
            
            if (offsets.Length >= 11)
            {
                Id = Convert.ToInt32(parameters[offsets[0]]);
                Name = parameters[offsets[1]] as string ?? string.Empty;
                Guild = parameters.ContainsKey(offsets[2]) ? parameters[offsets[2]] as string ?? string.Empty : string.Empty;
                Alliance = parameters.ContainsKey(offsets[3]) ? parameters[offsets[3]] as string ?? string.Empty : string.Empty;
                Faction = (Faction)parameters[offsets[4]];
                
                EncryptedPosition = parameters[offsets[5]] as byte[];
                Speed = parameters.ContainsKey(offsets[6]) ? (float)parameters[offsets[6]] : 5.5f;

                Health = parameters.ContainsKey(offsets[7]) ?
                    new Health(Convert.ToInt32(parameters[offsets[7]]), Convert.ToInt32(parameters[offsets[8]]))
                    : new Health(Convert.ToInt32(parameters[offsets[8]]));

                Equipments = ConvertArray(parameters[offsets[9]]);
                Spells = ConvertArray(parameters[offsets[10]]);
            }
            else
            {
                Id = 0;
                Name = string.Empty;
                Guild = string.Empty;
                Alliance = string.Empty;
                Faction = Faction.NoPVP;
                EncryptedPosition = null;
                Speed = 5.5f;
                Health = new Health(100);
                Equipments = null;
                Spells = null;
            }
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