using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AudioUtils;
using static Dialog;

public class Talkable : MonoBehaviour
{
    [field: SerializeField] public Dialog Dialog { get; private set; }
    [field: SerializeField] public bool Busy { get; set; }
    [field: SerializeField] public bool FacePlayer { get; private set; }
    [field: SerializeField] public float DistanceAboveHead { get; private set; }
    [field: SerializeField] public Music music { get; private set; }
    [SerializeField] PlayerController player;
    [SerializeField] UIDialogContainer dialogContainer;
    [SerializeField] FloatingTextManager floatingText;
    [SerializeField] CutScenePlayer cutscenePlayer;
    ConversationOutcome conversationOutcome;
    Action<ConversationOutcome> onCompleteConversation;
    Branch[] currentBranches;

    public void Start() {
        ClearVisitedFlag(Dialog.branches);
        SetBranchParent(Dialog.branches, null);
    }

    private void Update() {
        if (!cutscenePlayer.IsPlayingCutScene() && FacePlayer) {
            GetComponent<SpriteRenderer>().flipX = player.transform.position.x < transform.position.x;
        }
    }

    private void SetBranchParent(Branch[] branches, Branch parent) {
        foreach (Branch branch in branches) {
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

    internal void StartDialog(Action<ConversationOutcome> onCompleteCallback) {
        currentBranches = Dialog.branches;
        conversationOutcome = new ConversationOutcome();
        onCompleteConversation = onCompleteCallback;
        floatingText.onEmptyQueue += ShowDialogOptions;
        
        if (!String.IsNullOrEmpty(Dialog.greeting)) {
            floatingText.AddText(player.gameObject, Dialog.greeting, Dialog.fmodEvent, 0);
        }
        if (!String.IsNullOrEmpty(Dialog.reply)) {
            floatingText.AddText(gameObject, Dialog.reply, Dialog.fmodEvent, 1);
        }
        //if (AudioUtils.GetCurrentMusicPlaying() != Music.Wearhouse) { // not great but too close to deadline
        //    AudioUtils.PlayMusic(AudioUtils.Music.Dialog, Camera.main.transform.position);
        //}
    }

    public void ShowDialogOptions() {
        dialogContainer.Activate(Dialog, currentBranches, OnOptionSelected);
    }

    public void OnOptionSelected(string option) {
        Branch branch = FindBranch(option, Dialog.branches);
        if (branch.cutscene != null) {
            conversationOutcome.CutScene = branch.cutscene;
        }
        if (branch.objectGained != null) {
            conversationOutcome.ItemGained = branch.objectGained;
        }
        if (branch.question.Length != 0 && !branch.question.StartsWith("(")) {
            floatingText.AddText(player.gameObject, branch.question, Dialog.fmodEvent, branch.fmodQuestionId);
        }
        if (branch.answer.Length != 0) {
            floatingText.AddText(gameObject, branch.answer, Dialog.fmodEvent, branch.fmodAnswerId);
        }
        if (branch.reaction.Length != 0) {
            floatingText.AddText(player.gameObject, branch.reaction, Dialog.fmodEvent, branch.fmodReactionId);
        }
        if (branch.branches.Length == 0) {
            if (branch.final) {
                floatingText.onEmptyQueue -= ShowDialogOptions;
                floatingText.onEmptyQueue += CloseConversation;
            } else {
                if (branch.parent != null) {
                    currentBranches = branch.parent.branches;
                } else {
                    currentBranches = Dialog.branches;
                }
            }
            branch.visited = true; // only trigger visited when it's the last branch, then go backwards from there
            SetVisitedTree(branch);
        } else {
            currentBranches = branch.branches;
        }
    }

    private void CloseConversation() {
        floatingText.onEmptyQueue -= CloseConversation;
        onCompleteConversation(conversationOutcome);
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
