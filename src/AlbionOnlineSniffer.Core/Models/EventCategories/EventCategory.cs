using System.Collections.Generic;

namespace AlbionOnlineSniffer.Core.Models.EventCategories
{
    /// <summary>
    /// Categoria de eventos para organização e filtros
    /// </summary>
    public class EventCategory
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public EventType Type { get; set; }
        public List<string> EventNames { get; set; } = new();
        public List<int> PacketCodes { get; set; } = new();
        public bool IsEnabled { get; set; } = true;
        public int Priority { get; set; } = 0;
    }

    /// <summary>
    /// Tipo de evento (Discovery ou UDP)
    /// </summary>
    public enum EventType
    {
        Discovery,
        UDP
    }

    /// <summary>
    /// Configuração de filtros para eventos
    /// </summary>
    public class EventFilterConfig
    {
        public List<string> EnabledCategories { get; set; } = new();
        public List<string> DisabledEventNames { get; set; } = new();
        public List<int> DisabledPacketCodes { get; set; } = new();
        public bool ShowHiddenPackets { get; set; } = false;
        public bool ShowVisiblePackets { get; set; } = true;
        public int MinPacketCount { get; set; } = 0;
        public string SearchTerm { get; set; } = string.Empty;
    }
}
