using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutOfBusTrigger : MonoBehaviour
{
    private bool entered;

    // Update is called once per frame
    void Update()
    {
        List<Collider2D> collidedWith = new List<Collider2D>();
        GetComponent<Collider2D>().OverlapCollider(new ContactFilter2D(), collidedWith);
        collidedWith = collidedWith.FindAll(c => c.gameObject.tag == "Player");
        if (collidedWith.Count == 1 && entered == false) {
            entered = true;
            Movable movable = collidedWith[0].GetComponent<Movable>();
            movable.EmitSound = false;
            movable.LimitLeftX = -9.35f;
            movable.LimitRightX = 35f;
            AudioUtils.StopWalkingSound();

        }
    }
}
