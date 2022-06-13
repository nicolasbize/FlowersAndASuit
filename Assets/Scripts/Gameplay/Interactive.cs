using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Dialog;

public class Interactive : MonoBehaviour
{

    public string hintText = "";
    public float distanceToInteraction = 3f;
    public string observation = "";
    public string observationAfterTakingObject = "";
    public InventoryItem itemGained;
    public bool destroyAfterPickup;
    public Dialog dialog;
    public Transform dialogContainer;
    public Transform cutsceneManager;
    public FloatingTextManager floatingTextManager;
    public float overrideThickness;
    public float overrideTextDistanceAboveHead;
    public Transform warpTo;
    public bool busy;
    GameObject currentPlayer;
    GameObject currentTarget;
    Branch[] currentBranches;
    InventoryItem itemGainedFromDialog = null;
    CutScene cutsceneToBePlayed = null;

    public void Start() {
        if (dialog != null) {
            ClearVisitedFlag(dialog.branches);
            SetBranchParent(dialog.branches, null);
        }
    }

    private void SetBranchParent(Branch[] branches, Branch parent) {
        foreach(Branch branch in branches) {
            branch.parent = parent;
            SetBranchParent(branch.branches, branch);
        }
    }

    private void ClearVisitedFlag(Branch[] branches) {
        foreach (Branch branch in branches) {
            ClearVisitedFlag(branch.branches);
            branch.visited = false;
        }
    }

    public void StartDialog(GameObject player, GameObject target) {
        currentPlayer = player;
        currentTarget = target;
        if (dialog != null) {
            currentBranches = dialog.branches;
            floatingTextManager.onEmptyQueue += ShowDialogOptions;
            currentTarget.GetComponent<Animator>().SetBool("is_talking", true);
            currentPlayer.GetComponent<Animator>().SetBool("is_talking", true);
            if (!String.IsNullOrEmpty(dialog.greeting)) {
                floatingTextManager.AddText(currentPlayer, dialog.greeting);
            }
            if (!String.IsNullOrEmpty(dialog.reply)) {
                floatingTextManager.AddText(currentTarget, dialog.reply);
            }
        } else {
            string text = observation;
            if (itemGained != null) {
                // check if the player already has the item in their inventory
                if (currentPlayer.GetComponent<PlayerController>().HasItemInInventory(itemGained)) {
                    text = observationAfterTakingObject;
                } else {
                    itemGainedFromDialog = itemGained;
                }
            }
            currentPlayer.GetComponent<Animator>().SetBool("is_talking", true);
            floatingTextManager.onEmptyQueue += FreePlayerFromConversation;
            floatingTextManager.AddText(currentPlayer, text);
        }
        
    }
    
    public void FreePlayerFromConversation() {
        floatingTextManager.onEmptyQueue -= FreePlayerFromConversation;
        currentPlayer.GetComponent<PlayerController>().SetIdle();
        if (dialog != null) {
            currentTarget.GetComponent<Animator>().SetBool("is_talking", false);
            currentTarget.GetComponent<Animator>().SetBool("mouth_open", false);
        }
        currentPlayer.GetComponent<Animator>().SetBool("is_talking", false);
        currentPlayer.GetComponent<Animator>().SetBool("mouth_open", false);
        CheckForGainedObject();
        CheckForCutScene();
        currentPlayer = null;
        currentTarget = null;
    }

    private void CheckForGainedObject() {
        if (itemGainedFromDialog != null) {
            currentPlayer.GetComponent<PlayerController>().AddToInventory(itemGainedFromDialog);
            itemGainedFromDialog = null;
            if (destroyAfterPickup) {
                Destroy(gameObject);
            }
        }
    }

    private void CheckForCutScene() {
        if (cutsceneToBePlayed != null) {
            cutsceneManager.GetComponent<CutScenePlayer>().PlayCutscene(cutsceneToBePlayed);
            cutsceneToBePlayed = null;
        }
    }

    public void ShowDialogOptions() {
        CheckForGainedObject();
        dialogContainer.gameObject.GetComponent<UIDialogContainer>().Activate(dialog, currentBranches, OnOptionSelected);
    }

    public void OnOptionSelected(string option) {
        Branch branch = FindBranch(option, dialog.branches);
        if (branch.cutscene != null) {
            cutsceneToBePlayed = branch.cutscene;
        }
        if (branch.objectGained != null) {
            itemGainedFromDialog = branch.objectGained;
        }
        if (branch.question.Length != 0 && branch.cutscene == null) {
            floatingTextManager.AddText(currentPlayer, branch.question);
            //FMODUnity.RuntimeManager.PlayOneShot("event:/SFX/Interactions/Enzo-Jim", Enzo.position, Jim.position, 3);

        }
        if (branch.answer.Length != 0) {
            floatingTextManager.AddText(currentTarget, branch.answer);
        }
        if (branch.reaction.Length != 0) {
            floatingTextManager.AddText(currentPlayer, branch.reaction);
        }
        if (branch.branches.Length == 0) {
            if (branch.final) {
                floatingTextManager.onEmptyQueue -= ShowDialogOptions;
                floatingTextManager.onEmptyQueue += FreePlayerFromConversation;
            } else {
                if (branch.parent != null) {
                    currentBranches = branch.parent.branches;
                } else {
                    currentBranches = dialog.branches;
                }
            }
            branch.visited = true; // only trigger visited when it's the last branch, then go backwards from there
            SetVisitedTree(branch);
        } else {
            currentBranches = branch.branches;
        }
    }

    private void SetVisitedTree(Branch lastVisited) {
        bool isParentFullyVisited = true;
        if (lastVisited.parent != null) {
            foreach (Branch branch in lastVisited.parent.branches) {
                if (!branch.visited) {
                    isParentFullyVisited = false;
                }
            }
            if (isParentFullyVisited) {
                lastVisited.parent.visited = true;
                SetVisitedTree(lastVisited.parent);
            }
        }
    }

    private Branch FindBranch(string option, Branch[] branches) {
        Branch targetBranch = null;
        foreach (Branch branch in branches) {
            if (branch.question == option) {
                targetBranch = branch;
            } else {
                Branch nextBranch = FindBranch(option, branch.branches);
                if (nextBranch != null) {
                    targetBranch = nextBranch;
                }
            }
        }
        return targetBranch;
    }
}
