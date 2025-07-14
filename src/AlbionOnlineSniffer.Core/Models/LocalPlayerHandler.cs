using System.Numerics;
using AlbionOnlineSniffer.Core.Interfaces;
namespace AlbionOnlineSniffer.Core.Models {
    public class LocalPlayerHandler : AlbionOnlineSniffer.Core.Interfaces.ILocalPlayerHandler {
        public Player LocalPlayer { get; set; }
    }
} 