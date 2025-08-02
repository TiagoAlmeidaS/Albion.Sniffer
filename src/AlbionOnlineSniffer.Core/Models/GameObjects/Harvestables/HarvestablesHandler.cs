using System.Collections.Concurrent;
using System.Numerics;
using System.Reflection;
using AlbionOnlineSniffer.Core.Utility;

namespace AlbionOnlineSniffer.Core.Models.GameObjects.Harvestables
{
    [Obfuscation(Feature = "mutation", Exclude = false)]
    public class HarvestablesHandler
    {
        public ConcurrentDictionary<int, Harvestable> harvestableList = new ConcurrentDictionary<int, Harvestable>();
        private readonly Dictionary<int, string> harvestableTypes = new Dictionary<int, string>();

        private readonly Localplayer.LocalPlayerHandler localPlayerHandler;
        public HarvestablesHandler(Dictionary<int, string> harvestableTypes, Localplayer.LocalPlayerHandler localPlayerHandler)
        {
            this.harvestableTypes = harvestableTypes ?? new Dictionary<int, string>();
            this.localPlayerHandler = localPlayerHandler;
        }

        public void AddHarvestable(int id, int type, int tier, Vector2 position, int count, int charge)
        {
            lock (harvestableList)
            {
                if (harvestableList.ContainsKey(id))
                    harvestableList.TryRemove(id, out Harvestable h);

                harvestableList.TryAdd(id, new Harvestable(id, LoadHarvestableType(type), tier, position, count, charge));
            }
        }

        public void RemoveHarvestables()
        {
            lock (harvestableList)
                harvestableList.RemoveAll(t => Vector2.Distance(t.Value.Position, localPlayerHandler.localPlayer.Position) > 70);
        }

        public void UpdateHarvestable(int id, int count, int charge)
        {
            lock (harvestableList)
            {
                if (harvestableList.TryGetValue(id, out Harvestable temp))
                {
                    temp.Count = count;
                    temp.Charge = charge;
                }
            }
        }

        public void Clear()
        {
            lock (harvestableList)
                harvestableList.Clear();
        }

        private string LoadHarvestableType(int type)
        {
            lock (harvestableList)
            {
                if (harvestableTypes != null && harvestableTypes.ContainsKey(type))
                    return harvestableTypes[type];

                return "null";
            }
        }
    }
}
