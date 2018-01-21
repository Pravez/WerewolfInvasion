using System.Collections.Generic;
using UnityEngine;
using Voronoi;

namespace Generation
{
    public static class World
    {
        public static int Worldsize = 10;
        public static int WorldTotalSize = Worldsize * Worldsize;
        public static int Size = 60;

        public static int MiniWorldSize = 200;
        
        public static Dictionary<Vector2Int, List<City>> Villages = new Dictionary<Vector2Int, List<City>>();

        public static void AddVillage(Vector2 position, bool isLabyrinth)
        {
            Vector2Int chunkpos = new Vector2Int((int) position.x, (int) position.y);
            City toAdd = new City(position);
            if (isLabyrinth)
                toAdd.SetSizeBetween(5, 10);
            if (Villages.ContainsKey(chunkpos))
            {
                Villages[chunkpos].Add(toAdd);
            }
            else
            {
                Villages.Add(chunkpos, new List<City>(){toAdd});
            }
        }

        public static bool TryGetVillage(Vector2Int position, out List<City> village)
        {
            position = new Vector2Int(Mathf.Abs(position.x), Mathf.Abs(position.y));
            village = null;
            if (Villages.ContainsKey(position))
            {
                village = Villages[position];
                return true;
            }
            return false;
        }
    }
}