using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static InventoryItem;

public class PlayerController : MonoBehaviour
{

    [SerializeField] float speed = 4f;
    [SerializeField] float marginTarget = 0.1f;
    [SerializeField] Transform hintText;
    [SerializeField] float toolbarY = -2.8f;
    [SerializeField] UIInventoryManager inventoryManager;
    [SerializeField] Game game;

    ConversationManager conversationManager;
    Vector3 target = Vector3.zero;
    Animator animator = null;
    SpriteRenderer spriteRenderer = null;
    GameObject interactiveTarget = null;
    GameObject hovered = null;
    InventoryItem currentItemDragged = null;
    Vector2 screenBounds = new Vector2(-61.5f, 65f);
    public bool isInLiving = false;

    public enum PlayerState { Idle, Talking, Moving, PickingUpItem, InteractingBackground }
    public PlayerState State { get; private set; }

    private void Start() {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        conversationManager = game.GetComponent<ConversationManager>();
    }

    void Update() {
        isInLiving = transform.position.y > -100;
        if (animator.GetBool("is_moving") && isInLiving) {
            bool isOnGrass = transform.position.x < -47;
            AudioUtils.PlayWalkingSound(isOnGrass ? AudioUtils.Surface.Grass : AudioUtils.Surface.Ground);
        } else {
            AudioUtils.StopWalkingSound();
        }

        if (game.IsBusy())
            return;

        HighlightHoveredObjects();
        HandleClickInteractions();
        MovePlayer();

        if (isInLiving) {
            animator.SetBool("is_moving", State == PlayerState.Moving);
        } else {
            animator.SetBool("is_walking_flowers", State == PlayerState.Moving);
        }
        animator.SetBool("is_talking", State == PlayerState.Talking);

    }

    private void HighlightHoveredObjects() {
        hintText.GetComponent<UIInventoryUsageHint>().HoveredInteractive = null;
        if (CanMove()) {
            var hoveredPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(hoveredPosition, Vector2.zero);
            if (hoveredPosition.y > Camera.main.transform.position.y + toolbarY) { // don't allow clicking in toolbar for movement
                if (hit.collider != null) {
                    var targetObject = hit.collider.gameObject;
                    if (targetObject.GetComponent<Interactive>() != null) {
                        SpriteUtils.RemoveOutlines();
                        SpriteUtils.AddOutline(targetObject);
                        hovered = targetObject;
                        hintText.GetComponent<UIInventoryUsageHint>().HoveredInteractive = hovered.GetComponent<Interactive>();
                    }
                } else if (hovered != null) {
                    SpriteUtils.RemoveOutline(hovered);
                    hovered = null;
                }
            } else {
                SpriteUtils.RemoveOutlines();
            }
        }
    }

    private void MovePlayer() {
        if (State == PlayerState.Moving && target != Vector3.zero) {
            transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
            if (Mathf.Abs(transform.position.x - target.x) < marginTarget) {
                transform.position = SpriteUtils.PixelAlign(transform.position);
                State = PlayerState.Idle;
                if (interactiveTarget != null) {
                    InteractWith(interactiveTarget);
                    interactiveTarget = null;
                }
            }
        }
    }

    private void InteractWith(GameObject target) {
        Interactive interactive = target.GetComponent<Interactive>();
        Warpable warpable = target.GetComponent<Warpable>();
        Observable observable = target.GetComponent<Observable>();
        Talkable talkable = target.GetComponent<Talkable>();
        Pickable pickable = target.GetComponent<Pickable>();
        LookTowards(target.transform.position);
        SpriteUtils.RemoveOutlines();

        if (warpable != null) {
            State = PlayerState.InteractingBackground;
            warpable.Warp(this, SetIdle);
        } else if (pickable != null) {
            State = PlayerState.Talking;
            conversationManager.ThinkOutLoud(pickable.TextBeforePickup, () => {
                pickable.PickUp(this, SetIdle);
            });
        } else if (talkable != null) {
            State = PlayerState.Talking;
            conversationManager.TalkTo(talkable, SetIdle);
        } else if (observable != null) {
            State = PlayerState.Talking;
            conversationManager.ThinkOutLoud(observable.Observation, SetIdle);
        }
    }

    private void LookTowards(Vector3 position) {
        Vector2 direction = transform.position.x < position.x ? Vector2.right : Vector2.left;
        spriteRenderer.flipX = direction == Vector2.left;
    }

    public void SetDraggedInventoryItem(InventoryItem item) {
        currentItemDragged = item;
    }

    public void RemoveDraggedInventoryItem() {
        // we just stopped dragging the object, check if we're trying to do an action first
        if (hovered != null) {
            Interaction interaction = new List<Interaction>(currentItemDragged.interactions).Find(i => i.objectName == hovered.name);
            if (interaction != null) {
                if (interaction.callbackName.Length > 0) {
                    Invoke(interaction.callbackName, 0);
                } else if (interaction.cutscene != null) {
                    SpriteUtils.RemoveOutlines();
                    game.GetComponent<CutScenePlayer>().PlayCutscene(interaction.cutscene);
                }
            } else {
                conversationManager.ThinkOutLoud(inventoryManager.GetNextNegativeUseFeedback(), SetIdle);
            }
        }
        currentItemDragged = null;
    }

    private void TryPlantDrugs() {
        ScottAI scott = GameObject.Find("Scott").GetComponent<ScottAI>();
        if (scott.IsOnPhone()) {
            scott.PlantDrugs();
        } else {
            State = PlayerState.Talking;
            conversationManager.ThinkOutLoud(new SpokenLine("I need to distract him first", 7), SetIdle);
        }
    }

    private void TryCutString() {
        JimAI jim = GameObject.Find("Jim").GetComponent<JimAI>();
        if (jim.CanCutKite()) {
            jim.CutKite();
        } else {
            State = PlayerState.Talking;
            conversationManager.ThinkOutLoud(new SpokenLine("I can't do that.   I need to get rid of the police officer first", 8), SetIdle);
        }
    }

    private void HandleClickInteractions() {
        if (Input.GetMouseButtonDown(0) && CanMove()) {
            var mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);
            if (mousePosition.y > Camera.main.transform.position.y + toolbarY) { // don't allow clicking in toolbar for movement
                target = mousePosition;
                target.z = transform.position.z; // don't play with z axis
                target.y = transform.position.y; // stay on same horizontal strip
                if (hit.collider != null) {
                    var targetObject = hit.collider.gameObject;
                    if (targetObject.GetComponent<Interactive>() != null && targetObject.tag != "Player") {
                        Interactive interactive = targetObject.GetComponent<Interactive>();
                        interactiveTarget = targetObject;
                        float distToLeft = Mathf.Abs(targetObject.transform.position.x - interactive.DistanceToInteraction - transform.position.x);
                        float distToRight = Mathf.Abs(targetObject.transform.position.x + interactive.DistanceToInteraction - transform.position.x);
                        if (distToLeft < distToRight) {
                            target.x = targetObject.transform.position.x - interactive.DistanceToInteraction;
                        } else {
                            target.x = targetObject.transform.position.x + interactive.DistanceToInteraction;
                        }
                    } else {
                        interactiveTarget = null;
                    }
                } else {
                    // we're just moving, let's box it up
                    target.x = Mathf.Clamp(target.x, screenBounds.x, screenBounds.y);
                    interactiveTarget = null;
                }
                LookTowards(target);
                State = PlayerState.Moving;
            }
        }
    }

    public void SetIdle() {
        interactiveTarget = null;
        State = PlayerState.Idle;
    }

    bool CanMove() {
        return State == PlayerState.Idle || State == PlayerState.Moving;
    }

}
