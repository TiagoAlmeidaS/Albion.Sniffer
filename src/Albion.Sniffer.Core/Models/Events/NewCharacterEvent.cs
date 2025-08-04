using System.Numerics;
using System.Reflection;
using Albion.Network;
using Albion.Sniffer.Core.Models.GameObjects.Players;
using Albion.Sniffer.Core.Services;

namespace Albion.Sniffer.Core.Models.Events
{
    [Obfuscation(Feature = "mutation", Exclude = false)]
    public class NewCharacterEvent : BaseEvent
    {
        byte[] offsets = PacketOffsetsLoader.GlobalPacketOffsets?.NewCharacter;

        public NewCharacterEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            Id = Convert.ToInt32(parameters[offsets[0]]);

            Name = parameters[offsets[1]] as string;
            Guild = parameters.ContainsKey(offsets[2]) ? parameters[offsets[2]] as string : string.Empty;
            Alliance = parameters.ContainsKey(offsets[3]) ? parameters[offsets[3]] as string : string.Empty;
            Faction = (Albion.Sniffer.Core.Utility.Faction)parameters[offsets[4]];
            
            EncryptedPosition = parameters[offsets[5]] as byte[];
            Speed = parameters.ContainsKey(offsets[6]) ? (float)parameters[offsets[6]] : 5.5f;

            Health = parameters.ContainsKey(offsets[7]) ?
                new Health(Convert.ToInt32(parameters[offsets[7]]), Convert.ToInt32(parameters[offsets[8]]))
                : new Health(Convert.ToInt32(parameters[offsets[8]]));

            Equipments = ConvertArray(parameters[offsets[9]]);
            Spells = ConvertArray(parameters[offsets[10]]);
        }

        public int Id { get; }

        public string Name { get; }
        public string Guild { get; }
        public string Alliance { get; }
        public Albion.Sniffer.Core.Utility.Faction Faction { get; }

        public Vector2 Position { get; }
        public byte[] EncryptedPosition { get; }
        public float Speed { get; }

        public Health Health { get; }

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
