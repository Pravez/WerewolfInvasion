using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Generation;
using UnityEngine;

public class GenerateLand : MonoBehaviour
{
	public const int PlayerNeighbourhood = 6;
    public const float PlayerVisibility = 5.0f;
	public const float MaxDistance = 10.0f;
	
	public Chunk Land;
	public VoronoiDemo Voronoi;
	public SWAD Simulation;
    public Ui GameUi;

    public Vector2Int previousPosition;
    public Vector2Int currentPosition;

	private Dictionary<Vector2Int, Chunk> _chunks;
	private List<Vector2Int> _chunksToKeep;
	private List<Vector2Int> _toBeRemoved;


	void Start ()
	{
		_chunks = new Dictionary<Vector2Int, Chunk>();
		_chunksToKeep = new List<Vector2Int>();
		_toBeRemoved = new List<Vector2Int>();
		Chunk.voronoi = Voronoi;
		Chunk.simulation = Simulation;
		LivingCreature.Land = this;
	    SWAD.GameUi = GameUi;
	    PlayerController.land = this;
		PlayerController._simulation = Simulation;
	    previousPosition = new Vector2Int(0, 0);
	}
	
	void Update ()
	{
	    currentPosition = new Vector2Int(
	        (int) (Camera.main.transform.position.x / Chunk.SIZE),
	        (int) (Camera.main.transform.position.z / Chunk.SIZE));
        if(previousPosition != currentPosition)
		    CheckAndUpdateChunksAroundPosition(currentPosition);
	}

	void CheckAndUpdateChunksAroundPosition(Vector2Int position)
	{
	    previousPosition = currentPosition;
		_chunksToKeep.Clear();
		_toBeRemoved.Clear();
		Vector2Int temp = new Vector2Int();
		Chunk tempChunk;
		
		//En premier lieu on parcourt autour du joueur (de la camera)
		// avec une taille de voisins de SquareSize
		for (int i = position.x - PlayerNeighbourhood; i < position.x + PlayerNeighbourhood; ++i)
		{
			for (int j = position.y - PlayerNeighbourhood; j < position.y + PlayerNeighbourhood; ++j)
			{
				temp.x = i;
				temp.y = j;

				if (!_chunks.ContainsKey(temp))
				{
					//Si le chunk n'est pas dans le dictionnaire on l'ajoute
					_chunks.Add(temp, 
						Instantiate(Land, new Vector3(i * Chunk.SIZE, 0, j * Chunk.SIZE), Quaternion.identity));
				}else if (_chunks.TryGetValue(temp, out tempChunk))
				{
					// S'il était dedans, et qu'il était désactivé, on le réactive
					if (!tempChunk.isActiveAndEnabled)
					{
						tempChunk.gameObject.SetActive(true);
					}
				}
				
			    //On signale que c'est un chunk qu'on veut actif (parce qu'autour de la caméra)
			        _chunksToKeep.Add(temp);
			}
		}

		
		foreach (var vector2Int in _chunks.Keys)
		{
			// Pour tous les chunks, on désactive ceux qui ne sont pas autour de la caméra
			//(ceux qui n'ont pas été ajoutés lors du parcours du voisinnage)
			if (!_chunksToKeep.Contains(vector2Int))
			{
				if (_chunks.TryGetValue(vector2Int, out tempChunk))
				{
					if (IsTooFarFrom(vector2Int, position))
					{
						_toBeRemoved.Add(vector2Int);
						// Besoin de détruire le chunk ET son gameObject
						// du coup surcharge de la méthode OnDestroy() dans le chunk
						// qui supprime à son tour le gameObject
						Destroy(tempChunk);
					    _chunksToKeep.Remove(vector2Int);
					}
					else
					{
						tempChunk.gameObject.SetActive(false);
					}
				}
			}
		}

		_toBeRemoved.ForEach(v => _chunks.Remove(v));
	}

	public static bool IsTooFarFrom(Vector2Int position, Vector2Int from)
	{
		return new Vector2Int(Math.Abs(position.x - from.x), Math.Abs(position.y - from.y)).magnitude > MaxDistance;
	}

	public bool GetNearestPeasant(int posx, int posy, out GameObject peasant)
	{
		Chunk chunk;
		peasant = null;
		if(_chunks.TryGetValue(new Vector2Int(posx, posy), out chunk))
		{
			peasant = chunk.population
				.FirstOrDefault(p => p.GetComponent<LivingCreature>().IsPeasant());
		    if (peasant == null)
		        return false;
			return true;
		}

		return false;
	}
	
	public bool GetNearestWerewolf(int posx, int posy, out GameObject werewolf)
	{
		Chunk chunk;
		werewolf = null;
		if(_chunks.TryGetValue(new Vector2Int(posx, posy), out chunk))
		{
			werewolf = chunk.population
				.FirstOrDefault(p => p.GetComponent<LivingCreature>().IsWerewolf());
		    if (werewolf == null)
		        return false;
			return true;
		}
		return false;
	}

    public bool AddForgeOnPosition(float x, float y, GameObject forge)
    {
        Chunk chunk;
        int posx = (int) (x / World.Size), posy = (int) (y / World.Size);
        if (_chunks.TryGetValue(new Vector2Int(posx, posy), out chunk))
        {
            return chunk.PlaceForge(forge);
        }

        return false;
    }

    public float GetHeightOnWorldPosition(float x, float y)
    {
        Chunk chunk;
        int posx = (int) (x / World.Size), posy = (int) (y / World.Size);
        if (_chunks.TryGetValue(new Vector2Int(posx, posy), out chunk))
        {
            return chunk.GetHeight((int) (x % World.Size), (int) (y % World.Size));
        }

        return 0;
    }

	public void KillAroundPosition(Vector3 position)
	{
		int x = (int) (position.x / World.Size), y = (int) (position.z / World.Size);
		Chunk temp;
		if (_chunks.TryGetValue(new Vector2Int(x, y), out temp))
		{
			temp.DestroyAround(position);
		}
	}
}
