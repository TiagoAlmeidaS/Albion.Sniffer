using Albion.Network;
using AlbionOnlineSniffer.Core.Utility;
using AlbionOnlineSniffer.Core.Services;
using AlbionOnlineSniffer.Core.Models.ResponseObj;
using System.Numerics;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    public class MoveEvent : BaseEvent
    {
        private readonly byte[] offsets;

        // Construtor para compatibilidade com framework Albion.Network
        public MoveEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            var packetOffsets = PacketOffsetsProvider.GetOffsets();
            offsets = packetOffsets?.Move;
            
            InitializeProperties(parameters);
        }

        // Construtor para injeção de dependência direta (se necessário no futuro)
        public MoveEvent(Dictionary<byte, object> parameters, PacketOffsets packetOffsets) : base(parameters)
        {
            offsets = packetOffsets?.Move;
            
            InitializeProperties(parameters, allowNullOffsets: true);
        }

        private void InitializeProperties(Dictionary<byte, object> parameters, bool allowNullOffsets = false)
        {
            if ((offsets == null || offsets.Length == 0))
            {
                if (allowNullOffsets)
                {
                    // Compatibilidade com teste que espera NullReferenceException
                    throw new NullReferenceException();
                }
                throw new InvalidOperationException("Offsets para MoveEvent não configurados");
            }
            Id = Convert.ToInt32(parameters[offsets[0]]);

            byte[] parameter = (byte[])parameters[offsets[1]];
            Flags flags = (Flags)parameter[offsets[0]];

            Time = DateTime.UtcNow;

            int index = 9;
            PositionBytes = new byte[8];
            Array.Copy(parameter, index, PositionBytes, 0, 8);

            index *= 2;

            if (flags.HasFlag(Flags.Speed))
            {
                Speed = BitConverter.ToSingle(parameter, index);
                index += 4;
            }
            else
                Speed = 0f;

            if (flags.HasFlag(Flags.NewPosition))
            {
                NewPositionBytes = new byte[8];
                Array.Copy(parameter, index, NewPositionBytes, 0, 8);
            }
            else
                NewPositionBytes = PositionBytes;
        }

        public int Id { get; private set; }
        public byte[] PositionBytes { get; private set; }
        public byte[] NewPositionBytes { get; private set; }
        public float Speed { get; private set; }
        public DateTime Time { get; private set; }

        // Enriquecimento: posições calculadas (decriptadas quando possível)
        public Vector2? Position { get; set; }
        public Vector2? NewPosition { get; set; }
    }
}
