using System.Numerics;
using Albion.Network;
using AlbionOnlineSniffer.Core.Services;
using AlbionOnlineSniffer.Core.Models.ResponseObj;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    public class ChangeClusterEvent : BaseOperation
    {
        private readonly byte[] offsets;

        // Construtor para compatibilidade com framework Albion.Network
        public ChangeClusterEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            PacketOffsets? packetOffsets = null;
            try { packetOffsets = PacketOffsetsProvider.GetOffsets(); }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException("PacketOffsetsProvider não foi configurado. Chame Configure() primeiro.", ex);
            }
            offsets = packetOffsets?.ChangeCluster ?? new byte[] { 0 };

            LocationId = parameters.TryGetValue(offsets[0], out var loc) ? loc as string : null;
            Type = (offsets.Length > 1 && parameters.TryGetValue(offsets[1], out var typeObj)) ? typeObj as string : "NULL";
            DynamicClusterData = (offsets.Length > 2 && parameters.TryGetValue(offsets[2], out var dataObj) && dataObj is byte[] bytes)
                ? ReadClusterData(bytes)
                : null;
        }

        // Construtor para injeção de dependência direta (se necessário no futuro)
        public ChangeClusterEvent(Dictionary<byte, object> parameters, PacketOffsets packetOffsets) : base(parameters)
        {
            offsets = packetOffsets?.ChangeCluster ?? new byte[] { 0 };
            
            LocationId = parameters[offsets[0]] as string;
            Type = parameters.ContainsKey(offsets[1]) ? parameters[offsets[1]] as string : "NULL";
            DynamicClusterData = parameters.ContainsKey(offsets[2]) && parameters[offsets[2]] is byte[]
                ? ReadClusterData(parameters[offsets[2]] as byte[])
                : null;
        }

        public string LocationId { get; }

        public string Type { get; }

        public DynamicClusterData DynamicClusterData { get; }


        private static DynamicClusterData ReadClusterData(byte[] data)
        {
            try
            {
                var clusterData = new DynamicClusterData();

                using var ms = new MemoryStream(data);
                using var reader = new BinaryReader(ms);

                clusterData.Type = reader.ReadByte();

                if (clusterData.Type == 1)
                {
                    clusterData.ClusterId = reader.ReadString();
                    clusterData.ClusterName = reader.ReadString();
                    clusterData.UnknownVector1 = new Vector2(reader.ReadSingle(), reader.ReadSingle());
                    clusterData.UnknownVector2 = new Vector2(reader.ReadSingle(), reader.ReadSingle());
                    reader.ReadBytes(0x2B); // unknown
                    clusterData.TemplateInstances = new List<TemplateInstance>();

                    for (int templateInstancesCount = reader.ReadInt32(), i = 0;
                         i < templateInstancesCount;
                         ++i)
                    {
                        var layers = new List<string>();

                        var templateInstanceId = reader.ReadString();
                        var templateName = reader.ReadString();
                        var position = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                        var rotation = reader.ReadSingle();
                        reader.ReadBytes(1); // unknown

                        for (int layersCount = reader.ReadInt32(), j = 0;
                             j < layersCount;
                             ++j)
                        {
                            layers.Add(reader.ReadString());
                        }

                        clusterData.TemplateInstances.Add(new TemplateInstance
                        {
                            TemplateInstanceId = templateInstanceId,
                            TemplateName = templateName,
                            Position = position,
                            Rotation = rotation,
                            Layers = layers
                        });
                    }

                    clusterData.ClusterName = reader.ReadString();
                    clusterData.ClusterType = reader.ReadString();
                    clusterData.Level = reader.ReadByte();
                    reader.ReadBytes(2); // unknown

                    clusterData.Exits = new List<DynamicExit>();
                    for (var i = 0; i < 2; ++i)
                    {
                        clusterData.Exits.Add(new DynamicExit
                        {
                            Str1 = reader.ReadString(),
                            Str2 = reader.ReadString(),
                            Str3 = reader.ReadString(),
                            Str4 = reader.ReadString()
                        });
                    }

                    return clusterData;
                }
            }
            catch (Exception)
            {
                Console.Error.WriteLine("Can't read cluster data");
            }

            return null;
        }
    }

    public class DynamicClusterData
    {
        public byte Type;
        public string ClusterId;
        public string LongClusterId;
        public Vector2 UnknownVector1;
        public Vector2 UnknownVector2;
        public List<TemplateInstance> TemplateInstances;
        public string ClusterName;
        public string ClusterType;
        public byte Level;
        public byte UnknownByte3;
        public List<DynamicExit> Exits;

        public bool HasNextFloor => Exits?.Count == 2 && !string.IsNullOrWhiteSpace(Exits[1].Str1);
    }

    public class DynamicExit
    {
        public string Str1;
        public string Str2;
        public string Str3;
        public string Str4;
    }

    public class TemplateInstance
    {
        public string TemplateInstanceId;
        public string TemplateName;
        public Vector3 Position;
        public float Rotation;
        public List<string> Layers;
    }
}