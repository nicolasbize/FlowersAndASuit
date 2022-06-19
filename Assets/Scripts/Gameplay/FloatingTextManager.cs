using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static AudioUtils;
using static Dialog;

public class FloatingTextManager : MonoBehaviour
{
    public Transform textElement;
    bool isShowing = false;
    bool isMouthOpen = false;
    GameObject currentTarget = null;
    Queue<SingleDialogText> messageQueue = new Queue<SingleDialogText>();

    public delegate void OnQueueEmpty();
    public event OnQueueEmpty onEmptyQueue;

    public void Start() {
        Invoke("MoveMouth", 0f);
    }

    public void AddText(GameObject target, SpokenLine line, AudioUtils.DialogConversation fmodEvent) {
        AddText(target, line.Text, fmodEvent, line.FmodId);
    }

    public void AddText(GameObject target, string text, AudioUtils.DialogConversation fmodEvent = AudioUtils.DialogConversation.None, int fmodId = 0) {
        string[] phrases = text.Split(new[] { "   " }, StringSplitOptions.None);
        for (int i=0; i<phrases.Length; i++) {
            if (phrases[i].Trim().Length > 0) {
                SingleDialogText sentence = new SingleDialogText();
                sentence.speaker = target;
                sentence.text = phrases[i];
                if (i == 0) { // even though we break down long sentences into several displayed strings, we only have a single audio event
                    sentence.fmodEvent = fmodEvent;
                    sentence.fmodId = fmodId;
                }
                messageQueue.Enqueue(sentence);
            }
        }
    }

    public bool HasEnquedMessagesForOtherThan(GameObject target) {
        return new List<SingleDialogText>(messageQueue.ToArray()).Find(t => t.speaker.name != target.name) != null;
    }

    public bool HasMessagesInQueue() {
        return messageQueue.Count > 0;
    }

    public int NbMessagesInQueue() {
        return messageQueue.Count;
    }

    public void RemoveMessagesFor(GameObject target) {
        Queue<SingleDialogText> newQueue = new Queue<SingleDialogText>();
        while (messageQueue.Count > 0) {
            SingleDialogText message = messageQueue.Dequeue();
            if (message.speaker.name != target.name) {
                newQueue.Enqueue(message);
            }
        }
        messageQueue = newQueue;
    }


    private void Update() {
        if (!isShowing && messageQueue.Count > 0) {
            StartCoroutine(ShowMessage(messageQueue.Dequeue()));
        }
        // FIXME: this forces every animator to have a mouth_open property, even animated objects in the game.
        if (isShowing) {
            currentTarget.GetComponent<Animator>().SetBool("mouth_open", isMouthOpen);
        }

    }

    IEnumerator ShowMessage(SingleDialogText message) {
        textElement.GetComponent<TextMeshPro>().text = message.text;
        float additionalDistanceAboveHead = 0f;
        if (message.speaker.GetComponent<Talkable>() != null) {
            additionalDistanceAboveHead = message.speaker.GetComponent<Talkable>().DistanceAboveHead;
        }
        float distAbove = message.speaker.GetComponent<SpriteRenderer>().sprite.bounds.extents.y * 2 + additionalDistanceAboveHead;
        textElement.position = message.speaker.transform.position + Vector3.up * distAbove;
        textElement.parent = message.speaker.transform;
        textElement.GetComponent<MeshRenderer>().enabled = true;
        isShowing = true;

        currentTarget = message.speaker;
        currentTarget.GetComponent<Animator>().SetBool("is_talking", true);
        if (message.fmodEvent != AudioUtils.DialogConversation.None) {
            AudioUtils.PlayDialog(message.fmodEvent, message.fmodId);
        }
        float waitTime = Mathf.Max(2, message.text.Split(' ').Length / 1.8f);
        yield return new WaitForSeconds(waitTime);
        currentTarget.GetComponent<Animator>().SetBool("is_talking", false);
        currentTarget.GetComponent<Animator>().SetBool("mouth_open", false);
        isShowing = false;
        textElement.GetComponent<MeshRenderer>().enabled = false;
        if (messageQueue.Count == 0 && onEmptyQueue != null) {
            onEmptyQueue();
            currentTarget = null;
        }
    }

    void MoveMouth() {
        isMouthOpen = !isMouthOpen;
        float randomTime = UnityEngine.Random.Range(0.1f, 0.3f);
        Invoke("MoveMouth", randomTime);
    }

}
