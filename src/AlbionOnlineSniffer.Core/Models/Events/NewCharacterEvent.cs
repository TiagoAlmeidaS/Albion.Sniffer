using System.Numerics;
using System.Reflection;
using Albion.Network;
using AlbionOnlineSniffer.Core.Models.GameObjects.Players;
using AlbionOnlineSniffer.Core.Services;
using System.Collections.Generic;
using AlbionOnlineSniffer.Core.Models.ResponseObj;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    [Obfuscation(Feature = "mutation", Exclude = false)]
    public class NewCharacterEvent : BaseEvent
    {
        private readonly byte[] offsets;

        // Construtor para compatibilidade com framework Albion.Network
        public NewCharacterEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            var packetOffsets = PacketOffsetsProvider.GetOffsets();
            offsets = packetOffsets?.NewCharacter;
            
            InitializeProperties(parameters);
        }

        // Construtor para injeção de dependência direta (se necessário no futuro)
        public NewCharacterEvent(Dictionary<byte, object> parameters, PacketOffsets packetOffsets) : base(parameters)
        {
            offsets = packetOffsets?.NewCharacter;
            
            InitializeProperties(parameters);
        }

        private void InitializeProperties(Dictionary<byte, object> parameters)
        {
            Id = Convert.ToInt32(parameters[offsets[0]]);
            Name = (string)parameters[offsets[1]];
            GuildName = (string)parameters[offsets[2]];
            AllianceName = (string)parameters[offsets[3]];

            if (parameters.ContainsKey(offsets[4]) && parameters[offsets[4]] is byte[] positionBytes)
            {
                PositionBytes = positionBytes;
            }

            Items = parameters[offsets[5]] as float[];
        }

        public int Id { get; private set; }
        public string Name { get; private set; }
        public string GuildName { get; private set; }
        public string AllianceName { get; private set; }
        public byte[] PositionBytes { get; private set; }
        public float[] Items { get; private set; }

        public Vector2 Position { get; set; }

        private int[] ConvertArray(object value)
        {
            int[] numArray1;

            switch (value)
            {
                case byte[] numArray2:
                    numArray1 = new int[numArray2.Length];
                    for (int index = 0; index < numArray2.Length; ++index)
                        numArray1[index] = numArray2[index];
                    break;

                case short[] numArray3:
                    numArray1 = new int[numArray3.Length];
                    for (int index = 0; index < numArray3.Length; ++index)
                        numArray1[index] = numArray3[index];
                    break;

                default:
                    numArray1 = (int[])value;
                    break;
            }

            return numArray1;
        }
    }
}
