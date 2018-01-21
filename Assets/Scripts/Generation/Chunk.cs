using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Generation;
using UnityEngine.AI;
using Voronoi;
using Random = UnityEngine.Random;

public class Chunk : MonoBehaviour
{
    [Range(2, 100)] public static int SAMPLES = World.Size;
    public static int SIZE = World.Size;
    private float[,] map = null;
    public float posx, posy;

    //public static Vector2Int WorldSize = new Vector2Int(5, 5);

    public GameObject treePrefab;
    public List<GameObject> trees;

    public GameObject buildingPrefab;
    public List<GameObject> buildings;

    public static VoronoiDemo voronoi;
    public static SWAD simulation;

    public List<GameObject> population;
    public GameObject Peasant;
    public GameObject ArmedPeasant;
    public GameObject WereWolf;

    public Mesh terrain;
    Vector3[] vertices;
    Vector2[] uv;
    int[] triangles;

    public int Exponent = 2;

    private GameObject _forge;

    public static float PerlinNoise(float x, float y)
    {
        return Mathf.PerlinNoise(0.23f + x * 0.53f, y * 0.59f);
    }

    private float[,] GenerateHeightMap()
    {
        float x, y, nx, ny; //alphaX = posx/WorldSize.x, alphaY = posy/WorldSize.y;
        float[,] map = new float[SAMPLES, SAMPLES];
        for (int i = 0; i < SAMPLES; i++)
        for (int j = 0; j < SAMPLES; j++)
        {
            x = i / (float) (SAMPLES - 1) + 0.5f;
            y = j / (float) (SAMPLES - 1) + 0.5f;
            nx = Mathf.Abs(x + posx);
            ny = Mathf.Abs(y + posy);
            // We can use only positives, or absolute values, or square
            // The idea is to eliminate artefacts between generated chunks
            map[i, j] = 5f * Mathf.Pow(
                PerlinNoise(nx, ny)
                + 0.5f * PerlinNoise(2 * nx, 2 * ny)
                + 0.25f * PerlinNoise(4 * nx, 4 * ny), Exponent);
            // Pour faire en sorte d'avoir un aspect de répétition du monde, un modulo
            /*map [i, j] = 20*(
                             (Mathf.PerlinNoise(Mathf.Abs(x+posx%WorldSize.x),Mathf.Abs(y+posy%WorldSize.y)) * 1-alphaX +
                             Mathf.PerlinNoise(Mathf.Abs(x+(WorldSize.x - posx)),Mathf.Abs(y+posy%WorldSize.y)) * alphaX) * (1-alphaY) +
                             (Mathf.PerlinNoise(Mathf.Abs(x+posx%WorldSize.x),Mathf.Abs(y+posy%WorldSize.y)) * 1-alphaX +
                              Mathf.PerlinNoise(Mathf.Abs(x+posx%WorldSize.x),Mathf.Abs(y+(WorldSize.y - posy))) * alphaX) * alphaY);*/
        }

        return map;
    }

    // Create a terrain from the heightmap
    private Mesh GenerateTerrain(float[,] map)
    {
        float rsample = 1 / (float) (SAMPLES - 1);
        int ti = 0;
        int vi = 0;

        vertices = new Vector3[SAMPLES * SAMPLES];
        uv = new Vector2[vertices.Length];

        triangles = new int[(SAMPLES - 1) * (SAMPLES - 1) * 6];
        for (int i = 0, y = 0; y < SAMPLES; y++)
        {
            for (int x = 0; x < SAMPLES; x++, i++)
            {
                vertices[i] = new Vector3(x * SIZE * rsample, map[x, y], y * SIZE * rsample);
                //uv [i] = new Vector2 ((x * rsample*SIZE)/SIZE , (y * rsample * SIZE)/SIZE);
                uv[i] = new Vector2(((((float)x)/(SAMPLES-1)+posx)/World.Worldsize),((((float)y)/(SAMPLES-1)+posy)/World.Worldsize));
            }
        }

        for (int y = 0; y < SAMPLES - 1; y++)
        {
            for (int x = 0; x < SAMPLES - 1; x++)
            {
                triangles[ti] = vi;
                triangles[ti + 1] = vi + SAMPLES;
                triangles[ti + 2] = vi + SAMPLES + 1;
                triangles[ti + 3] = vi;
                triangles[ti + 4] = vi + SAMPLES + 1;
                triangles[ti + 5] = vi + 1;
                ti += 6;
                vi++;
            }

            vi++;
        }

        return terrain;
    }

