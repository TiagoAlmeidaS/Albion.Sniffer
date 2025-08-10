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
                        { 0, ids[i] },
                        { 5, types[i] },
                        { 7, tiers[i] },
                        { 8, new float[] { positions[i * 2], positions[i * 2 + 1] } },
                        { 10, sizes[i] }
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
                        { 0, ids[i] },
                        { 5, types[i] },
                        { 7, tiers[i] },
                        { 8, new float[] { positions[i * 2], positions[i * 2 + 1] } },
                        { 10, sizes[i] }
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
