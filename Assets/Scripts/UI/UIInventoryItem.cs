using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIInventoryItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] float maxThickness = 0.04f;
    private InventoryItem item;
    private Transform hintText;

    public void SetItemInfo(InventoryItem inventoryItem, Transform hint) {
        item = inventoryItem;
        hintText = hint;
    }

    public InventoryItem GetItem() {
        return item;
    }

    public void OnPointerEnter(PointerEventData eventData) {
        GetComponent<RawImage>().material.SetFloat("Thickness", .04f);
        hintText.GetComponent<UIInventoryUsageHint>().SetItem(item);
    }

    public void OnPointerExit(PointerEventData eventData) {
        RemoveOutline();
    }

    public void RemoveOutline() {
        GetComponent<RawImage>().material.SetFloat("Thickness", 0f);
        hintText.GetComponent<UIInventoryUsageHint>().SetItem(null);
    }

    internal void DuplicateInfoFrom(UIInventoryItem other) {
        item = other.item; // not sure if we should clone this or not
        hintText = other.hintText;
    }
}