    public List<GameObject> GenerateTrees()
    {
        List<GameObject> trees = new List<GameObject>();
        int R = 3;

        for (int yc = 0; yc < SAMPLES; yc++)
        {
            for (int xc = 0; xc < SAMPLES; xc++)
            {
                float max = 0;
                // there are more efficient algorithms than this
                for (int yn = yc - R; yn <= yc + R; yn++)
                {
                    for (int xn = xc - R; xn <= xc + R; xn++)
                    {
                        if (yn >= 0 && yn < SAMPLES && xn >= 0 && xn < SAMPLES)
                        {
                            float e = map[xn, yn];
                            if (e > max)
                            {
                                max = e;
                            }
                        }
                    }
                }

                if (map[xc, yc] == max)
                {
                    Vector3 pos = new Vector3(posx * SIZE + xc, map[xc, yc], posy * SIZE + yc);
                    if (!IsTreeAroundAnother(pos))
                    {
                        var tree = Instantiate(GameManager.Instance.AssetManager.GetTree(), pos, Quaternion.identity);
                        tree.transform.SetParent(gameObject.transform);
                        trees.Add(tree);
                    }
                }
            }
        }

        return trees;
    }

    public List<GameObject> PlaceBuildings(List<City> villages, double coefx, double coefy)
    {
        List<GameObject> _buildings = new List<GameObject>();
        villages.ForEach(v =>
        {
            int signx = Mathf.FloorToInt((float) coefx) < 0
                ? Mathf.FloorToInt((float) coefx) + 1
                : Mathf.FloorToInt((float) coefx);
            int signy = Mathf.FloorToInt((float) coefy) < 0
                ? Mathf.FloorToInt((float) coefy) + 1
                : Mathf.FloorToInt((float) coefy);

            double x = (v.position.x * World.Size) * Math.Sign(coefx) + signx * World.Size * World.Worldsize;
            double y = map[(int) ((v.position.x - Math.Truncate(v.position.x)) * World.Size),
                (int) ((v.position.y - Math.Truncate(v.position.y)) * World.Size)];
            double z = (v.position.y * World.Size) * Math.Sign(coefy) + signy * World.Size * World.Worldsize;
            if (v.size < 5)
            {
                var b = Instantiate(GameManager.Instance.AssetManager.GetHouse(),
                    new Vector3((float) x, (float) y, (float) z)
                    , Quaternion.identity);
                b.transform.SetParent(gameObject.transform);
                b.transform.localScale = new Vector3(3, 3, 3);
                _buildings.Add(b);
            }
            else
            {
                GameManager.Instance.GenerateLabyrinth(new Vector3((float) x, (float) y, (float) z), new Vector3Int(Random.Range(5, 10), Random.Range(5, 10), Random.Range(1, 3)));
            }
        });


        return _buildings;
    }

    public bool IsTreeAroundAnother(Vector3 position)
    {
        float min_distance = 100f;
        bool min = false;
        min = trees.Select(t => t.transform.position)
            .Any(p => Mathf.Abs(p.magnitude + position.magnitude) <= min_distance);

        return false;
    }

    public void GeneratePopulation()
    {
        int peasants = simulation.GetPeasants();
        int armed = simulation.GetArmedPeasants();
        int werewolves = simulation.GetWerewolves();

        InstantiateRandomly(GameManager.Instance.AssetManager.GetPeasant(), peasants / World.WorldTotalSize);
        InstantiateRandomly(GameManager.Instance.AssetManager.GetArmedPeasant(), armed / World.WorldTotalSize);
        InstantiateRandomly(WereWolf, werewolves / World.WorldTotalSize);
    }

