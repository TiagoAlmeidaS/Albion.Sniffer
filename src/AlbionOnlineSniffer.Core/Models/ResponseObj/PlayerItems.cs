namespace AlbionOnlineSniffer.Core.Models.ResponseObj
{
    public class PlayerItems
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Itempower { get; set; }

        public PlayerItems()
        {
            Name = string.Empty;
        }

        public PlayerItems(int id, string name, int itempower)
        {
            Id = id;
            Name = name ?? string.Empty;
            Itempower = itempower;
        }
    }
}
