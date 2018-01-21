using System.Collections;
using System.Collections.Generic;
using Generation;
using UnityEngine;
using UnityEngine.AI;

public class LivingCreature : MonoBehaviour
{
    public const string PeasantName = "Peasant";
    public const string WerewolfName = "Werewolf";
    public const string ArmedPeasantName = "ArmedPeasant";
    public const string WolfLayer = "Werewolf";
    public static float MaxDistance = (World.Size * GenerateLand.MaxDistance);
    public static GenerateLand Land;

    public NavMeshAgent Agent;

    public int Behavior;
    public Chunk CurrentChunk;
    public GameObject Target;

    public Vector3 Destination;
    public bool Moving = false;

    public int MovingRadius = 30;

    private Animation animation;
    private Animator animator;

    private bool _movingToForge;

    // Use this for initialization
    void Start()
    {
        Agent = GetComponent<NavMeshAgent>();
        Agent.autoRepath = true;

        animation = GetComponent<Animation>();
        animator = GetComponent<Animator>();

        _movingToForge = false;

        switch (Behavior)
        {
            case 0:
                transform.name = PeasantName;
                Agent.speed = 8f;
                break;
            case 1:
                transform.name = WerewolfName;
                Agent.speed = 15f;
                gameObject.layer = LayerMask.NameToLayer(WolfLayer);
                break;
            case 2:
                transform.name = ArmedPeasantName;
                Agent.speed = 13f;
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Only display if we are on an active chunk
        if (!CurrentChunk.isActiveAndEnabled)
        {
            CurrentChunk.population.Remove(gameObject);
            Destroy(gameObject);
        }

        if (Agent.isOnNavMesh)
        {
            if (!Moving)
                MoveCreature();
            else if ((_movingToForge && Agent.remainingDistance < 0.5f) || Agent.remainingDistance < Mathf.Epsilon)
            {
                //As a peasant, I reached the forge
                if (_movingToForge)
                {
                    CurrentChunk.PeasantReachedForge(gameObject);
                }

                StopMoving();
            }
        }
    }

    void StopMoving()
    {
        Moving = false;
    }

    public void MoveCreature()
    {
        switch (Behavior)
        {
            case 0:
                MoveLikePeasant();
                break;
            case 2:
                MoveLikeArmedPeasant();
                break;
            case 1:
                MoveLikeWerewolf();
                break;
        }
    }

    public void MoveLikePeasant()
    {
        /*float x = Random.Range(CurrentChunk.posx * World.Size - 2 * World.Size,
                CurrentChunk.posx * World.Size + 2 * World.Size),
            z = Random.Range(CurrentChunk.posy * World.Size - 2 * World.Size,
                CurrentChunk.posy * World.Size + 2 * World.Size);*/

        float x = Random.Range(transform.position.x - MovingRadius, transform.position.x + MovingRadius),
            z = Random.Range(transform.position.z - MovingRadius, transform.position.z + MovingRadius);
        MoveToLocation(new Vector3(x,
            CurrentChunk.GetHeight((int) Mathf.Abs(x % World.Size), (int) Mathf.Abs(z % World.Size)), z));
    }

    public void MoveLikeWerewolf()
    {
        if (Target != null)
        {
            MoveToLocation(Target.transform.position);
        }
        else
        {
            if (Land.GetNearestPeasant((int) CurrentChunk.posx, (int) CurrentChunk.posy, out Target))
                MoveToLocation(Target.transform.position);
            else
                MoveLikePeasant();
        }
    }

    public void MoveLikeArmedPeasant()
    {
        if (Target != null)
        {
            MoveToLocation(Target.transform.position);
        }
        else
        {
            if (Land.GetNearestWerewolf((int) CurrentChunk.posx, (int) CurrentChunk.posy, out Target))
                MoveToLocation(Target.transform.position);
            else
                MoveLikePeasant();
        }
    }

    public void MoveToLocation(Vector3 target)
    {
        Destination = Agent.destination;
        Agent.destination = target;
        Agent.isStopped = false;
        Moving = true;
    }

    public bool IsPeasant()
    {
        return Behavior == 0;
    }

    public bool IsWerewolf()
    {
        return Behavior == 1;
    }

    public bool IsArmedPeasant()
    {
        return Behavior == 2;
    }

    public void MoveToForge(Vector3 position)
    {
        // If i'm a peasant
        if (IsPeasant())
        {
            Agent.isStopped = true;
            MoveToLocation(position);
            _movingToForge = true;
        }
    }
}