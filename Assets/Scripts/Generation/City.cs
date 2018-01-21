using UnityEngine;

namespace Voronoi
{
    public class City
    {
        public Vector2 position;
        public int size;

        public City(Vector2 pos, int size = 1)
        {
            position = pos;
            this.size = size;
        }

        public City SetSize(int size)
        {
            this.size = size;
            return this;
        }

        public City SetSizeBetween(int sizeMin, int sizeMax)
        {
            this.size = Random.Range(sizeMin, sizeMax);
            return this;
        }
    }
}