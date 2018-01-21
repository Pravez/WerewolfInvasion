using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NLabyrinth
{
    public struct AroundCell
    {
        public Cell top;
        public Cell bottom;
        public Cell left;
        public Cell right;

        public List<Cell> ToList()
        {
            return new List<Cell> {top, bottom, left, right}
                .Where(c => c != null)
                .ToList();
        }
    }

    public class LabyrinthGenerator
    {
        public Cell[,] cells;
        private System.Random random;
        public Vector2Int size;
        public Vector2Int stairs;
        public Vector2Int entry;
        public Vector2Int chest;

        private Dictionary<Vector2Int, Cell> frontier;

        public LabyrinthGenerator(Vector2Int size)
        {
            this.size = size;
            cells = new Cell[size.x,size.y];
            frontier = new Dictionary<Vector2Int, Cell>();

            random = new System.Random();

            InitLabyrinth();
            CreateLabyrinth();
        }

        public void InitLabyrinth()
        {
            for (int i = 0; i < size.x; ++i)
            {
                for(int j = 0;j < size.y; ++j)
                {
                    cells[i, j] = new Cell(new Vector2Int(i, j));
                }
            }
        }

        public void CreateLabyrinth()
        {
            int checkedCells = 0;
            Cell tmp;
            AroundCell around;

            tmp = cells[random.Next(size.x), random.Next(size.y)];
            tmp.visited = true;
            around = GetNeighboursOf(tmp);

            around.ToList()
                .Where(c => !frontier.ContainsKey(c.pos))
                .ToList()
                .ForEach(c => frontier.Add(c.pos, c));

            checkedCells++;



            while (checkedCells < size.x * size.y)
            {
                tmp = SelectRandomCell();
                around = GetVisitedNeighboursAroundCell(tmp);
                if (around.ToList().All(c => c == null))
                    throw new NullReferenceException();

                tmp.BreakWallAndUpdateNeighbourhood(around);

                GetNeighboursOf(tmp).ToList()
                    .Where(c => !c.visited && !frontier.ContainsKey(c.pos))
                    .ToList()
                    .ForEach(c => frontier.Add(c.pos, c));

                checkedCells++;
            }
        }

        public AroundCell GetVisitedNeighboursAroundCell(Cell c)
        {
            AroundCell around = GetNeighboursOf(c);

            around.top = around.top != null && around.top.visited ? around.top : null;
            around.left = around.left != null &&  around.left.visited ? around.left : null;
            around.bottom= around.bottom != null && around.bottom.visited ? around.bottom : null;
            around.right = around.right != null &&  around.right.visited ? around.right : null;

            return around;
        }

        public AroundCell GetNeighboursOf(Cell c)
        {
            AroundCell around = new AroundCell();
           

            if(c.pos.x + 1 < size.x)
                around.right = cells[c.pos.x+1, c.pos.y];
            if (c.pos.x -1 >= 0)
                around.left = cells[c.pos.x - 1, c.pos.y];
            if (c.pos.y + 1 < size.y)
               around.bottom = cells[c.pos.x, c.pos.y+1];
            if (c.pos.y - 1 >= 0)
               around.top = cells[c.pos.x, c.pos.y - 1];

            return around;
        }

        public Cell SelectRandomCell()
        {
            int r = random.Next(frontier.Keys.Count);
            Cell c;
            if (frontier.TryGetValue(frontier.Keys.ElementAt(r), out c))
            {
                frontier.Remove(frontier.Keys.ElementAt(r));
            }

            return c;
        }

        public void SetInterestPoints()
        {
            chest = new Vector2Int(random.Next(size.x), random.Next(size.y));
            do
            {
                stairs = new Vector2Int(random.Next(size.x), random.Next(size.y));
            } while (chest == stairs);
            entry = new Vector2Int(random.Next(size.x), random.Next(size.y));

        }
    }
}