using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssetManager : MonoBehaviour
{
    public List<GameObject> Trees;
    public List<GameObject> Houses;
    public List<GameObject> Peasants;
    public List<GameObject> ArmedPeasants;
    public List<GameObject> Werewolves;

    public GameObject Forge;

    public Animation WalkAnimationPeasant;
    public Animation IdleAnimationPeasant;

    public Texture Grass;
    public Texture Road;

    public GameObject GetTree()
    {
        return Trees[Random.Range(0, Trees.Count)];
    }

    public GameObject GetHouse()
    {
        return Houses[Random.Range(0, Houses.Count)];
    }

    public GameObject GetPeasant()
    {
        return Peasants[Random.Range(0, Peasants.Count)];
    }

    public GameObject GetArmedPeasant()
    {
        return ArmedPeasants[Random.Range(0, ArmedPeasants.Count)];
    }

    public GameObject GetWerewolf()
    {
        return Werewolves[Random.Range(0, Werewolves.Count)];
    }
}
