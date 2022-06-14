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
    [SerializeField] Transform inventoryManager;
    [SerializeField] Transform transitioner;
    [SerializeField] Transform cutsceneManager;
    Vector3 target = Vector3.zero;
    Animator animator = null;
    SpriteRenderer spriteRenderer = null;
    Vector2 direction = Vector2.right;
    GameObject interactiveTarget = null;
    GameObject hovered = null;
    InventoryItem currentItemDragged = null;

    // non generic... but not a lot of time left for the jam
    bool isPlantingDrugs = false;

    public enum State { Talking, Moving, Idle, Interacting }
    State state = State.Idle;

    private void Start() {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update() {
        if (cutsceneManager.GetComponent<CutScenePlayer>().IsPlayingCutScene())
            return;

        HighlightHoveredObjects();
        HandleClickInteractions();
        MovePlayer();

        animator.SetBool("is_moving", state == State.Moving);
        animator.SetBool("is_talking", state == State.Talking);
        

    }

    public void AddToInventory(InventoryItem item) {
        inventoryManager.GetComponent<UIInventoryManager>().AddToInventory(item);
    }
    public void RemoveFromInventory(string itemName) {
        inventoryManager.GetComponent<UIInventoryManager>().RemoveFromInventory(itemName);
    }

    public bool HasItemInInventory(InventoryItem item) {
        return inventoryManager.GetComponent<UIInventoryManager>().HasItemInInventory(item);
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
                        float thickness = targetObject.GetComponent<Interactive>().overrideThickness;
                        if (thickness == 0) { // TODO: fix this, right now I can't properly get the right outline on spritesheets
                            thickness = 1f / (18 * targetObject.GetComponent<SpriteRenderer>().bounds.size.x);
                        }
                        targetObject.GetComponent<SpriteRenderer>().material.SetFloat("Thickness", thickness);
                        hovered = targetObject;
                        hintText.GetComponent<UIInventoryUsageHint>().HoveredInteractive = hovered.GetComponent<Interactive>();
                    }
                } else if (hovered != null) {
                    hovered.GetComponent<SpriteRenderer>().material.SetFloat("Thickness", 0f);
                    hovered = null;
                }
            } else {
                CleanTipsAndOutline();
            }
        }
    }

    public void ShowCombinationResult(string thought) {
        state = State.Idle;
        GetComponent<Interactive>().floatingTextManager.AddText(gameObject, thought);
    }

    private void MovePlayer() {
        if (state == State.Moving && target != Vector3.zero) {
            transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
            if (Mathf.Abs(transform.position.x - target.x) < marginTarget) {
                float x = Mathf.Round(transform.position.x * 72) / 72;
                float y = Mathf.Round(transform.position.y * 72) / 72;
                float z = Mathf.Round(transform.position.z * 72) / 72;
                transform.position = new Vector3(x, y, z);
                state = State.Idle;
                if (isPlantingDrugs) {
                    GetComponent<Animator>().SetTrigger("interact_background");
                    isPlantingDrugs = false;
                    state = State.Interacting;
                    StartCoroutine(PlantDrugs());
                }
            }
        }
        if (state == State.Idle && interactiveTarget != null) {
            state = State.Interacting;
            Interactive interactive = interactiveTarget.GetComponent<Interactive>();
            // have the player point towards the target before interacting
            direction = transform.position.x < interactiveTarget.transform.position.x ? Vector2.right : Vector2.left;
            spriteRenderer.flipX = direction == Vector2.left;
            if (interactive.busy) {
                GetComponent<Interactive>().floatingTextManager.AddText(gameObject, "Seems busy at the moment...");
            } else {
                if (interactive.distanceToInteraction == 0) {
                    GetComponent<Animator>().SetTrigger(interactive.isPickup ? "pick_up" : "interact_background");
                } else {
                    state = State.Talking;
                }
                CleanTipsAndOutline();
                if (interactiveTarget.GetComponent<Interactive>().warpTo != null) {
                    GetComponent<Animator>().SetTrigger("interact_background");
                    transitioner.GetComponent<Animator>().SetTrigger("EnterDoor");
                    StartCoroutine(WarpTo(interactiveTarget.GetComponent<Interactive>().warpTo));
                } else {
                    interactiveTarget.GetComponent<Interactive>().StartDialog(interactiveTarget);
                    inventoryManager.GetComponent<UIInventoryManager>().active = false;
                }
            }
            interactiveTarget = null;
        }
    }

    IEnumerator PlantDrugs() {
        yield return new WaitForSeconds(1f);
        ScottAI scott = GameObject.Find("Scott").GetComponent<ScottAI>();
        inventoryManager.GetComponent<UIInventoryManager>().RemoveFromInventory("Fake Drugs");
        target.x = transform.position.x - 3f;
        direction = transform.position.x < target.x ? Vector2.right : Vector2.left;
        spriteRenderer.flipX = direction == Vector2.left;
        state = State.Moving;
        yield return new WaitForSeconds(2f);
        scott.PlantDrugs();
        SetIdle();
        GetComponent<Interactive>().floatingTextManager.AddText(gameObject, "I wouldn't want to be caught with this!");
    }

    IEnumerator WarpTo(Transform warp) {
        yield return new WaitForSeconds(.5f);
        SpawnInformation spawn = warp.GetComponent<SpawnInformation>();
        transform.position = spawn.playerPosition;
        Camera.main.GetComponent<CameraFollow>().leftBorder = spawn.limitCameraLeft;
        Camera.main.GetComponent<CameraFollow>().rightBorder = spawn.limitCameraRight;
        Camera.main.GetComponent<CameraFollow>().GoToFinalPosition();
        yield return new WaitForSeconds(.5f);
        transitioner.GetComponent<Animator>().SetTrigger("ExitDoor");
        SetIdle();
        yield return new WaitForSeconds(1f);
        transitioner.GetComponent<Animator>().Play("TransitionIdle");
    }

    private void CleanTipsAndOutline() {
        foreach (SpriteRenderer sr in FindObjectsOfType<SpriteRenderer>()) {
            sr.material.SetFloat("Thickness", 0f);   
        }

    }

    public State GetState() {
        return state;
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
                    CleanTipsAndOutline();
                    cutsceneManager.GetComponent<CutScenePlayer>().PlayCutscene(interaction.cutscene);
                }
            } else {
                GetComponent<Interactive>().floatingTextManager.AddText(gameObject, inventoryManager.GetComponent<UIInventoryManager>().GetNextNegativeUseFeedback());
            }
        }
        currentItemDragged = null;
    }

    private void TryPlantDrugs() {
        ScottAI scott = GameObject.Find("Scott").GetComponent<ScottAI>();
        if (scott.CanPlantDrugs()) {
            target.x = scott.transform.position.x + 0.2f;
            isPlantingDrugs = true;
            direction = transform.position.x < target.x ? Vector2.right : Vector2.left;
            spriteRenderer.flipX = direction == Vector2.left;
            state = State.Moving;
        } else {
            GetComponent<Interactive>().floatingTextManager.AddText(gameObject, "I need to distract him first");
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
        inventoryManager.GetComponent<UIInventoryManager>().active = true;
    }

    bool CanMove() {
        return state == State.Idle || state == State.Moving;
    }

}
