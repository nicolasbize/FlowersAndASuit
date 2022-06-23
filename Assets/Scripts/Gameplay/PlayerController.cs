using System.Collections.Generic;
using UnityEngine;
using static InventoryItem;

public class PlayerController : MonoBehaviour
{

    [SerializeField] SpokenLine[] invalidActionFeedbacks;
    [SerializeField] float speed = 4f;
    [SerializeField] float marginTarget = 0.1f;
    [SerializeField] Transform hintText;
    [SerializeField] float toolbarY = -2.8f;

    //ConversationManager conversationManager;
    Vector3 target = Vector3.zero;
    GameObject interactiveTarget = null;
    GameObject hovered = null;
    InventoryItem currentItemDragged = null;
    Vector2 screenBounds = new Vector2(-61.5f, 65f);
    public bool isInLiving = false;
    int invalidActionFeedbackIndex = 0;

    public enum PlayerState { Idle, InteractingBackground }
    public PlayerState State { get; set; }

    void Update() {
        HighlightHoveredObjects();
        HandleClickInteractions();
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

    private void InteractWith(GameObject target) {
        Interactive interactive = target.GetComponent<Interactive>();
        Warpable warpable = target.GetComponent<Warpable>();
        Observable observable = target.GetComponent<Observable>();
        Speakable speakable = target.GetComponent<Speakable>();
        Pickable pickable = target.GetComponent<Pickable>();
        GetComponent<Movable>().FaceTowards(target.transform.position);
        SpriteUtils.RemoveOutlines();

        if (warpable != null) {
            State = PlayerState.InteractingBackground;
            warpable.Warp(this, SetIdle);
        } else if (pickable != null) {
            State = PlayerState.InteractingBackground;
            GetComponent<Speakable>().Speak(pickable.TextBeforePickup, () => {
                pickable.PickUp(this, SetIdle);
            });
        } else if (speakable != null) {
            if (speakable.Busy) {
                GetComponent<Speakable>().Speak(new SpokenLine("Seems busy at the moment.", 6));
            } else {
                DialogManager.Instance.StartConversation(speakable.Dialog, GetComponent<Speakable>(), speakable);
            }
        } else if (observable != null) {
            GetComponent<Speakable>().Speak(observable.Observation);
        }
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
                    CutSceneManager.Instance.PlayCutscene(interaction.cutscene);
                }
            } else {
                ProvideNegativeFeedback();
            }
        }
        currentItemDragged = null;
    }

    private void TryPlantDrugs() {
        ScottAI scott = GameObject.Find("Scott").GetComponent<ScottAI>();
        if (scott.IsOnPhone()) {
            scott.PlantDrugs();
        } else {
            GetComponent<Speakable>().Speak(new SpokenLine("I need to distract him first", 7));
        }
    }

    private void TryCutString() {
        JimAI jim = GameObject.Find("Jim").GetComponent<JimAI>();
        if (jim.CanCutKite()) {
            jim.CutKite();
        } else {
            GetComponent<Speakable>().Speak(new SpokenLine("Can't do that, I need to get rid of the police officer first", 8));
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
                GetComponent<Movable>().MoveTo(target, () => {
                    if (interactiveTarget != null) {
                        InteractWith(interactiveTarget);
                        interactiveTarget = null;
                    }
                });
            }
        }
    }

    public void SetIdle() {
        interactiveTarget = null;
        State = PlayerState.Idle;
    }

    public bool CanMove() {
        if (DialogManager.Instance.InConversation()) return false;
        if (GetComponent<Speakable>().IsSpeaking()) return false;
        if (State == PlayerState.InteractingBackground) return false;
        if (CutSceneManager.Instance.IsPlayingCutScene()) return false;
        return true;
    }

    public void ProvideNegativeFeedback() {
        invalidActionFeedbackIndex = (invalidActionFeedbackIndex + 1) % invalidActionFeedbacks.Length;
        SpokenLine feedback = invalidActionFeedbacks[invalidActionFeedbackIndex];
        GetComponent<Speakable>().Speak(feedback);

    }

}
