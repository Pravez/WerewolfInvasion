using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Delaunay;
using Delaunay.Geo;
using Generation;
using Voronoi;
using Random = UnityEngine.Random;

public class VoronoiDemo : MonoBehaviour
{
    private int m_pointCount = 150;
    public Material land;
    private List<Vector2> m_points;
    private const int m_mapWidth = 200; //World.Size * World.WorldSize
    private const int m_mapHeight = 200; //World.Size * World.WorldSize
    private List<LineSegment> m_edges = null;
    private List<LineSegment> m_spanningTree;
    private List<LineSegment> m_delaunayTriangulation;
    public Texture2D tx;
    private Color[] pixels;
    float[,] map;

    private List<Vector2> realPoints;

    private Dictionary<Vector2, City> _cities;

    /* Generate a heightmap */
    float[,] createmap()
    {
        float[,] map = new float[m_mapWidth, m_mapHeight];
        for (int i = 0; i < m_mapWidth; i++)
        for (int j = 0; j < m_mapHeight; j++)
        {
            map[i, j] = Chunk.PerlinNoise(((float) i) / (m_mapWidth - 1) * World.Worldsize,
                ((float) j) / (m_mapHeight - 1) * World.Worldsize);
        }
        return map;
    }

    /* Create a random point */
    Vector2 findpoint(float[,] map)
    {
        int x = 0, y = 0;

        float maxHeight = map[Random.Range(0, m_mapWidth), Random.Range(0, m_mapHeight)];

        int tries = 0;
        while (tries < 100)
        {
            x = Random.Range(0, m_mapWidth);
            y = Random.Range(0, m_mapHeight);
            if (map[x, y] < maxHeight)
                break;

            tries++;
        }

        return new Vector2(x, y);
    }

    void Start()
    {
        map = createmap();
        _cities = new Dictionary<Vector2, City>();
        realPoints = new List<Vector2>();
        tx = new Texture2D((int) m_mapWidth, (int) m_mapHeight);
        pixels = new Color[(int) m_mapWidth * (int) m_mapHeight];
        for (int i = 0; i < m_mapWidth; i++)
        for (int j = 0; j < m_mapHeight; j++)
        {
            //pixels[i * m_mapHeight + j] =
              //  Color.Lerp(Color.green, Color.HSVToRGB(63 / 255f, 45 / 255f, 41 / 255f), map[i, j]);
            /* A remplacer pour le shader:*/
             pixels[i*m_mapHeight + j] = Color.green;
        }
        m_points = new List<Vector2>();
        List<uint> colors = new List<uint>();
        /* Randomly pick vertices */
        for (int i = 0; i < m_pointCount; i++)
        {
            colors.Add((uint) 0);
            Vector2 vec = findpoint(map);
            m_points.Add(vec);
            realPoints.Add(vec);

            new List<Vector2>
                {
                    new Vector2(vec.x - m_mapWidth, vec.y),
                    new Vector2(vec.x + m_mapWidth, vec.y),
                    new Vector2(vec.x - m_mapWidth, vec.y - m_mapHeight),
                    new Vector2(vec.x + m_mapWidth, vec.y - m_mapHeight),
                    new Vector2(vec.x, vec.y - m_mapHeight),
                    new Vector2(vec.x, vec.y + m_mapHeight),
                    new Vector2(vec.x + m_mapWidth, vec.y + m_mapHeight),
                    new Vector2(vec.x - m_mapWidth, vec.y + m_mapHeight)
                }
                .ForEach(p =>
                {
                    colors.Add(0);
                    m_points.Add(p);
                });
        }
        /* Generate Graphs */
        Delaunay.Voronoi v = new Delaunay.Voronoi(m_points, colors,
            new Rect(-m_mapWidth, -m_mapHeight, m_mapWidth * 2, m_mapHeight * 2));
        m_edges = v.VoronoiDiagram();
        m_spanningTree = v.SpanningTree(KruskalType.MINIMUM);
        m_delaunayTriangulation = v.DelaunayTriangulation();

        Dictionary<Vector2, List<LineSegment>> intersections = new Dictionary<Vector2, List<LineSegment>>();
        realPoints.ForEach(p =>
        {
            if (!intersections.ContainsKey(p))
                intersections.Add(p, new List<LineSegment>());
        });

        m_spanningTree.ForEach(e =>
            new List<Vector2?> {e.p0, e.p1}
                .Where(p => p != null)
                .ToList()
                .ForEach(p =>
                {
                    List<LineSegment> result;
                    if (intersections.TryGetValue((Vector2) p, out result))
                    {
                        if (!result.Contains(e))
                            result.Add(e);
                    }
                }));

        intersections.Keys.ToList().ForEach(p =>
        {
            List<LineSegment> result;
            if (intersections.TryGetValue(p, out result))
            {
                if (result.Count == 1)
                {
                    _cities.Add(p, new City(p));
                    World.AddVillage(ToWorldPosition(p), false);
                } 
                else if (result.Count > 3)
                {
                    _cities.Add(p, new City(p).SetSizeBetween(5, 10));
                    World.AddVillage(ToWorldPosition(p), true);
                }
            }
        });

        Color color = Color.blue;
        /* Shows Voronoi diagram */
        /*for (int i = 0; i < m_edges.Count; i++)
        {
            LineSegment seg = m_edges[i];
            Vector2 left = (Vector2) seg.p0;
            Vector2 right = (Vector2) seg.p1;
            DrawLine(left, right, color);
        }*/

        color = Color.red;
        /* Shows Delaunay triangulation */ /*
		if (m_delaunayTriangulation != null) {
			for (int i = 0; i < m_delaunayTriangulation.Count; i++) {
					LineSegment seg = m_delaunayTriangulation [i];				
					Vector2 left = (Vector2)seg.p0;
					Vector2 right = (Vector2)seg.p1;
					DrawLine (left, right,color);
			}
		}*/

        /* Shows spanning tree */

        color = Color.red;
        if (m_spanningTree != null)
        {
            for (int i = 0; i < m_spanningTree.Count; i++)
            {
                LineSegment seg = m_spanningTree[i];
                Vector2 left = (Vector2) seg.p0;
                Vector2 right = (Vector2) seg.p1;
                DrawLine(left, right, color);
            }
        }

        _cities.Add(new Vector2(0, 0), new City(new Vector2(10, 10)));
        _cities.Keys.ToList().ForEach(k => DrawPoint(_cities[k].position, Color.blue));

        land.SetTexture("_SplatTex", tx);
        tx.SetPixels(pixels);
        tx.Apply();
    }

