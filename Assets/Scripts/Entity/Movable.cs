using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Entities who have that property are expected to have walking animations.
public class Movable : MonoBehaviour
{

    [field: SerializeField] public float Speed { get; private set; } = 3f;
    [field: SerializeField] public float Margin { get; private set; } = 0.1f;
    [field: SerializeField] public bool EmitSound { get; set; } = false;
    [field: SerializeField] public string MoveAnimBoolean { get; set; } = "is-moving";
    [field: SerializeField] public float LimitLeftX { get; set; } = -61.5f;
    [field: SerializeField] public float LimitRightX { get; set; } = 65f;
    public Vector3 Destination { get; private set; }

    private Animator animator;
    private Action onArrive;
    private bool justArrived;
    private AudioUtils.Surface currentSurface = AudioUtils.Surface.Grass;
    private void Start() {
        animator = GetComponent<Animator>();
        StopMoving();
    }

    public bool IsMoving() {
        return (transform.position - Destination).magnitude > Margin;
    }

    public void MoveTo(Vector3 destination, Action onArriveCallback = null) {
        Destination = new Vector3(Mathf.Clamp(destination.x, LimitLeftX, LimitRightX), destination.y, destination.z);
        onArrive = onArriveCallback;
        justArrived = false;
    }

    public void StopMoving() {
        Destination = transform.position;
    }

    public void FaceTowards(Vector3 position) {
        Vector2 direction = transform.position.x < position.x ? Vector2.right : Vector2.left;
        GetComponent<SpriteRenderer>().flipX = direction == Vector2.left;
    }

    private void Update() {
        bool isMoving = IsMoving();
        if (MoveAnimBoolean != "") {
            animator.SetBool(MoveAnimBoolean, isMoving);
        }

        if (isMoving) {
            transform.position = Vector3.MoveTowards(transform.position, Destination, Speed * Time.deltaTime);
            FaceTowards(Destination);
            CheckForGround();
            if (EmitSound) {
                AudioUtils.PlayWalkingSound(currentSurface);
            }
        } else if (!justArrived) {
            justArrived = true;
            transform.position = SpriteUtils.PixelAlign(transform.position);
            if (onArrive != null) onArrive();
        } else {
            // note: if you have 2 movable entities that emit sound, any entity not moving will cancel the
            // moving sound for the game as it is centralized.
            if (EmitSound) {
                AudioUtils.StopWalkingSound();
            }
        }
    }

    void CheckForGround() {
        if (GetComponent<Collider2D>() != null) {
            List<Collider2D> collidedWith = new List<Collider2D>();
            GetComponent<Collider2D>().OverlapCollider(new ContactFilter2D(), collidedWith);
            collidedWith = collidedWith.FindAll(c => c.gameObject.tag == "Ground");
            if (collidedWith.Count > 0) {
                currentSurface = collidedWith[0].GetComponent<Ground>().GroundSurface;
            }
        }
    }

}
