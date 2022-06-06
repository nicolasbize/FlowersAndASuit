using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactive : MonoBehaviour
{

    public Dialog dialog;
    public Transform dialogContainer;
    public FloatingTextManager floatingTextManager;
    
    public void StartDialog(GameObject player, GameObject target) {
        floatingTextManager.AddText(player, dialog.greeting);
        floatingTextManager.AddText(target, dialog.reply);
        floatingTextManager.onEmptyQueue += ShowDialogOptions;
    }

    public void ShowDialogOptions() {
        dialogContainer.gameObject.GetComponent<DialogContainer>().Activate(dialog);
    }
}
