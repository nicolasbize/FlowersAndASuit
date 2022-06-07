using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Dialog;

public class Interactive : MonoBehaviour
{

    public float distanceToInteraction = 3f;
    public string observation = "";
    public Dialog dialog;
    public Transform dialogContainer;
    public FloatingTextManager floatingTextManager;
    GameObject currentPlayer;
    GameObject currentTarget;
    Branch[] currentBranches;

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
            floatingTextManager.AddText(currentPlayer, dialog.greeting);
            floatingTextManager.AddText(currentTarget, dialog.reply);
        } else {
            floatingTextManager.onEmptyQueue += FreePlayerFromConversation;
            floatingTextManager.AddText(currentPlayer, observation);
        }
        
    }
    
    public void FreePlayerFromConversation() {
        floatingTextManager.onEmptyQueue -= FreePlayerFromConversation;
        currentPlayer.GetComponent<PlayerController>().SetIdle();
        currentPlayer = null;
        currentTarget = null;
    }

    public void ShowDialogOptions() {
        dialogContainer.gameObject.GetComponent<DialogContainer>().Activate(dialog, currentBranches, OnOptionSelected);
    }

    public void OnOptionSelected(string option) {
        Branch branch = FindBranch(option, dialog.branches);
        if (branch.question.Length != 0) {
            floatingTextManager.AddText(currentPlayer, branch.question);
        }
        if (branch.answer.Length != 0) {
            floatingTextManager.AddText(currentTarget, branch.answer);
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
