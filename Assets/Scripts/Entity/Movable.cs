using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Entities who have that property are expected to have walking animations.
public class Movable : MonoBehaviour
{

    [field: SerializeField] public float Speed { get; private set; } = 3f;
    [field: SerializeField] public float Margin { get; private set; } = 0.1f;
    [field: SerializeField] public bool EmitSound { get; private set; } = false;
    [field: SerializeField] public string MoveAnimBoolean { get; set; } = "is-moving";
    public Vector3 Destination { get; private set; }

    private Animator animator;
    private Action onArrive;
    private bool justArrived;
    private void Start() {
        animator = GetComponent<Animator>();
        StopMoving();
    }

    public bool IsMoving() {
        return (transform.position - Destination).magnitude > Margin;
    }

    public void MoveTo(Vector3 destination, Action onArriveCallback = null) {
        Destination = destination;
        onArrive = onArriveCallback;
        justArrived = false;
    }

    public void StopMoving() {
        Destination = transform.position;
    }

    private void Update() {
        bool isMoving = IsMoving();
        animator.SetBool(MoveAnimBoolean, isMoving);

        if (isMoving) {
            transform.position = Vector3.MoveTowards(transform.position, Destination, Speed * Time.deltaTime);
            bool isOnGrass = transform.position.x < -47;
            Vector2 direction = transform.position.x < Destination.x ? Vector2.right : Vector2.left;
            GetComponent<SpriteRenderer>().flipX = direction == Vector2.left;
            // todo: check collider against ground collider and get sound from there directly.
            if (EmitSound) {
                AudioUtils.PlayWalkingSound(isOnGrass ? AudioUtils.Surface.Grass : AudioUtils.Surface.Ground);
            }
        } else if (!justArrived) {
            justArrived = true;
            transform.position = SpriteUtils.PixelAlign(transform.position);
            if (onArrive != null) onArrive();
        } else {
            AudioUtils.StopWalkingSound();
        }
    }

}
