using System.Numerics;
using AlbionOnlineSniffer.Core.Interfaces;
using AlbionOnlineSniffer.Core.Models.GameObjects;

namespace AlbionOnlineSniffer.Core.Models {
    public class LocalPlayerHandler : AlbionOnlineSniffer.Core.Interfaces.ILocalPlayerHandler {
        public Player LocalPlayer { get; set; }
    }
} 