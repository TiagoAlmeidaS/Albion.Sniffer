using System.Collections.Generic;

namespace AlbionOnlineSniffer.Core.Models
{
    /// <summary>
    /// Mapeamento de offsets para extração de dados dos pacotes Photon
    /// Baseado no albion-radar-deatheye-2pc
    /// </summary>
    public class PacketOffsets
    {
        public byte[] ChangeCluster { get; set; } = new byte[0];
        public byte[] ChangeFlaggingFinished { get; set; } = new byte[0];
        public byte[] CharacterEquipmentChanged { get; set; } = new byte[0];
        public byte[] HarvestableChangeState { get; set; } = new byte[0];
        public byte[] HealthUpdateEvent { get; set; } = new byte[0];
        public byte[] JoinResponse { get; set; } = new byte[0];
        public byte[] Leave { get; set; } = new byte[0];
        public byte[] MobChangeState { get; set; } = new byte[0];
        public byte[] Mounted { get; set; } = new byte[0];
        public byte[] Move { get; set; } = new byte[0];
        public byte[] MoveRequest { get; set; } = new byte[0];
        public byte[] NewCharacter { get; set; } = new byte[0];
        public byte[] NewDungeonExit { get; set; } = new byte[0];
        public byte[] NewDungeon { get; set; } = new byte[0];
        public byte[] NewFishingZoneObject { get; set; } = new byte[0];
        public byte[] NewHarvestableObject { get; set; } = new byte[0];
        public byte[] NewLootChest { get; set; } = new byte[0];
        public byte[] NewMobEvent { get; set; } = new byte[0];
        public byte[] NewWispGate { get; set; } = new byte[0];
        public byte[] WispGateOpened { get; set; } = new byte[0];
        public byte[] RegenerationHealthChangedEvent { get; set; } = new byte[0];
        public byte[] KeySync { get; set; } = new byte[0];
    }
} 