    private void InstantiateRandomly(GameObject obj, int quantity)
    {
        Vector3 position;
        for (int i = 0; i < quantity; ++i)
        {
            position = new Vector3(Random.Range(0, SIZE), Random.Range(0, SIZE));
            var result = Instantiate(obj,
                new Vector3(posx * SIZE + position.x,
                    map[(int) position.x, (int) position.y] + obj.transform.localScale.y,
                    posy * SIZE + position.y),
                Quaternion.identity);
            result.GetComponent<LivingCreature>().CurrentChunk = this;
            population.Add(result);
        }
    }

    public void Start()
    {
        posx = transform.position.x / SIZE;
        posy = transform.position.z / SIZE;
        terrain = new Mesh();

        StartCoroutine(LaunchInit(Init, Done));
    }

    IEnumerator LaunchInit(Action toRun, Action callback) {
        bool done = false;
        new Thread(()=>{
            toRun();
            done = true;
        }).Start();
        while (!done)
            yield return null;
        callback();
    }

    void Done()
    {
        terrain.vertices = vertices;
        terrain.triangles = triangles;
        terrain.uv = uv;
        terrain.RecalculateNormals();
        GetComponent<MeshCollider>().sharedMesh = terrain;
        GetComponent<MeshFilter>().mesh = terrain;

        List<City> temp;
        // Because voronoi is causing problems : chunks are symetrical but voronoi is toriq
        int posvillagex = (int) (posx < 0 ? World.Worldsize + posx % World.Worldsize : posx % World.Worldsize); 
        int posvillagey = (int) (posy < 0 ? World.Worldsize + posy % World.Worldsize : posy % World.Worldsize); 
        if (World.TryGetVillage(new Vector2Int(posvillagex, posvillagey), out temp))
        {
            buildings = PlaceBuildings(temp, (posx/World.Worldsize), (posy/World.Worldsize));
        }

        if (voronoi.isRoad(new Vector2(posx, posy)))
        {
            //case where it's a route
            var comp = (NavMeshModifierVolume) gameObject.AddComponent(typeof(NavMeshModifierVolume));
            comp.area = 4; // the fourth we added
            Debug.Log("Route on : " + posx + " " + posy);
        }
        else
        {
            trees = GenerateTrees();
        }

        GeneratePopulation();
    }

    void Init()
    {
        map = GenerateHeightMap();
        terrain = GenerateTerrain(map);
        population = new List<GameObject>();
    }

    private void OnDestroy()
    {
        Destroy(gameObject);
    }

    public float GetHeight(int x, int z)
    {
        x = Mathf.Clamp(x, 0, SIZE);
        z = Mathf.Clamp(z, 0, SIZE);

        return map[x, z];
    }

    public bool PlaceForge(GameObject forge)
    {
        if (_forge == null)
        {
            forge.transform.SetParent(transform);
            forge.transform.position = new Vector3(forge.transform.position.x,
                GetHeight((int) (forge.transform.position.x % World.Size),
                    (int) (forge.transform.position.z % World.Size)),
                forge.transform.position.z);

            population.ForEach(p => p.GetComponent<LivingCreature>().MoveToForge(new Vector3(forge.transform.position.x,
                forge.transform.position.y, forge.transform.position.z)));
            _forge = forge;
            return true;
        }

        return false;
    }

    public void PeasantReachedForge(GameObject peasant)
    {
        population.Remove(peasant);
        var armed = Instantiate(GameManager.Instance.AssetManager.GetArmedPeasant(),
            Vector3.zero,
            Quaternion.identity);
        armed.transform.SetParent(transform);
        armed.transform.position = _forge.transform.Find("ArmedSpawn").position;
        armed.GetComponent<LivingCreature>().CurrentChunk = this;
        population.Add(armed);
        Destroy(peasant);

        simulation.AddForgeToChangePeasantsToArmed();
    }

    public void DestroyAround(Vector3 position)
    {
        List<GameObject> destroyed = new List<GameObject>();
        population.Where(p => p.GetComponent<LivingCreature>().IsWerewolf()).ToList()
            .ForEach(p =>
            {
                Debug.Log("Killed werewolf !");
                Destroy(p);
                destroyed.Add(p);
                simulation.KillWerewolf();
            });
        
        destroyed.ForEach(d => population.Remove(d));
    }
}