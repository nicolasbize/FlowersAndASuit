using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIInventoryItem : MonoBehaviour
{
    [SerializeField] float maxThickness = 0.04f;
    public InventoryItem item;

    internal void DuplicateInfoFrom(UIInventoryItem other) {
        item = other.item; // not sure if we should clone this or not
    }

}
