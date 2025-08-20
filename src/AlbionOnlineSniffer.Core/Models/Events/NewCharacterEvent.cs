using System.Numerics;
using System.Reflection;
using Albion.Network;
using AlbionOnlineSniffer.Core.Models.GameObjects.Players;
using AlbionOnlineSniffer.Core.Services;
using System.Collections.Generic;
using AlbionOnlineSniffer.Core.Models.ResponseObj;
using AlbionOnlineSniffer.Core.Utility;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    [Obfuscation(Feature = "mutation", Exclude = false)]
    public class NewCharacterEvent : BaseEvent, IHasPosition
    {
        private readonly byte[] offsets;

        // Construtor para compatibilidade com framework Albion.Network
        public NewCharacterEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            var packetOffsets = PacketOffsetsLoader.GlobalPacketOffsets ?? PacketOffsetsProvider.GetOffsets();
            offsets = packetOffsets?.NewCharacter ?? new byte[] { 0, 1, 2, 3, 4, 5 };
            
            InitializeProperties(parameters);
        }

        // Construtor para injeção de dependência direta (se necessário no futuro)
        public NewCharacterEvent(Dictionary<byte, object> parameters, PacketOffsets packetOffsets) : base(parameters)
        {
            offsets = packetOffsets?.NewCharacter ?? new byte[] { 0, 1, 2, 3, 4, 5 };
            
            InitializeProperties(parameters);
        }

        private void InitializeProperties(Dictionary<byte, object> parameters)
        {
            // ✅ DEBUG: Log dos offsets e parâmetros recebidos
            var debugInfo = $"Offsets: [{string.Join(", ", offsets)}], Parameters: [{string.Join(", ", parameters.Keys)}]";
            System.Diagnostics.Debug.WriteLine($"🔍 NewCharacterEvent.InitializeProperties: {debugInfo}");
            
            try
            {
                // ✅ SEGURO: Usar SafeParameterExtractor para evitar KeyNotFoundException
                Id = SafeParameterExtractor.GetInt32(parameters, offsets[0]);
                Name = SafeParameterExtractor.GetString(parameters, offsets[1]);
                GuildName = SafeParameterExtractor.GetString(parameters, offsets[2]);
                AllianceName = SafeParameterExtractor.GetString(parameters, offsets[3]);

            // ✅ SEGURO: Usar SafeParameterExtractor para arrays
            PositionBytes = SafeParameterExtractor.GetByteArray(parameters, offsets[4]);
            Items = SafeParameterExtractor.GetFloatArray(parameters, offsets[5]);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ ERRO em NewCharacterEvent.InitializeProperties: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"🔍 Offsets: [{string.Join(", ", offsets)}]");
                System.Diagnostics.Debug.WriteLine($"🔍 Parameters: [{string.Join(", ", parameters.Keys)}]");
                throw; // Re-throw para manter o comportamento original
            }
        }

        public int Id { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public string GuildName { get; private set; } = string.Empty;
        public string AllianceName { get; private set; } = string.Empty;
        public byte[] PositionBytes { get; private set; } = Array.Empty<byte>();
        public float[] Items { get; private set; } = Array.Empty<float>();

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
