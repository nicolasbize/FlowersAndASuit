using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIInventoryUsageHint : MonoBehaviour
{
    [SerializeField] Transform player = null;
    private bool active = true;
    private InventoryItem item;
    private string targetName;


    public void SetItem(InventoryItem inventoryItem) {
        item = inventoryItem;
    }

    public void SetActive(bool active) {
        this.active = active;
    }

    public void SetTarget(string name) {
        targetName = name;
    }

    // Update is called once per frame
    void Update()
    {
        if (player.GetComponent<PlayerController>().GetState() == PlayerController.State.Idle) {
            string currentHint = "";
            if (item != null && targetName != null) {
                currentHint = "Use " + item.itemName + " on " + targetName;
            } else if (targetName != null) {
                currentHint = targetName;
            } else if (item != null) {
                currentHint = (active ? "Use " : "") + item.itemName;
            }
            GetComponent<TextMeshProUGUI>().text = currentHint;
        } else {
            GetComponent<TextMeshProUGUI>().text = "";
        }
    }
}
