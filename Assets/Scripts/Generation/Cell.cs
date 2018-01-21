using UnityEngine;
using Random = System.Random;

namespace NLabyrinth
{
    public class Cell
    {
        public Cell top;
        public Cell bottom;
        public Cell left;
        public Cell right;

        public Vector2Int pos;

        public bool visited;

        public Cell(Vector2Int pos)
        {
            this.pos = pos;
            visited = false;
        }

        public void BreakWallAndUpdateNeighbourhood(AroundCell around)
        {
            Random r = new Random();

            Cell selected = null;

            int value = 0;

            while (selected == null)
            {
                value = r.Next(4);
                switch (value)
                {
                    case 0:
                        selected = around.right;
                        break;
                    case 1:
                        selected = around.top;
                        break;
                    case 2:
                        selected = around.bottom;
                        break;
                    case 3:
                        selected = around.left;
                        break;
                }
            }

            switch (value)
            {
                case 0:
                    right = selected;
                    selected.left = right;
                    break;
                case 1:
                    top = selected;
                    selected.bottom = top;
                    break;
                case 2:
                    bottom = selected;
                    selected.top = bottom;
                    break;
                case 3:
                    left = selected;
                    selected.right = left;
                    break;
            }

            visited = true;
        }
    }
}