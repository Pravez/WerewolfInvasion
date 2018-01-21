using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NLabyrinth;
using UnityEngine;
using Cell = NLabyrinth.Cell;
using Random = System.Random;

public class GameManager : MonoBehaviour
{
    #region static instance creation

    public static GameManager Instance;

    void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("More than one GameManager in scene");
        }
        else
        {
            Instance = this;
            Setup();
        }
    }

    void Setup()
    {
        AssetManager = GetComponent<AssetManager>();
        labyrinths = new List<List<LabyrinthGenerator>>();
    }

    #endregion

    public AssetManager AssetManager;


    public List<List<LabyrinthGenerator>> labyrinths;

    public GameObject Floor;
    public GameObject WallEW;
    public GameObject WallNS;
    public GameObject Chest;

    public float TileSize;

    public PlayerController PlayerInstance;
    public static GameObject Player;


    public void GenerateLabyrinth(Vector3 startPosition, Vector3Int size)
    {
        List<LabyrinthGenerator> generator = new List<LabyrinthGenerator>(size.z);
        for (int i = 0; i < size.z; ++i)
            generator.Add(new LabyrinthGenerator(new Vector2Int(size.x, size.y)));
        generator.ForEach(l => l.SetInterestPoints());

        GenerateFloor(generator, new Vector2Int(size.x, size.y), startPosition);
        GenerateWalls(generator, new Vector2Int(size.x, size.y), startPosition);
    }

    void GenerateFloor(List<LabyrinthGenerator> generator, Vector2Int size, Vector3 startPosition)
    {
        for (int h = 0; h < generator.Count + 1; h++)
        {
            for (int i = 0; i < size.x; ++i)
            {
                for (int j = 0; j < size.y; ++j)
                {
                    if (h == generator.Count || generator[h].stairs != new Vector2Int(i, j))
                        Instantiate(Floor,
                            new Vector3(startPosition.x + i * TileSize + TileSize / 2f, startPosition.y + h * 8,
                                startPosition.z + j * TileSize + TileSize / 2),
                            Quaternion.identity);
                }
            }

            if (h < generator.Count)
                Instantiate(Chest,
                    new Vector3(startPosition.x + generator[h].chest.x * TileSize + TileSize / 2f,
                        startPosition.y + h * 8 + 0.5f,
                        startPosition.z + generator[h].chest.y * TileSize + TileSize / 2f),
                    Quaternion.identity);
        }
    }

    void GenerateWalls(List<LabyrinthGenerator> generator, Vector2Int size, Vector3 startPosition)
    {
        for (int h = 0; h < generator.Count; ++h)
        {
            for (int i = 0; i < size.x; ++i)
            {
                if (i != generator[h].entry.x || h > 0)
                {
                    Instantiate(WallNS,
                        new Vector3(startPosition.x + i * TileSize + TileSize / 2f, startPosition.y + 4f + h * 8,
                            startPosition.z + 0), Quaternion.identity);
                    Instantiate(WallNS,
                        new Vector3(startPosition.x + i * TileSize + TileSize / 2f, startPosition.y + 4f + h * 8,
                            startPosition.z + size.y * TileSize),
                        Quaternion.identity);
                }
            }

            for (int j = 0; j < size.y; ++j)
            {
                if (j != generator[h].entry.y || h > 0)
                {
                    Instantiate(WallEW,
                        new Vector3(startPosition.x + 0, startPosition.y + 4f + h * 8,
                            startPosition.z + j * TileSize + TileSize / 2f), Quaternion.identity);
                    Instantiate(WallEW,
                        new Vector3(startPosition.x + size.x * TileSize, startPosition.y + 4f + h * 8,
                            startPosition.z + j * TileSize + TileSize / 2),
                        Quaternion.identity);
                }
            }

            Cell tmp;
            for (int i = 0; i < size.x; ++i)
            {
                for (int j = 0; j < size.y; ++j)
                {
                    tmp = generator[h].cells[i, j];
                    if (tmp.bottom != null && i != size.x - 1)
                        Instantiate(WallNS,
                            new Vector3(startPosition.x + i * TileSize + TileSize / 2f, startPosition.y + 4f + h * 8,
                                startPosition.z + j * TileSize + TileSize),
                            Quaternion.identity);
                    if (tmp.left != null && j != size.y - 1)
                    {
                        Instantiate(WallEW,
                            new Vector3(startPosition.x + i * TileSize + TileSize, startPosition.y + 4f + h * 8,
                                startPosition.z + j * TileSize + TileSize / 2f),
                            Quaternion.identity);
                    }
                }
            }
        }
    }
}