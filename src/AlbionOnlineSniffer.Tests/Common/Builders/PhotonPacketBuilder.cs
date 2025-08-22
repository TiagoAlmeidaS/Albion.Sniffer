namespace AlbionOnlineSniffer.Tests.Common.Builders;

/// <summary>
/// Builder para criar pacotes Photon sintéticos para testes
/// </summary>
public class PhotonPacketBuilder
{
    private readonly List<byte> _packet = new();

    public PhotonPacketBuilder()
    {
        // Inicializa com header Photon básico
        _packet.AddRange(new byte[] { 0xF3, 0x01 }); // Magic bytes do Photon
    }

    public PhotonPacketBuilder WithOperationCode(byte opCode)
    {
        _packet.Add(opCode);
        return this;
    }

    public PhotonPacketBuilder WithEventCode(byte eventCode)
    {
        _packet.Add(0x02); // Event type
        _packet.Add(eventCode);
        return this;
    }

    public PhotonPacketBuilder WithPayload(byte[] payload)
    {
        _packet.AddRange(payload);
        return this;
    }

    public PhotonPacketBuilder WithPlayerSpottedData(
        string playerName = "TestPlayer",
        int tier = 5,
        float x = 100f,
        float y = 200f,
        float z = 0f)
    {
        // Simula estrutura de um evento PlayerSpotted
        _packet.Add(0x02); // Event type
        _packet.Add(0x10); // Event code para PlayerSpotted (exemplo)
        
        // Nome do jogador
        var nameBytes = System.Text.Encoding.UTF8.GetBytes(playerName);
        _packet.Add((byte)nameBytes.Length);
        _packet.AddRange(nameBytes);
        
        // Tier
        _packet.Add((byte)tier);
        
        // Posição (simplificado)
        _packet.AddRange(BitConverter.GetBytes(x));
        _packet.AddRange(BitConverter.GetBytes(y));
        _packet.AddRange(BitConverter.GetBytes(z));
        
        return this;
    }

    public PhotonPacketBuilder WithMobSpawnedData(
        string mobType = "Wolf",
        int tier = 3,
        float x = 150f,
        float y = 250f,
        float z = 0f,
        int health = 1000)
    {
        // Simula estrutura de um evento MobSpawned
        _packet.Add(0x02); // Event type
        _packet.Add(0x11); // Event code para MobSpawned (exemplo)
        
        // Tipo do mob
        var typeBytes = System.Text.Encoding.UTF8.GetBytes(mobType);
        _packet.Add((byte)typeBytes.Length);
        _packet.AddRange(typeBytes);
        
        // Tier
        _packet.Add((byte)tier);
        
        // Posição
        _packet.AddRange(BitConverter.GetBytes(x));
        _packet.AddRange(BitConverter.GetBytes(y));
        _packet.AddRange(BitConverter.GetBytes(z));
        
        // Health
        _packet.AddRange(BitConverter.GetBytes(health));
        
        return this;
    }

    public PhotonPacketBuilder WithHarvestableData(
        string resourceType = "Wood",
        int tier = 4,
        float x = 300f,
        float y = 400f,
        float z = 0f,
        int charges = 5)
    {
        // Simula estrutura de um evento Harvestable
        _packet.Add(0x02); // Event type
        _packet.Add(0x12); // Event code para Harvestable (exemplo)
        
        // Tipo do recurso
        var typeBytes = System.Text.Encoding.UTF8.GetBytes(resourceType);
        _packet.Add((byte)typeBytes.Length);
        _packet.AddRange(typeBytes);
        
        // Tier
        _packet.Add((byte)tier);
        
        // Posição
        _packet.AddRange(BitConverter.GetBytes(x));
        _packet.AddRange(BitConverter.GetBytes(y));
        _packet.AddRange(BitConverter.GetBytes(z));
        
        // Charges
        _packet.AddRange(BitConverter.GetBytes(charges));
        
        return this;
    }

    public byte[] Build()
    {
        // Adiciona checksum simples no final (simulado)
        var checksum = _packet.Sum(b => b) % 256;
        _packet.Add((byte)checksum);
        
        return _packet.ToArray();
    }

    /// <summary>
    /// Cria um pacote inválido para testes de erro
    /// </summary>
    public static byte[] CreateInvalidPacket()
    {
        return new byte[] { 0x00, 0x00, 0xFF, 0xFF }; // Pacote inválido
    }

    /// <summary>
    /// Cria um pacote vazio válido
    /// </summary>
    public static byte[] CreateEmptyPacket()
    {
        return new byte[] { 0xF3, 0x01, 0x00, 0xF4 }; // Header + checksum
    }
}