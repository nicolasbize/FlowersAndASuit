using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static InventoryItem;

public class UIInventoryManager : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{

    [SerializeField] InventoryItem[] inventoryItems;
    [SerializeField] Transform prefabInventoryItem;
    [SerializeField] Transform UI;
    [SerializeField] Transform hintTextObject;
    [SerializeField] Transform player;
    [SerializeField] string[] negativeCombinationFeedbacks;

    private int feedbackIndex = 0;
    private GameObject hoveredItem = null;
    private GameObject draggedObject = null;
    private GameObject currentInventoryTarget = null;
    public bool active = true;

    private void Start() {
        RefreshInventory();
    }

    private void Update() {
        if (active) {
            foreach (Transform child in transform) {
                child.gameObject.GetComponent<RawImage>().material.SetFloat("Thickness", 0f);
            }

            PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
            eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
            results = results.FindAll(r => r.gameObject.tag == "InventoryItem");
            if (draggedObject != null) {
                hoveredItem = results.Find(r => r.gameObject.GetComponent<RawImage>().enabled && r.gameObject.name != draggedObject.gameObject.name).gameObject;
                Vector2 pos;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(UI.transform as RectTransform, Input.mousePosition, Camera.main, out pos);
                draggedObject.transform.position = UI.transform.TransformPoint(pos);
            } else {
                hoveredItem = results.Find(r => r.gameObject.GetComponent<RawImage>().enabled).gameObject;
            }

            foreach (RaycastResult result in results) {
                result.gameObject.GetComponent<RawImage>().material.SetFloat("Thickness", 0.04f);
            }

            RefreshToolTip();
        }
    }

    private void RefreshToolTip() {
        UIInventoryUsageHint hint = hintTextObject.GetComponent<UIInventoryUsageHint>();
        if (draggedObject != null && hoveredItem != null) {
            string hoveredName = hoveredItem.GetComponent<UIInventoryItem>().item.itemName;
            hint.SetItem(draggedObject.GetComponent<UIInventoryItem>().item);
            hint.SetTarget(hoveredName);
        } else if (draggedObject == null && hoveredItem != null) {
            hint.SetItem(hoveredItem.GetComponent<UIInventoryItem>().item);
        } else if (draggedObject != null && hoveredItem == null) {
            hint.SetItem(draggedObject.GetComponent<UIInventoryItem>().item);
        } else {
            hint.SetItem(null);
            hint.SetTarget(null);
        }
        hint.SetActive(draggedObject != null);
    }

    public void AddToInventory(InventoryItem item) {
        List<InventoryItem> newInventory = new List<InventoryItem>(inventoryItems);
        if (newInventory.Find(i => i.itemName == item.itemName) == null) {
            newInventory.Add(item);
            inventoryItems = newInventory.ToArray();
        }
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

    public void OnPointerDown(PointerEventData eventData) {
        if (active && hoveredItem != null) {
            currentInventoryTarget = hoveredItem.gameObject;
            draggedObject = Instantiate(currentInventoryTarget, UI);
            draggedObject.GetComponent<RawImage>().material.SetFloat("Thickness", 0.04f);
            draggedObject.GetComponent<UIInventoryItem>().DuplicateInfoFrom(hoveredItem.GetComponent<UIInventoryItem>());
            currentInventoryTarget.GetComponent<RawImage>().enabled = false;
            player.GetComponent<PlayerController>().SetDraggedInventoryItem(hoveredItem.GetComponent<UIInventoryItem>().item);
            RefreshToolTip();
        }
    }

    public void OnPointerUp(PointerEventData eventData) {
        if (active) {

            if (currentInventoryTarget != null) {
                currentInventoryTarget.GetComponent<RawImage>().enabled = true;
                currentInventoryTarget = null;
            }
            if (draggedObject != null) {
                if (hoveredItem != null) { // check for valid combination
                    InventoryItem hovered = hoveredItem.GetComponent<UIInventoryItem>().item;
                    InventoryItem dragged = draggedObject.GetComponent<UIInventoryItem>().item;
                    bool foundCombination = false;
                    string thought = "";
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
                        }
                    }
                    if (foundCombination) {
                        RefreshInventory();
                        player.GetComponent<PlayerController>().ShowCombinationResult(thought);
                    } else {
                        feedbackIndex = (feedbackIndex + 1) % negativeCombinationFeedbacks.Length;
                        player.GetComponent<PlayerController>().ShowCombinationResult(negativeCombinationFeedbacks[feedbackIndex]);
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
