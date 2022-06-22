using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static InventoryItem;
using static PlayerController;

public class InventoryManager : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{

    public static InventoryManager Instance { get; private set; }
    void Awake() {
        if (Instance != null) {
            GameObject.Destroy(Instance);
        } else {
            Instance = this;
        }
        DontDestroyOnLoad(this);
    }


    [SerializeField] InventoryItem[] inventoryItems;
    [SerializeField] Transform prefabInventoryItem;
    [SerializeField] Transform UI;
    [SerializeField] Transform hintTextObject;
    [SerializeField] PlayerController player;

    private GameObject hoveredItem = null;
    private GameObject draggedObject = null;
    private GameObject currentInventoryTarget = null;

    private void Start() {
        RefreshInventory();
    }

    public bool IsActive { get {
            return player.CanMove() && player.GetComponent<Movable>().IsMoving() == false;
    } }

    private void Update() {
        if (IsActive) {
            foreach (Transform child in transform) {
                child.gameObject.GetComponent<RawImage>().material.SetFloat("_Thickness", 0f);
                child.gameObject.GetComponent<RawImage>().material.SetColor("_OutlineColor", new Color(0, 0, 0, 1));
            }

            PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
            eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
            results = results.FindAll(r => r.gameObject.tag == "InventoryItem");
            if (draggedObject != null) {
                RaycastResult hoveredObject = results.Find(r => r.gameObject.GetComponent<RawImage>().enabled && r.gameObject.name != draggedObject.gameObject.name);
                hoveredItem = hoveredObject.gameObject;
                // moved dragged object with mouse cursor
                Vector2 pos;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(UI.transform as RectTransform, Input.mousePosition, Camera.main, out pos);
                draggedObject.transform.position = UI.transform.TransformPoint(pos);
            } else {
                hoveredItem = results.Find(r => r.gameObject.GetComponent<RawImage>().enabled).gameObject;
            }

            foreach (RaycastResult result in results) {
                result.gameObject.GetComponent<RawImage>().material.SetFloat("_Thickness", 0.04f);
                result.gameObject.GetComponent<RawImage>().material.SetColor("_OutlineColor", new Color(230, 230, 230, 1));
            }
            RefreshToolTip();
        }
    }

    private void RefreshToolTip() {
        UIInventoryUsageHint hint = hintTextObject.GetComponent<UIInventoryUsageHint>();
        if (hoveredItem != null) {
            hint.HoveredInventoryItem = hoveredItem.GetComponent<UIInventoryItem>().item;
        } else {
            hint.HoveredInventoryItem = null;
        }
        if (draggedObject != null) {
            hint.DraggedInventoryItem = draggedObject.GetComponent<UIInventoryItem>().item;
        } else {
            hint.DraggedInventoryItem = null;
        }
    }

    public void AddToInventory(InventoryItem item) {
        List<InventoryItem> newInventory = new List<InventoryItem>(inventoryItems);
        if (newInventory.Find(i => i.itemName == item.itemName) == null) {
            newInventory.Add(item);
            inventoryItems = newInventory.ToArray();
        }
        RefreshInventory();
    }

    public bool HasItemInInventory(InventoryItem item) {
        return new List<InventoryItem>(inventoryItems).Find(i => i.itemName == item.itemName) != null;
    }

    public bool HasItemInInventory(string itemName) {
        return new List<InventoryItem>(inventoryItems).Find(i => i.itemName == itemName) != null;
    }

    public void RemoveFromInventory(string itemName) {
        inventoryItems = new List<InventoryItem>(inventoryItems).FindAll(i => i.itemName != itemName).ToArray();
        RefreshInventory();
    }

    private void RefreshInventory() {
        foreach (Transform child in transform) {
            GameObject.Destroy(child.gameObject);
        }
        foreach (InventoryItem item in inventoryItems) {
            var inventoryItem = Instantiate(prefabInventoryItem, transform);
            inventoryItem.GetComponent<RawImage>().material = Instantiate(inventoryItem.GetComponent<RawImage>().material);
            inventoryItem.GetComponent<RawImage>().texture = item.texture as Texture;
            inventoryItem.GetComponent<UIInventoryItem>().item = item;
        }
    }

    public void ClearInventory() {
        inventoryItems = new List<InventoryItem>().ToArray();
        RefreshInventory();
    }

    public void OnPointerDown(PointerEventData eventData) {
        if (IsActive && hoveredItem != null) {
            currentInventoryTarget = hoveredItem.gameObject;
            draggedObject = Instantiate(currentInventoryTarget, UI);
            draggedObject.GetComponent<RawImage>().material.SetFloat("_Thickness", 0.04f);
            draggedObject.GetComponent<RawImage>().material.SetColor("_OutlineColor", new Color(230, 230, 230, 1));
            draggedObject.GetComponent<UIInventoryItem>().DuplicateInfoFrom(hoveredItem.GetComponent<UIInventoryItem>());
            currentInventoryTarget.GetComponent<RawImage>().enabled = false;
            player.GetComponent<PlayerController>().SetDraggedInventoryItem(hoveredItem.GetComponent<UIInventoryItem>().item);
            RefreshToolTip();
            AudioUtils.PlaySound(AudioUtils.SoundType.UIClick);
        }
    }

    public void OnPointerUp(PointerEventData eventData) {
        if (IsActive) {

            if (currentInventoryTarget != null) {
                currentInventoryTarget.GetComponent<RawImage>().enabled = true;
                currentInventoryTarget = null;
            }
            if (draggedObject != null) {
                if (hoveredItem != null) { // check for valid combination
                    InventoryItem hovered = hoveredItem.GetComponent<UIInventoryItem>().item;
                    InventoryItem dragged = draggedObject.GetComponent<UIInventoryItem>().item;
                    bool foundCombination = false;
                    SpokenLine thought = null;
                    foreach (Combination combination in hovered.combinations) {
                        if (combination.combineWith.itemName == dragged.itemName) { // valid combination
                            thought = combination.thought;
                            foundCombination = true;
                            List<InventoryItem> newInventory = new List<InventoryItem>();
                            foreach (InventoryItem prevItem in inventoryItems) {
                                if (prevItem.itemName == hovered.itemName) {
                                    newInventory.Add(combination.result);
                                } else if (prevItem.itemName != dragged.itemName) {
                                    newInventory.Add(prevItem);
                                }
                            }
                            inventoryItems = newInventory.ToArray();
                            AudioUtils.PlaySound(AudioUtils.SoundType.UIClick);
                        }
                    }
                    player.State = PlayerState.Talking;
                    if (foundCombination) {
                        RefreshInventory();
                        player.GetComponent<Speakable>().Speak(thought);
                    } else {
                        player.ProvideNegativeFeedback();
                    }
                }
                Destroy(draggedObject);
                draggedObject = null;
                player.GetComponent<PlayerController>().RemoveDraggedInventoryItem();
            }
            RefreshToolTip();
        }
    }


}
