namespace AlbionOnlineSniffer.Core.Models.ResponseObj
{
    public class PacketOffsets
    {
        public byte[] ChangeCluster { get; set; }
        public byte[] ChangeFlaggingFinished { get; set; }
        public byte[] CharacterEquipmentChanged { get; set; }
        public byte[] HarvestableChangeState { get; set; }
        public byte[] HealthUpdateEvent { get; set; }
        public byte[] JoinResponse { get; set; }
        public byte[] Leave { get; set; }
        public byte[] MobChangeState { get; set; }
        public byte[] Mounted { get; set; }
        public byte[] Move { get; set; }
        public byte[] MoveRequest { get; set; }
        public byte[] NewCharacter { get; set; }
        public byte[] NewDungeonExit { get; set; }
        public byte[] NewFishingZoneObject { get; set; }
        public byte[] NewHarvestableObject { get; set; }
        public byte[] NewLootChest { get; set; }
        public byte[] NewMobEvent { get; set; }
        public byte[] NewWispGate { get; set; }
        public byte[] WispGateOpened { get; set; }
        public byte[] RegenerationHealthChangedEvent { get; set; }
        public byte[] KeySync { get; set; }

        public PacketOffsets()
        {
            ChangeCluster = Array.Empty<byte>();
            ChangeFlaggingFinished = Array.Empty<byte>();
            CharacterEquipmentChanged = Array.Empty<byte>();
            HarvestableChangeState = Array.Empty<byte>();
            HealthUpdateEvent = Array.Empty<byte>();
            JoinResponse = Array.Empty<byte>();
            Leave = Array.Empty<byte>();
            MobChangeState = Array.Empty<byte>();
            Mounted = Array.Empty<byte>();
            Move = Array.Empty<byte>();
            MoveRequest = Array.Empty<byte>();
            NewCharacter = Array.Empty<byte>();
            NewDungeonExit = Array.Empty<byte>();
            NewFishingZoneObject = Array.Empty<byte>();
            NewHarvestableObject = Array.Empty<byte>();
            NewLootChest = Array.Empty<byte>();
            NewMobEvent = Array.Empty<byte>();
            NewWispGate = Array.Empty<byte>();
            WispGateOpened = Array.Empty<byte>();
            RegenerationHealthChangedEvent = Array.Empty<byte>();
            KeySync = Array.Empty<byte>();
        }
    }
}
