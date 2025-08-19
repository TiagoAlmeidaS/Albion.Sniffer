namespace AlbionOnlineSniffer.Core.Models.ResponseObj
{
    public class MobInfo
    {
        public int Id { get; set; }
        public int Tier { get; set; }
        public string Type { get; set; }
        public string HarvestableType { get; set; }
        public int Rarity { get; set; }
        public string Queue { get; set; }
        public string MobName { get; set; }

        public MobInfo()
        {
            Type = string.Empty;
            HarvestableType = string.Empty;
            Queue = string.Empty;
            MobName = string.Empty;
        }

        public MobInfo(int id, int tier, string type, string harvestableType, int rarity, string queue, string mobName)
        {
            Id = id;
            Tier = tier;
            Type = type ?? string.Empty;
            HarvestableType = harvestableType ?? string.Empty;
            Rarity = rarity;
            Queue = queue ?? string.Empty;
            MobName = mobName ?? string.Empty;
        }
    }
}
