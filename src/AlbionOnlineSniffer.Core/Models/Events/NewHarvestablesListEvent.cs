using Albion.Network;
using AlbionOnlineSniffer.Core.Models.ResponseObj;
using AlbionOnlineSniffer.Core.Services;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    public class NewHarvestablesListEvent : BaseEvent
    {
        private readonly List<NewHarvestableEvent> harvestableObjects;

        // Construtor para compatibilidade com framework Albion.Network
        public NewHarvestablesListEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            harvestableObjects = new List<NewHarvestableEvent>();
            var packetOffsets = PacketOffsetsProvider.GetOffsets();
            InitializeHarvestableObjects(parameters, packetOffsets);
        }

        // Construtor para injeção de dependência direta (se necessário no futuro)
        public NewHarvestablesListEvent(Dictionary<byte, object> parameters, PacketOffsets packetOffsets) : base(parameters)
        {
            harvestableObjects = new List<NewHarvestableEvent>();
            InitializeHarvestableObjects(parameters, packetOffsets);
        }

        private void InitializeHarvestableObjects(Dictionary<byte, object> parameters, PacketOffsets packetOffsets)
        {
            // lista já inicializada nos construtores

            if (parameters[0] is byte[])
            {
                var ids = (byte[])parameters[0];
                var types = (byte[])parameters[1];
                var tiers = (byte[])parameters[2];
                var positions = (float[])parameters[3];
                var sizes = (byte[])parameters[4];

                for (int i = 0; i < ids.Length; i++)
                {
                    var harvestParameters = new Dictionary<byte, object>
                    {
                        { 0, ids[i] },
                        { 1, types[i] },
                        { 2, new byte[] { 0,0,0,0, 0,0,0,0 } },
                        { 3, tiers[i] },
                        { 4, sizes[i] }
                    };

                    // Monta PositionBytes (x,y floats) em byte[]
                    var xBytes = BitConverter.GetBytes(positions[i * 2]);
                    var yBytes = BitConverter.GetBytes(positions[i * 2 + 1]);
                    var posBytes = new byte[8];
                    Array.Copy(xBytes, 0, posBytes, 0, 4);
                    Array.Copy(yBytes, 0, posBytes, 4, 4);
                    // como a lista é readonly, definimos antes de criar o objeto; não tocar após add
                    harvestParameters[2] = posBytes;

                    harvestableObjects.Add(new NewHarvestableEvent(harvestParameters, packetOffsets));
                }
            }
            else if (parameters[0] is short[])
            {
                var ids = (short[])parameters[0];
                var types = (byte[])parameters[1];
                var tiers = (byte[])parameters[2];
                var positions = (float[])parameters[3];
                var sizes = (byte[])parameters[4];

                for (int i = 0; i < ids.Length; i++)
                {
                    var harvestParameters = new Dictionary<byte, object>
                    {
                        { 0, ids[i] },
                        { 1, types[i] },
                        { 2, new byte[] { 0,0,0,0, 0,0,0,0 } },
                        { 3, tiers[i] },
                        { 4, sizes[i] }
                    };

                    // Monta PositionBytes
                    var xBytes = BitConverter.GetBytes(positions[i * 2]);
                    var yBytes = BitConverter.GetBytes(positions[i * 2 + 1]);
                    var posBytes = new byte[8];
                    Array.Copy(xBytes, 0, posBytes, 0, 4);
                    Array.Copy(yBytes, 0, posBytes, 4, 4);
                    harvestParameters[2] = posBytes;

                    harvestableObjects.Add(new NewHarvestableEvent(harvestParameters, packetOffsets));
                }
            }
        }

        public IReadOnlyCollection<NewHarvestableEvent> HarvestableObjects
        {
            get
            {
                return harvestableObjects;
            }
        }
    }
}
