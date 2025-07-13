using System.Collections.Generic;
namespace AlbionOnlineSniffer.Core.Models {
    public class Equipment {
        public List<PlayerItems> Items { get; set; } = new();
        public int AllItemPower { get; set; }
    }
} 