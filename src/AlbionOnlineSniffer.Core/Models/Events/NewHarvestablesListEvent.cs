using Albion.Network;
using AlbionOnlineSniffer.Core.Models.ResponseObj;
using AlbionOnlineSniffer.Core.Services;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    public class NewHarvestablesListEvent : BaseEvent
    {
        private List<NewHarvestableEvent> harvestableObjects;

        // Construtor para compatibilidade com framework Albion.Network
        public NewHarvestablesListEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            var packetOffsets = PacketOffsetsProvider.GetOffsets();
            InitializeHarvestableObjects(parameters, packetOffsets);
        }

        // Construtor para injeção de dependência direta (se necessário no futuro)
        public NewHarvestablesListEvent(Dictionary<byte, object> parameters, PacketOffsets packetOffsets) : base(parameters)
        {
            InitializeHarvestableObjects(parameters, packetOffsets);
        }

        private void InitializeHarvestableObjects(Dictionary<byte, object> parameters, PacketOffsets packetOffsets)
        {
            harvestableObjects = new List<NewHarvestableEvent>();

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
                        { 0, (int)ids[i] },
                        { 1, (int)types[i] },
                        { 2, new byte[] { BitConverter.GetBytes(positions[i * 2])[0], BitConverter.GetBytes(positions[i * 2])[1], BitConverter.GetBytes(positions[i * 2])[2], BitConverter.GetBytes(positions[i * 2])[3], BitConverter.GetBytes(positions[i * 2 + 1])[0], BitConverter.GetBytes(positions[i * 2 + 1])[1], BitConverter.GetBytes(positions[i * 2 + 1])[2], BitConverter.GetBytes(positions[i * 2 + 1])[3] } },
                        { 3, (byte)tiers[i] },
                        { 4, (byte)sizes[i] }
                    };

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
                        { 0, (int)ids[i] },
                        { 1, (int)types[i] },
                        { 2, new byte[] { BitConverter.GetBytes(positions[i * 2])[0], BitConverter.GetBytes(positions[i * 2])[1], BitConverter.GetBytes(positions[i * 2])[2], BitConverter.GetBytes(positions[i * 2])[3], BitConverter.GetBytes(positions[i * 2 + 1])[0], BitConverter.GetBytes(positions[i * 2 + 1])[1], BitConverter.GetBytes(positions[i * 2 + 1])[2], BitConverter.GetBytes(positions[i * 2 + 1])[3] } },
                        { 3, (byte)tiers[i] },
                        { 4, (byte)sizes[i] }
                    };

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
