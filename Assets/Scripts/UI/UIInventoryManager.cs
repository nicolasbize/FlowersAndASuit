using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIInventoryManager : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{

    [SerializeField] InventoryItem[] inventoryitems;
    [SerializeField] Transform prefabInventoryItem;
    [SerializeField] Transform UI;
    [SerializeField] Transform hintTextObject;
    [SerializeField] Transform player;

    private GameObject draggedObject = null;
    private GameObject currentInventoryTarget = null;

    private void Start() {
        ClearInventory();
        DisplayInventory();
    }

    private void Update() {
        if (draggedObject != null) {
            Vector2 pos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(UI.transform as RectTransform, Input.mousePosition, Camera.main, out pos);
            draggedObject.transform.position = UI.transform.TransformPoint(pos);
        }
    }

    private void ClearInventory() {
        foreach (Transform child in transform) {
            GameObject.Destroy(child.gameObject);
        }
    }

    private void DisplayInventory() {
        foreach (InventoryItem item in inventoryitems) {
            var inventoryItem = Instantiate(prefabInventoryItem, transform);
            inventoryItem.GetComponent<RawImage>().material = Instantiate(inventoryItem.GetComponent<RawImage>().material);
            inventoryItem.GetComponent<RawImage>().texture = item.texture as Texture;
            inventoryItem.GetComponent<UIInventoryItem>().SetItemInfo(item, hintTextObject);

        }
    }

    public void OnPointerDown(PointerEventData eventData) {
        var objectClicked = eventData.pointerCurrentRaycast.gameObject;
        if (objectClicked.tag == "InventoryItem") {
            currentInventoryTarget = objectClicked;
            draggedObject = Instantiate(objectClicked, UI);
            draggedObject.GetComponent<UIInventoryItem>().DuplicateInfoFrom(objectClicked.GetComponent<UIInventoryItem>());
            currentInventoryTarget.GetComponent<RawImage>().enabled = false;
            player.GetComponent<PlayerController>().SetDraggedInventoryItem(objectClicked.GetComponent<UIInventoryItem>().GetItem());
        }
    }

    public void OnPointerUp(PointerEventData eventData) {
        if (currentInventoryTarget != null) {
            currentInventoryTarget.GetComponent<RawImage>().enabled = true;
            currentInventoryTarget.GetComponent<UIInventoryItem>().RemoveOutline();
            currentInventoryTarget = null;
        }
        if (draggedObject != null) {
            Destroy(draggedObject);
            draggedObject = null;
            player.GetComponent<PlayerController>().RemoveDraggedInventoryItem();
        }
    }
}
