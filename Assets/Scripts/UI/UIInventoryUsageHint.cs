using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIInventoryUsageHint : MonoBehaviour
{
    [SerializeField] Transform player = null;

    public InventoryItem DraggedInventoryItem { get; set; }
    public InventoryItem HoveredInventoryItem { get; set; }
    public Interactive HoveredInteractive { get; set; }


    void Update()
    {
        if (player.GetComponent<PlayerController>().GetState() == PlayerController.State.Idle) {
            string currentHint = "";
            if (HoveredInteractive != null && DraggedInventoryItem != null) {
                currentHint = "Use " + DraggedInventoryItem.itemName + " on " + HoveredInteractive.hintText;
            } else if (HoveredInventoryItem != null && DraggedInventoryItem != null) {
                currentHint = "Combine " + DraggedInventoryItem.itemName + " with " + HoveredInventoryItem.itemName;
            } else if (HoveredInventoryItem != null && DraggedInventoryItem == null && HoveredInteractive == null) {
                currentHint = HoveredInventoryItem.itemName;
            } else if (HoveredInteractive != null && HoveredInventoryItem == null && DraggedInventoryItem == null) {
                currentHint = HoveredInteractive.hintText;
            }

            GetComponent<TextMeshProUGUI>().text = currentHint;
        } else {
            GetComponent<TextMeshProUGUI>().text = "";
        }
    }
}
