using AlbionOnlineSniffer.Core.Handlers;

namespace AlbionOnlineSniffer.Core
{
    public static class DependencyProvider
    {
        public static IPlayersHandler CreatePlayersHandler() => new PlayersHandler();
        public static IMobsHandler CreateMobsHandler() => new MobsHandler();
        public static IHarvestablesHandler CreateHarvestablesHandler() => new HarvestablesHandler();
        public static ILootChestsHandler CreateLootChestsHandler() => new LootChestsHandler();
        public static IDungeonsHandler CreateDungeonsHandler() => new DungeonsHandler();
        public static IFishNodesHandler CreateFishNodesHandler() => new FishNodesHandler();
        public static IGatedWispsHandler CreateGatedWispsHandler() => new GatedWispsHandler();
        public static IConfigHandler CreateConfigHandler() => new ConfigHandler();
        public static ILocalPlayerHandler CreateLocalPlayerHandler() => new LocalPlayerHandler();
        // Adicione outros factories conforme necessário
    }
} 