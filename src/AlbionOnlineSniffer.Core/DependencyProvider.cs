using AlbionOnlineSniffer.Core.Handlers;
using AlbionOnlineSniffer.Core.Interfaces;
using AlbionOnlineSniffer.Core.Models;

namespace AlbionOnlineSniffer.Core
{
    public static class DependencyProvider
    {
        public static IPlayersManager CreatePlayersManager() => new PlayersManager(new List<PlayerItems>());
        public static IMobsManager CreateMobsManager() => new MobsManager(new List<MobInfo>());
        public static IHarvestablesManager CreateHarvestablesManager() => new HarvestablesManager(new Dictionary<int, string>(), new LocalPlayerHandler());
        public static ILootChestsManager CreateLootChestsManager() => new LootChestsManager();
        public static IDungeonsManager CreateDungeonsManager() => new DungeonsManager();
        public static IFishNodesManager CreateFishNodesManager() => new FishNodesManager();
        public static IGatedWispsManager CreateGatedWispsManager() => new GatedWispsManager();
        public static IConfigHandler CreateConfigHandler() => new ConfigHandler();
        public static ILocalPlayerHandler CreateLocalPlayerHandler() => new LocalPlayerHandler();
        // Adicione outros factories conforme necessário
    }
} 