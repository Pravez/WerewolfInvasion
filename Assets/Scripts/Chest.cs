using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chest : MonoBehaviour
{
    private int resourcesAmount;

    private void Start()
    {
        resourcesAmount = Random.Range(80, 210);

        transform.localScale = new Vector3(3, 3, 3);
        transform.Rotate(new Vector3(-90, 0, 0));

        GetComponent<ParticleSystem>().Play();
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.collider.gameObject.CompareTag(PlayerController.Name))
        {
            other.collider.gameObject.transform.GetComponentInParent<PlayerController>().GatherResources(resourcesAmount);
            Destroy(gameObject);
        }
    }
}
