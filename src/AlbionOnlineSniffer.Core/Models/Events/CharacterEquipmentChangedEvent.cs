using Albion.Network;
using AlbionOnlineSniffer.Core.Services;
using AlbionOnlineSniffer.Core.Models.ResponseObj;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    public class CharacterEquipmentChangedEvent : BaseEvent
    {
        private readonly byte[] offsets;

        // Compat constructor for tests using provider-based offsets
        public CharacterEquipmentChangedEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            var packetOffsets = PacketOffsetsLoader.GlobalPacketOffsets ?? PacketOffsetsProvider.GetOffsets();
            offsets = packetOffsets?.CharacterEquipmentChanged ?? new byte[] { 0, 1, 2 };
            Id = Convert.ToInt32(parameters[offsets[0]]);
            Equipments = ConvertArray(parameters[offsets[1]]);
            Spells = ConvertArray(parameters[offsets[2]]);
        }

        public CharacterEquipmentChangedEvent(Dictionary<byte, object> parameters, PacketOffsets packetOffsets) : base(parameters)
        {
            offsets = packetOffsets?.CharacterEquipmentChanged ?? new byte[] { 0, 1, 2 };
            
            Id = Convert.ToInt32(parameters[offsets[0]]);
            Equipments = ConvertArray(parameters[offsets[1]]);
            Spells = ConvertArray(parameters[offsets[2]]);
        }

        public int Id { get; }

        public int[] Equipments { get; }

        public int[] Spells { get; }

        private int[] ConvertArray(object value)
        {
            int[] numArray1;
            switch (value)
            {
                case byte[] numArray2:
                    numArray1 = new int[numArray2.Length];
                    for (int index = 0; index < numArray2.Length; ++index)
                        numArray1[index] = (int)numArray2[index];
                    break;
                case short[] numArray3:
                    numArray1 = new int[numArray3.Length];
                    for (int index = 0; index < numArray3.Length; ++index)
                        numArray1[index] = (int)numArray3[index];
                    break;
                case float[] floatArray:
                    numArray1 = new int[floatArray.Length];
                    for (int index = 0; index < floatArray.Length; ++index)
                        numArray1[index] = (int)floatArray[index];
                    break;
                case double[] doubleArray:
                    numArray1 = new int[doubleArray.Length];
                    for (int index = 0; index < doubleArray.Length; ++index)
                        numArray1[index] = (int)doubleArray[index];
                    break;
                default:
                    numArray1 = (int[])value;
                    break;
            }
            return numArray1;
        }
    }
}
