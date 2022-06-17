using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parrallax : MonoBehaviour
{

    [SerializeField] Transform player;
    [SerializeField] float zoom = 10f;

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(-player.position.x / zoom, transform.position.y, transform.position.z);
    }
}
