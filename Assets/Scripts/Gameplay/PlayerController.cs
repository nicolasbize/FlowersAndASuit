using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    [SerializeField] float speed = 4f;
    [SerializeField] float marginTarget = 0.1f;
    [SerializeField] Transform hintText;
    Vector3 target = Vector3.zero;
    Animator animator = null;
    SpriteRenderer spriteRenderer = null;
    Vector2 direction = Vector2.right;
    GameObject interactiveTarget = null;
    GameObject hovered = null;
    InventoryItem currentItemDragged = null;

    enum State { Talking, Moving, Idle, Interacting }
    State state = State.Idle;

    private void Start() {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update() {
        HighlightHoveredObjects();
        HandleClickInteractions();
        MovePlayer();

        animator.SetBool("is_moving", state == State.Moving);
        
    }

    private void HighlightHoveredObjects() {
        if (currentItemDragged != null) {
            hintText.GetComponent<UIInventoryUsageHint>().SetTarget(null);
        }
        if (CanMove()) {
            var hoveredPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(hoveredPosition, Vector2.zero);
            if (hit.collider != null) {
                var targetObject = hit.collider.gameObject;
                if (targetObject.GetComponent<Interactive>() != null) {
                    float thickness = targetObject.GetComponent<Interactive>().overrideThickness;
                    if (thickness == 0) { // TODO: fix this, right now I can't properly get the right outline on spritesheets
                        thickness = 1f / (18 * targetObject.GetComponent<SpriteRenderer>().bounds.size.x);
                    }
                    targetObject.GetComponent<SpriteRenderer>().material.SetFloat("Thickness", thickness);
                    hovered = targetObject;
                    if (currentItemDragged != null) {
                        hintText.GetComponent<UIInventoryUsageHint>().SetTarget(hovered.GetComponent<Interactive>().hintText);
                    }
                }
            } else if (hovered != null) {
                hovered.GetComponent<SpriteRenderer>().material.SetFloat("Thickness", 0f);
                hovered = null;
            }
        }
    }

    private void MovePlayer() {
        if (state == State.Moving && target != Vector3.zero) {
            transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
            if (Mathf.Abs(transform.position.x - target.x) < marginTarget) {
                state = State.Idle;
            }
        }
        if (state == State.Idle && interactiveTarget != null) {
            // have the player point towards the target before interacting
            direction = transform.position.x < interactiveTarget.transform.position.x ? Vector2.right : Vector2.left;
            spriteRenderer.flipX = direction == Vector2.left;
            state = State.Talking;
            interactiveTarget.GetComponent<SpriteRenderer>().material.SetFloat("Thickness", 0f);
            interactiveTarget.GetComponent<Interactive>().StartDialog(gameObject, interactiveTarget);
        }
    }

    public void SetDraggedInventoryItem(InventoryItem item) {
        currentItemDragged = item;
    }

    public void RemoveDraggedInventoryItem() {
        currentItemDragged = null;
    }

    private void HandleClickInteractions() {
        if (Input.GetMouseButtonDown(0) && CanMove()) {
            var mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);
            if (mousePosition.y > -2.8) { // don't allow clicking in toolbar for movement
                target = mousePosition;
                target.z = transform.position.z; // don't play with z axis
                target.y = transform.position.y; // stay on same horizontal strip
                if (hit.collider != null) {
                    var targetObject = hit.collider.gameObject;
                    if (targetObject.GetComponent<Interactive>() != null) {
                        Interactive interactive = targetObject.GetComponent<Interactive>();
                        interactiveTarget = targetObject;
                        float distToLeft = Mathf.Abs(targetObject.transform.position.x - interactive.distanceToInteraction - transform.position.x);
                        float distToRight = Mathf.Abs(targetObject.transform.position.x + interactive.distanceToInteraction - transform.position.x);
                        if (distToLeft < distToRight) {
                            target.x = targetObject.transform.position.x - interactive.distanceToInteraction;
                        } else {
                            target.x = targetObject.transform.position.x + interactive.distanceToInteraction;
                        }
                    } else {
                        interactiveTarget = null;
                    }
                } else {
                    interactiveTarget = null;
                }
                direction = transform.position.x < target.x ? Vector2.right : Vector2.left;
                spriteRenderer.flipX = direction == Vector2.left;
                state = State.Moving;
            }
        }
    }

    public void SetIdle() {
        interactiveTarget = null;
        state = State.Idle;
    }

    bool CanInteract() {
        return true;
    }
    bool CanMove() {
        return state == State.Idle || state == State.Moving;
    }

}