    private void DrawPoint(Vector2 p, Color c)
    {
        if (p.x < m_mapWidth && p.x >= 0 && p.y < m_mapHeight && p.y >= 0)
            pixels[(int) p.x * m_mapHeight + (int) p.y] = c;
    }

    private bool isinside(Vector2 p, Vector2 q)
    {
        return((p.x < m_mapWidth && p.x >= 0 && p.y < m_mapHeight && p.y >= 0) ||
               (q.x < m_mapWidth && q.x >= 0 && q.y < m_mapHeight && q.y >= 0));
    }

    // Bresenham line algorithm
    private void DrawLine(Vector2 p0, Vector2 p1, Color c)
    {
        int x0 = (int) p0.x;
        int y0 = (int) p0.y;
        int x1 = (int) p1.x;
        int y1 = (int) p1.y;

        // For roads
        /*Vector2 u = new Vector2(x0 - x1, y0 -y1);
        Vector2 w = World.Worldsize * new Vector2(((float) x0) / m_mapWidth, ((float)y0) / m_mapHeight);
        Vector2 v = World.Worldsize * new Vector2(((float)x1 )/ m_mapWidth, ((float)y1) / m_mapHeight);
        GameObject roadgo = Instantiate(road, new Vector3(w.x * World.Size, 0, w.y * World.Size), Quaternion.identity);
        roadgo.SetActive(false);    
        roadgo.transform.localScale = new Vector3(u.magnitude / m_mapWidth * World.Size * World.Worldsize / 3, 1, 1);
        roadgo.transform.eulerAngles = new Vector3(0, Vector2.Angle(Vector2.right, u), 0);*/
        
        
        int dx = Mathf.Abs(x1 - x0);
        int dy = Mathf.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;
        if (!isinside(p0, p1))
            return;
        while (true)
        {
            if (x0 < m_mapWidth && x0 >= 0 && y0 < m_mapHeight && y0 >= 0)
                pixels[x0 * m_mapHeight + y0] = c;

            if (x0 == x1 && y0 == y1) break;
            int e2 = 2 * err;
            if (e2 > -dy)
            {
                err -= dy;
                x0 += sx;
            }
            if (e2 < dx)
            {
                err += dx;
                y0 += sy;
            }
        }
    }

    public bool isRoad(Vector2 position)
    {
        Vector2 p = ToMinimapChunkDimension(position);
        if (p.x < m_mapWidth && p.x >= 0 && p.y < m_mapHeight && p.y >= 0)
        {
            return pixels[(int) (p.x * m_mapHeight + p.y)] == Color.red;
        }
        return false;
    }

    public static Vector2 ToWorldPosition(Vector2 p)
    {
        return new Vector2(p.x * World.Worldsize / m_mapWidth, p.y * World.Worldsize / m_mapHeight);
    }

    public static Vector2 ToMinimapChunkDimension(Vector2 p)
    {
        return new Vector2(p.x * m_mapWidth / World.Worldsize, p.y * m_mapHeight / World.Worldsize);
    }

    public static Vector2 ToMinimapDimensions(Vector2 p)
    {
        return new Vector2(p.x * m_mapWidth / (World.Size * World.Worldsize),
            p.y * m_mapHeight / (World.Size * World.Worldsize));
    }

    public static Vector2 ToWorldDimensions(Vector2 p)
    {
        return new Vector2(p.x * (World.Size * World.Worldsize) / m_mapWidth,
            p.y * (World.Size * World.Worldsize) / m_mapHeight);
    }

    public static Vector2Int ToIntVec(Vector2 vec)
    {
        return new Vector2Int((int) vec.x, (int) vec.y);
    }
}