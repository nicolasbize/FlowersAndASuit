using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Dialog;

public class DialogManager : MonoBehaviour
{
    [SerializeField] float timeBetweenConversations = 0.3f;
    [SerializeField] UIDialogContainer dialogOptionsPanel;

    public static DialogManager Instance { get; private set; }
    void Awake() {
        if (Instance != null) {
            GameObject.Destroy(Instance);
        } else {
            Instance = this;
        }
        DontDestroyOnLoad(this);
    }

    Dialog currentDialog;
    Speakable currentInquirer;
    Speakable currentResponder;
    Queue<SingleDialogText> messageQueue = new Queue<SingleDialogText>();
    Branch[] currentBranches;
    ConversationOutcome conversationOutcome;
    bool hasCompletedConversation;
    Action<ConversationOutcome> onCompleteConversation;


    public bool InConversation() {
        return currentDialog != null;
    }

    public void StartConversation(Dialog dialog, Speakable inquirer, Speakable responder, Action<ConversationOutcome> onCompleteCallback = null) {
        currentDialog = dialog;
        currentBranches = dialog.branches;
        currentInquirer = inquirer;
        currentResponder = responder;
        conversationOutcome = new ConversationOutcome();
        hasCompletedConversation = false;
        onCompleteConversation = onCompleteCallback;
        messageQueue.Clear();
        DialogTreeUtils.ClearVisitedFlag(dialog.branches);
        DialogTreeUtils.SetBranchParent(dialog.branches, null);
        if (!String.IsNullOrEmpty(dialog.greeting)) {
            messageQueue.Enqueue(new SingleDialogText() {
                speaker = inquirer,
                text = dialog.greeting,
                fmodEvent = dialog.fmodEvent,
                fmodId = 0
            });
        }
        if (!String.IsNullOrEmpty(dialog.reply)) {
            messageQueue.Enqueue(new SingleDialogText() {
                speaker = responder,
                text = dialog.reply,
                fmodEvent = dialog.fmodEvent,
                fmodId = 1
            });
        }
        StartCoroutine(PursueConversation());
    }

    IEnumerator PursueConversation(float waitTime = 0f) {
        yield return new WaitForSeconds(waitTime);
        if (messageQueue.Count > 0) {
            SingleDialogText voice = messageQueue.Dequeue();
            SpokenLine line = new SpokenLine(voice.text, voice.fmodId);
            voice.speaker.Speak(line, voice.fmodEvent, () => {
                StartCoroutine(PursueConversation(timeBetweenConversations));
            });
        } else {
            if (hasCompletedConversation) {
                currentDialog = null;
                if (conversationOutcome.ItemGained != null) {
                    InventoryManager.Instance.AddToInventory(conversationOutcome.ItemGained);
                }
                if (conversationOutcome.CutScene != null) {
                    //cutscenePlayer.PlayCutscene(outcome.CutScene);
                }
            } else {
                dialogOptionsPanel.Activate(currentDialog, currentBranches, OnDialogOptionSelected);
            }
        }
    }

    private void OnDialogOptionSelected(string option) {
        Branch branch = DialogTreeUtils.FindBranch(option, currentDialog.branches);
        if (branch.cutscene != null) {
            conversationOutcome.CutScene = branch.cutscene;
        }
        if (branch.objectGained != null) {
            conversationOutcome.ItemGained = branch.objectGained;
        }
        if (branch.question.Length != 0 && !branch.question.StartsWith("(")) {
            messageQueue.Enqueue(new SingleDialogText() {
                speaker = currentInquirer,
                text = branch.question,
                fmodEvent = currentDialog.fmodEvent,
                fmodId = branch.fmodQuestionId
            });
        }
        if (branch.answer.Length != 0) {
            messageQueue.Enqueue(new SingleDialogText() {
                speaker = currentResponder,
                text = branch.answer,
                fmodEvent = currentDialog.fmodEvent,
                fmodId = branch.fmodAnswerId
            });
        }
        if (branch.reaction.Length != 0) {
            messageQueue.Enqueue(new SingleDialogText() {
                speaker = currentInquirer,
                text = branch.reaction,
                fmodEvent = currentDialog.fmodEvent,
                fmodId = branch.fmodReactionId
            });
        }
        if (branch.branches.Length == 0) {
            if (branch.final) {
                hasCompletedConversation = true;
                //floatingText.onEmptyQueue -= ShowDialogOptions;
                //floatingText.onEmptyQueue += CloseConversation;
            } else {
                if (branch.parent != null) {
                    currentBranches = branch.parent.branches;
                } else {
                    currentBranches = currentDialog.branches;
                }
            }
            branch.visited = true; // only trigger visited when it's the last branch, then go backwards from there
            DialogTreeUtils.SetVisitedTree(branch);
        } else {
            currentBranches = branch.branches;
        }
        StartCoroutine(PursueConversation()); // no need to wait to chat after clicking on an option
    }
}
