using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIInventoryUsageHint : MonoBehaviour
{

    private InventoryItem item;
    private string targetName;

    public void SetItem(InventoryItem inventoryItem) {
        item = inventoryItem;
    }

    public void SetTarget(string name) {
        targetName = name;
    }

    // Update is called once per frame
    void Update()
    {
        string currentHint = "";
        if (item != null) {
            currentHint = "Use " + item.itemName;
        }
        if (targetName != null) {
            currentHint += " on " + targetName;
        }
        GetComponent<TextMeshProUGUI>().text = currentHint;
    }
}
