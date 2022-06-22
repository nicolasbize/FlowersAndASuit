using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static PlayerController;

public class UIInventoryUsageHint : MonoBehaviour
{
    [SerializeField] PlayerController player;
    //[SerializeField] Game gameplayManager;

    public InventoryItem DraggedInventoryItem { get; set; }
    public InventoryItem HoveredInventoryItem { get; set; }
    public Interactive HoveredInteractive { get; set; }


    void Update()
    {
        if (ShouldDisplay()) {
            GetComponent<TextMeshProUGUI>().text = GetHintText();
        } else {
            GetComponent<TextMeshProUGUI>().text = "";
        }
    }

    bool ShouldDisplay() {
        return GameInitializer.Instance.IsReady && !CutSceneManager.Instance.IsPlayingCutScene();
    }

    string GetHintText() {
        string currentHint = "";
        if (HoveredInteractive != null && DraggedInventoryItem != null) {
            currentHint = "Use " + DraggedInventoryItem.itemName + " on " + HoveredInteractive.HintText;
        } else if (HoveredInventoryItem != null && DraggedInventoryItem != null) {
            currentHint = "Combine " + DraggedInventoryItem.itemName + " with " + HoveredInventoryItem.itemName;
        } else if (HoveredInventoryItem != null && DraggedInventoryItem == null && HoveredInteractive == null) {
            currentHint = HoveredInventoryItem.itemName;
        } else if (HoveredInteractive != null && HoveredInventoryItem == null && DraggedInventoryItem == null) {
            currentHint = HoveredInteractive.HintText;
        }
        return currentHint;
    }
}
