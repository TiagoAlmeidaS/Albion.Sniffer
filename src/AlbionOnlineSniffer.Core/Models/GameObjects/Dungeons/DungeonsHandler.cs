using System.Collections.Concurrent;
using System.Numerics;
using System.Reflection;

namespace AlbionOnlineSniffer.Core.Models.GameObjects.Dungeons
{
    [Obfuscation(Feature = "mutation", Exclude = false)]
    public class DungeonsHandler
    {
        public ConcurrentDictionary<int, X975.Radar.GameObjects.Dungeons.Dungeon> dungeonsList = new ConcurrentDictionary<int, X975.Radar.GameObjects.Dungeons.Dungeon>();

        public void AddDungeon(int id, string type, Vector2 position, int charges)
        {
            lock (dungeonsList)
            {
                if (dungeonsList.ContainsKey(id))
                    dungeonsList.TryRemove(id, out X975.Radar.GameObjects.Dungeons.Dungeon d);

                dungeonsList.TryAdd(id, new X975.Radar.GameObjects.Dungeons.Dungeon(id, type, position, charges));
            }
        }

        public void Remove(int id)
        {
            lock (dungeonsList)
                dungeonsList.TryRemove(id, out X975.Radar.GameObjects.Dungeons.Dungeon d);
        }

        public void Clear()
        {
            lock (dungeonsList)
                dungeonsList.Clear();
        }
    }
}
